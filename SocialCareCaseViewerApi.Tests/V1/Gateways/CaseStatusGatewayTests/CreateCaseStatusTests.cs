using System;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using SocialCareCaseViewerApi.V1.Gateways;
using SocialCareCaseViewerApi.Tests.V1.Helpers;
using Microsoft.EntityFrameworkCore;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using System.Collections.Generic;
using Moq;
using SocialCareCaseViewerApi.V1.Helpers;

namespace SocialCareCaseViewerApi.Tests.V1.Gateways.CaseStatusGatewayTests
{
    [TestFixture]
    public class CreateCaseStatusTests : DatabaseTests
    {
        private CaseStatusGateway _caseStatusGateway;
        private Mock<ISystemTime> _mockSystemTime;

        [SetUp]
        public void Setup()
        {
            _mockSystemTime = new Mock<ISystemTime>();
            _caseStatusGateway = new CaseStatusGateway(DatabaseContext, _mockSystemTime.Object);
            DatabaseContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        }

        [Test]
        public void CreatesACaseStatus()
        {
            var person = TestHelpers.CreatePerson();
            DatabaseContext.Persons.Add(person);
            DatabaseContext.SaveChanges();

            var requestField = new List<CaseStatusRequestField>() { new CaseStatusRequestField() { Name = "reason", Selected = "N0" } };
            var request = CaseStatusHelper.CreateCaseStatusRequest(person.Id, fields: requestField);

            _caseStatusGateway.CreateCaseStatus(request);

            var caseStatus = DatabaseContext.CaseStatuses.FirstOrDefault();

            caseStatus?.PersonId.Should().Be(request.PersonId);
            caseStatus?.Answers.Count.Should().Be(1);
            caseStatus?.Answers.FirstOrDefault()?.Answer.Should().Be("N0");
            caseStatus?.Notes.Should().Be(request.Notes);
        }

        [Test]
        public void CreatesACaseStatusWithoutAnswers()
        {
            var person = TestHelpers.CreatePerson();
            DatabaseContext.Persons.Add(person);
            DatabaseContext.SaveChanges();

            var request = CaseStatusHelper.CreateCaseStatusRequest(person.Id, fields: new List<CaseStatusRequestField>());

            _caseStatusGateway.CreateCaseStatus(request);

            var caseStatus = DatabaseContext.CaseStatuses.FirstOrDefault();

            caseStatus?.PersonId.Should().Be(request.PersonId);
        }

        [Test]
        public void SetsStartDateToNow()
        {
            var fakeTime = new DateTime(2000, 1, 1, 15, 30, 0);

            var person = TestHelpers.CreatePerson();
            DatabaseContext.Persons.Add(person);
            DatabaseContext.SaveChanges();

            var requestField = new List<CaseStatusRequestField>() { new CaseStatusRequestField() { Name = "reason", Selected = "N0" } };
            var request = CaseStatusHelper.CreateCaseStatusRequest(personId: person.Id, fields: requestField, startDate: fakeTime);

            _caseStatusGateway.CreateCaseStatus(request);

            var caseStatus = DatabaseContext.CaseStatuses.FirstOrDefault();
            caseStatus?.StartDate.Should().Be(fakeTime);
        }

        [Test]
        public void AuditsTheCaseStatus()
        {
            var person = TestHelpers.CreatePerson();
            DatabaseContext.Persons.Add(person);
            DatabaseContext.SaveChanges();

            var requestField = new List<CaseStatusRequestField> { new CaseStatusRequestField() { Name = "reason", Selected = "N0" } };
            var request = CaseStatusHelper.CreateCaseStatusRequest(person.Id, fields: requestField);

            _caseStatusGateway.CreateCaseStatus(request);

            var caseStatus = DatabaseContext.CaseStatuses.FirstOrDefault();
            caseStatus?.CreatedAt.Should().NotBeNull();
            caseStatus?.CreatedBy.Should().Be(request.CreatedBy);
        }
    }
}
