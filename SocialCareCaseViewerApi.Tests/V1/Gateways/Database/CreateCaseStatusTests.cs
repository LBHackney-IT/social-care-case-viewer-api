using System;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using SocialCareCaseViewerApi.V1.Gateways;
using SocialCareCaseViewerApi.Tests.V1.Helpers;
using Microsoft.EntityFrameworkCore;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using System.Collections.Generic;

namespace SocialCareCaseViewerApi.Tests.V1.Gateways.Database
{
    [TestFixture]
    public class CreateCaseStatusTests : DatabaseTests
    {
        private CaseStatusGateway _caseStatusGateway;

        [SetUp]
        public void Setup()
        {
            _caseStatusGateway = new CaseStatusGateway(DatabaseContext);
            DatabaseContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        }

        [Test]
        public void CreatesACaseStatus()
        {
            var (caseStatusType, _, caseOptions) = CaseStatusHelper.SaveCaseStatusFieldsToDatabase(DatabaseContext);

            var person = TestHelpers.CreatePerson();
            DatabaseContext.Persons.Add(person);
            DatabaseContext.SaveChanges();

            var requestField = new List<CaseStatusRequestField>() { new CaseStatusRequestField() { Name = "reason", Selected = "N0" } };
            var request = CaseStatusHelper.CreateCaseStatusRequest(personId: person.Id, type: caseStatusType.Name, fields: requestField);

            _caseStatusGateway.CreateCaseStatus(request);

            var caseStatus = DatabaseContext.CaseStatuses.Include(x => x.SelectedOptions).FirstOrDefault();

            caseStatus?.PersonId.Should().Be(request.PersonId);
            caseStatus?.TypeId.Should().Be(caseStatusType.Id);
            caseStatus?.SelectedOptions[0].FieldOptionId.Should().Be(caseOptions.Id);
        }

        [Test]
        public void SetsStartDateToNow()
        {
            var fakeTime = new DateTime(2000, 1, 1, 15, 30, 0);

            var person = TestHelpers.CreatePerson();
            DatabaseContext.Persons.Add(person);
            DatabaseContext.SaveChanges();

            var (caseStatusType, _, _) = CaseStatusHelper.SaveCaseStatusFieldsToDatabase(DatabaseContext);
            var requestField = new List<CaseStatusRequestField>() { new CaseStatusRequestField() { Name = "reason", Selected = "N0" } };
            var request = CaseStatusHelper.CreateCaseStatusRequest(personId: person.Id, type: caseStatusType.Name, fields: requestField, startDate: fakeTime);

            _caseStatusGateway.CreateCaseStatus(request);

            var caseStatus = DatabaseContext.CaseStatuses.Include(x => x.SelectedOptions).FirstOrDefault();
            caseStatus?.StartDate.Should().Be(fakeTime);
        }

        [Test]
        public void AuditsTheCaseStatus()
        {
            var person = TestHelpers.CreatePerson();
            DatabaseContext.Persons.Add(person);
            DatabaseContext.SaveChanges();

            var (caseStatusType, _, _) = CaseStatusHelper.SaveCaseStatusFieldsToDatabase(DatabaseContext);
            var requestField = new List<CaseStatusRequestField> { new CaseStatusRequestField() { Name = "reason", Selected = "N0" } };
            var request = CaseStatusHelper.CreateCaseStatusRequest(person.Id, caseStatusType.Name, requestField);

            _caseStatusGateway.CreateCaseStatus(request);

            var caseStatus = DatabaseContext.CaseStatuses.Include(x => x.SelectedOptions).FirstOrDefault();
            caseStatus?.CreatedAt.Should().NotBeNull();
            caseStatus?.CreatedBy.Should().Be(request.CreatedBy);
        }
    }
}
