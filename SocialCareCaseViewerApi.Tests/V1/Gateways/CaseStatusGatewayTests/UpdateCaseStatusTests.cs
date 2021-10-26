using FluentAssertions;
using Moq;
using NUnit.Framework;
using SocialCareCaseViewerApi.Tests.V1.Helpers;
using SocialCareCaseViewerApi.V1.Exceptions;
using SocialCareCaseViewerApi.V1.Gateways;
using SocialCareCaseViewerApi.V1.Helpers;
using SocialCareCaseViewerApi.V1.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SocialCareCaseViewerApi.Tests.V1.Gateways.CaseStatusGatewayTests
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
        public void WhenACaseStatusIsNotFoundItThrowsAnException()
        {
            var request = TestHelpers.CreateUpdateCaseStatusRequest();
            request.CaseStatusId = 1L;

            Action act = () => _caseStatusGateway.UpdateCaseStatus(request);

            act.Should().Throw<CaseStatusDoesNotExistException>()
            .WithMessage($"Case status with {request.CaseStatusId} not found");
        }

        [Test]
        public void WhenCaseStatusHasEndDateAlreadyItThrowsAnException()
        {
            var request = TestHelpers.CreateUpdateCaseStatusRequest();
            var (caseStatus, _, _) = CaseStatusHelper.SavePersonWithCaseStatusToDatabase(DatabaseContext);
            DatabaseContext.SaveChanges();
            request.CaseStatusId = caseStatus.Id;

            Action act = () => _caseStatusGateway.UpdateCaseStatus(request);

            act.Should().Throw<CaseStatusAlreadyClosedException>()
                .WithMessage($"Case status with {request.CaseStatusId} has already been closed.");
        }

        //CIN
        [Test]
        public void WhenValidEndDateIsProvidedAndTheTypeIsCINitUpdatesTheStatusWithNewEndDateWithoutSettingEndDateOrDiscardedAtForAnswers()
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

            updatedCaseStatus.Answers.Count.Should().Be(1);
            updatedCaseStatus.Answers.First().EndDate.Should().BeNull();
            updatedCaseStatus.Answers.First().DiscardedAt.Should().BeNull();
        }

        //CP
        [Test]
        public void WhenValidEndDateIsProvidedAndTheTypeIsCPitUpdatesTheStatusWithNewEndDateWithoutAddingAnswers()
        {
            var request = TestHelpers.CreateUpdateCaseStatusRequest();

            var (caseStatus, _, _) = CaseStatusHelper.SavePersonWithCaseStatusToDatabase(DatabaseContext);
            var answer = TestHelpers.CreateCaseStatusAnswers(min: 1, max: 1).FirstOrDefault();
            caseStatus.Answers = null;
            caseStatus.Type = "CP";
            caseStatus.EndDate = null;

            DatabaseContext.SaveChanges();
            request.CaseStatusId = caseStatus.Id;

            _caseStatusGateway.UpdateCaseStatus(request);

            var updatedCaseStatus = DatabaseContext.CaseStatuses.FirstOrDefault(x => x.Id == caseStatus.Id);

            updatedCaseStatus.EndDate.Should().NotBeNull();

            updatedCaseStatus.Answers.Count.Should().Be(0);
        }

        //LAC
        [Test]
        public void WhenValidEndDateIsProvidedAndTheTypeIsLACitUpdatesTheStatusAndTheCurrentActiveAnswersWithEndDate()
        {
            var request = TestHelpers.CreateUpdateCaseStatusRequest();

            var (caseStatus, _, _) = CaseStatusHelper.SavePersonWithCaseStatusToDatabase(DatabaseContext);

            var answers = TestHelpers.CreateCaseStatusAnswers(min: 2, max: 2, endDate: null, discardedAt: null, groupId: Guid.NewGuid().ToString());

            caseStatus.Answers = answers;
            caseStatus.Type = "LAC";
            caseStatus.EndDate = null;            

            DatabaseContext.SaveChanges();
            request.CaseStatusId = caseStatus.Id;

            _caseStatusGateway.UpdateCaseStatus(request);

            var updatedCaseStatus = DatabaseContext.CaseStatuses.FirstOrDefault(x => x.Id == caseStatus.Id);

            updatedCaseStatus.EndDate.Should().NotBeNull();

            updatedCaseStatus.Answers.Count.Should().Be(2);
            updatedCaseStatus.Answers.All(x => x.EndDate != null).Should().BeTrue();
            updatedCaseStatus.Answers.All(x => x.DiscardedAt == null).Should().BeTrue();
        }

        //LAC
        [Test]

        public void WhenValidEndDateIsProvidedAndTheTypeIsLACAndThereAreScheduledAnswersItUpdatesTheStatusAndTheCurrentActiveAnswersWithEndDateAndSetsDiscardedDateToTheScheduledAnswers()
        {
            var activeGroupId = Guid.NewGuid().ToString();
            var scheduledGroupId = Guid.NewGuid().ToString();

            var request = TestHelpers.CreateUpdateCaseStatusRequest();

            var (caseStatus, _, _) = CaseStatusHelper.SavePersonWithCaseStatusToDatabase(DatabaseContext);

            var currentActiveAnswers = TestHelpers.CreateCaseStatusAnswers(min: 2, max: 2, endDate: null, discardedAt: null, groupId: activeGroupId);
            var scheduledAnswers = TestHelpers.CreateCaseStatusAnswers(min: 2, max: 2, endDate: null, discardedAt: null, startDate: DateTime.Today.AddDays(1), groupId: scheduledGroupId);

            caseStatus.Answers = new List<CaseStatusAnswer>();
            caseStatus.Answers.AddRange(currentActiveAnswers);
            caseStatus.Answers.AddRange(scheduledAnswers);
            caseStatus.Type = "LAC";
            caseStatus.EndDate = null;

            DatabaseContext.SaveChanges();
            request.CaseStatusId = caseStatus.Id;

            _caseStatusGateway.UpdateCaseStatus(request);

            var updatedCaseStatus = DatabaseContext.CaseStatuses.FirstOrDefault(x => x.Id == caseStatus.Id);

            updatedCaseStatus.EndDate.Should().NotBeNull();

            updatedCaseStatus.Answers.Count.Should().Be(4);

            updatedCaseStatus.Answers.Where(x => x.GroupId == activeGroupId).All(x => x.EndDate != null).Should().BeTrue();
            updatedCaseStatus.Answers.Where(x => x.GroupId == activeGroupId).All(x => x.DiscardedAt == null).Should().BeTrue();

            updatedCaseStatus.Answers.Where(x => x.GroupId == scheduledGroupId).All(x => x.EndDate == null).Should().BeTrue();
            updatedCaseStatus.Answers.Where(x => x.GroupId == scheduledGroupId).All(x => x.DiscardedAt != null).Should().BeTrue();
        }
    }
}
