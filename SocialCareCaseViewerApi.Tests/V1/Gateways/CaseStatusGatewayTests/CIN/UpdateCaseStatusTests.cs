using FluentAssertions;
using Moq;
using NUnit.Framework;
using SocialCareCaseViewerApi.Tests.V1.Helpers;
using SocialCareCaseViewerApi.V1.Gateways;
using SocialCareCaseViewerApi.V1.Helpers;
using SocialCareCaseViewerApi.V1.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SocialCareCaseViewerApi.Tests.V1.Gateways.CaseStatusGatewayTests.CIN
{
    [TestFixture]
    public class UpdateCaseStatusTests : DatabaseTests
    {
        private CaseStatusGateway _caseStatusGateway;
        private Mock<ISystemTime> _mockSystemTime;

        [SetUp]
        public void Setup()
        {
            _mockSystemTime = new Mock<ISystemTime>();
            _caseStatusGateway = new CaseStatusGateway(DatabaseContext, _mockSystemTime.Object);
        }

        [Test]
        public void WhenTypeIsCINAndValidEndDateIsProvidedItUpdatesTheStatusWithNewEndDateWithoutSettingEndDateOrDiscardedAtForAnswers()
        {
            var request = TestHelpers.CreateUpdateCaseStatusRequest();

            var (caseStatus, _, _) = CaseStatusHelper.SavePersonWithCaseStatusToDatabase(DatabaseContext);
            var answer = TestHelpers.CreateCaseStatusAnswers(min: 1, max: 1).FirstOrDefault();
            caseStatus.Answers = new List<CaseStatusAnswer>() { answer };
            caseStatus.Type = "CIN";
            caseStatus.EndDate = null;

            DatabaseContext.SaveChanges();
            request.CaseStatusId = caseStatus.Id;

            _caseStatusGateway.UpdateCaseStatus(request);

            var updatedCaseStatus = DatabaseContext.CaseStatuses.FirstOrDefault(x => x.Id == caseStatus.Id);

            updatedCaseStatus.EndDate.Should().NotBeNull();
            updatedCaseStatus.LastModifiedBy.Should().Be(request.EditedBy);

            updatedCaseStatus.Answers.Count.Should().Be(1);
            updatedCaseStatus.Answers.First().EndDate.Should().BeNull();
            updatedCaseStatus.Answers.First().DiscardedAt.Should().BeNull();
        }

        [Test]
        public void WhenTypeIsCINAndEndDateIsNotProvidedAndStartDateIsProvidedItUpdatesTheStartDate()
        {
            var request = TestHelpers.CreateUpdateCaseStatusRequest();
            var (caseStatus, _, _) = CaseStatusHelper.SavePersonWithCaseStatusToDatabase(DatabaseContext);

            caseStatus.Type = "CIN";
            caseStatus.EndDate = null;

            DatabaseContext.SaveChanges();

            request.CaseStatusId = caseStatus.Id;
            request.StartDate = DateTime.Today.AddDays(-1);
            request.EndDate = null;

            _caseStatusGateway.UpdateCaseStatus(request);

            var updatedCaseStatus = DatabaseContext.CaseStatuses.FirstOrDefault(x => x.Id == caseStatus.Id);
            updatedCaseStatus.StartDate.Should().Be((DateTime) request.StartDate);
            updatedCaseStatus.LastModifiedBy.Should().Be(request.EditedBy);
        }

        [Test]
        public void WhenTypeIsCINAndEndDateAndStartDateAreNotProvidedItDoesNotUpdateOtherDataColumnsThanTheNote()
        {
            var (caseStatus, _, _) = CaseStatusHelper.SavePersonWithCaseStatusToDatabase(DatabaseContext);

            caseStatus.Type = "CIN";
            caseStatus.EndDate = null;
            DatabaseContext.SaveChanges();

            var request = TestHelpers.CreateUpdateCaseStatusRequest();
            request.EndDate = null;
            request.StartDate = null;
            request.Notes = "New note";
            request.CaseStatusId = caseStatus.Id;

            _caseStatusGateway.UpdateCaseStatus(request);

            var updatedCaseStatus = DatabaseContext.CaseStatuses.FirstOrDefault(x => x.Id == caseStatus.Id);

            updatedCaseStatus.PersonId.Should().Be(caseStatus.PersonId);
            updatedCaseStatus.EndDate.Should().Be(caseStatus.EndDate);
            updatedCaseStatus.Type.Should().Be(caseStatus.Type);
            updatedCaseStatus.StartDate.Should().Be(caseStatus.StartDate);
            updatedCaseStatus.Notes.Should().Be(request.Notes);
            updatedCaseStatus.LastModifiedBy.Should().Be(request.EditedBy);
        }

        [Test]
        public void WhenTypeIsCINAndEndDateAndStartDateAreNotProvidedAndTheProvidedNoteIsEmptyItUpdatesTheCurrentNoteWithEmptyValue()
        {
            var (caseStatus, _, _) = CaseStatusHelper.SavePersonWithCaseStatusToDatabase(DatabaseContext);

            caseStatus.Type = "CIN";
            caseStatus.EndDate = null;
            DatabaseContext.SaveChanges();

            var request = TestHelpers.CreateUpdateCaseStatusRequest();
            request.EndDate = null;
            request.StartDate = null;
            request.Notes = "";
            request.CaseStatusId = caseStatus.Id;

            _caseStatusGateway.UpdateCaseStatus(request);

            var updatedCaseStatus = DatabaseContext.CaseStatuses.FirstOrDefault(x => x.Id == caseStatus.Id);

            updatedCaseStatus.Notes.Should().Be(request.Notes);
            updatedCaseStatus.LastModifiedBy.Should().Be(request.EditedBy);
        }
    }
}
