using FluentAssertions;
using Moq;
using NUnit.Framework;
using SocialCareCaseViewerApi.Tests.V1.Helpers;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
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

            updatedCaseStatus.Answers.Count.Should().Be(1);
            updatedCaseStatus.Answers.First().EndDate.Should().BeNull();
            updatedCaseStatus.Answers.First().DiscardedAt.Should().BeNull();
        }

        //CP
        [Test]
        public void WhenTypeIsCPAndValidEndDateIsProvidedItUpdatesTheStatusWithNewEndDateWithoutAddingAnswers()
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
        public void WhenTypeIsLACAndValidEndDateIsProvidedItUpdatesTheStatusAndTheCurrentActiveAnswersWithEndDate()
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
        public void WhenTypeIsLACAndValidEndDateIsProvidedAndThereAreScheduledAnswersItUpdatesTheStatusAndTheCurrentActiveAnswersWithEndDateAndSetsDiscardedDateToTheScheduledAnswers()
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

        //tests when end date is not provided
        //CIN
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
        }

        //CIN
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
        }

        //CP
        [Test]
        public void WhenTypeIsCPandEndDateIsNotProvidedStartDateIsUpdatedToProvidedStartDate()
        {
            var (caseStatus, _, _) = CaseStatusHelper.SavePersonWithCaseStatusToDatabase(DatabaseContext);

            caseStatus.Type = "CP";
            caseStatus.EndDate = null;
            DatabaseContext.SaveChanges();

            var request = TestHelpers.CreateUpdateCaseStatusRequest();
            request.EndDate = null;
            request.CaseStatusId = caseStatus.Id;
            request.StartDate = DateTime.Today.AddDays(-1).Date;

            _caseStatusGateway.UpdateCaseStatus(request);

            var updatedCaseStatus = DatabaseContext.CaseStatuses.FirstOrDefault(x => x.Id == caseStatus.Id);
            updatedCaseStatus.StartDate.Date.Should().Be((DateTime) request.StartDate?.Date);
        }

        [Test]
        public void WhenTypeIsCPandValidUpdateRequestIsReceivedItSetsTheCurrentAnswerAsDiscardedAndAddsTheNewAnswer()
        {
            var activeGroupId = Guid.NewGuid().ToString();

            var newRequestAnswer =
                new CaseStatusValue()
                {
                    Option = "new option",
                    Value = "new value"
                };

            var request = TestHelpers.CreateUpdateCaseStatusRequest();
            request.Answers = new List<CaseStatusValue>
            {
                newRequestAnswer
            };

            var (caseStatus, _, _) = CaseStatusHelper.SavePersonWithCaseStatusToDatabase(DatabaseContext);

            var currentActiveAnswer = TestHelpers.CreateCaseStatusAnswers(min: 1, max: 1, endDate: null, discardedAt: null, groupId: activeGroupId);

            caseStatus.Answers = new List<CaseStatusAnswer>();
            caseStatus.Answers.AddRange(currentActiveAnswer);
            caseStatus.Type = "CP";
            caseStatus.EndDate = null;

            DatabaseContext.SaveChanges();

            request.CaseStatusId = caseStatus.Id;
            request.EndDate = null;
            request.StartDate = DateTime.Today.AddDays(-1).Date;

            _caseStatusGateway.UpdateCaseStatus(request);

            var updatedCaseStatus = DatabaseContext.CaseStatuses.FirstOrDefault(x => x.Id == caseStatus.Id);

            updatedCaseStatus.Answers.Count.Should().Be(2);

            var discardedAnswers = updatedCaseStatus.Answers.Where(x => x.GroupId == activeGroupId);

            discardedAnswers.All(x => x.DiscardedAt != null).Should().BeTrue();

            var newAnswers = updatedCaseStatus.Answers.Where(x => x.GroupId != activeGroupId);

            newAnswers.All(x => x.DiscardedAt == null).Should().BeTrue();
            newAnswers.All(x => x.StartDate.Date == request.StartDate?.Date).Should().BeTrue();
            newAnswers.All(x => x.Option == newRequestAnswer.Option).Should().BeTrue();
            newAnswers.All(x => x.Value == newRequestAnswer.Value).Should().BeTrue();
        }

        //LAC

        [Test]
        public void WhenTypeIsLACAndEndDateIsNotProvidedItUpdatesTheCurrentActiveAnswersWithDiscardedDateAndAddsNewAnswersWithProvidedStartDateAndUpdatesTheCaseStartDate()
        {
            var activeGroupId = Guid.NewGuid().ToString();

            var request = TestHelpers.CreateUpdateCaseStatusRequest();
            request.EndDate = null;
            request.StartDate = DateTime.Today.AddDays(-1);

            var newRequestAnswer = new CaseStatusValue() { Option = "new option", Value = "new value" };
            var newRequestAnswer2 = new CaseStatusValue() { Option = "new option", Value = "new value" };

            request.Answers = new List<CaseStatusValue>() { newRequestAnswer, newRequestAnswer2 };

            var (caseStatus, _, _) = CaseStatusHelper.SavePersonWithCaseStatusToDatabase(DatabaseContext);

            var answers = TestHelpers.CreateCaseStatusAnswers(min: 2, max: 2, endDate: null, discardedAt: null, groupId: activeGroupId);

            caseStatus.Answers = answers;
            caseStatus.Type = "LAC";
            caseStatus.EndDate = null;

            DatabaseContext.SaveChanges();

            request.CaseStatusId = caseStatus.Id;
            request.StartDate = DateTime.Now.AddDays(-1);

            _caseStatusGateway.UpdateCaseStatus(request);

            var updatedCaseStatus = DatabaseContext.CaseStatuses.FirstOrDefault(x => x.Id == caseStatus.Id);

            updatedCaseStatus.StartDate.Should().Be(request.StartDate.Value);
            updatedCaseStatus.Answers.Count.Should().Be(4);

            var discardedAnswers = updatedCaseStatus.Answers.Where(x => x.GroupId == activeGroupId);

            discardedAnswers.All(x => x.DiscardedAt != null).Should().BeTrue();

            var newAnswers = updatedCaseStatus.Answers.Where(x => x.GroupId != activeGroupId);

            newAnswers.All(x => x.DiscardedAt == null).Should().BeTrue();
            newAnswers.All(x => x.StartDate.Date == request.StartDate?.Date).Should().BeTrue();
        }

        [Test]
        public void WhenTypeIsLACAndEndDateIsNotProvidedAndThereAreMoreThanOneGroupOfAnswersAndTheStartDateOverlapsWithThePreviousStartDateItThrowsInvalidStartDateException()
        {
            var previousGroupId = Guid.NewGuid().ToString();
            var activeGroupId = Guid.NewGuid().ToString();

            var request = TestHelpers.CreateUpdateCaseStatusRequest();
            request.EndDate = null;

            var newRequestAnswer = new CaseStatusValue() { Option = "new option", Value = "new value" };
            var newRequestAnswer2 = new CaseStatusValue() { Option = "new option", Value = "new value" };

            request.Answers = new List<CaseStatusValue>() { newRequestAnswer, newRequestAnswer2 };

            var (caseStatus, _, _) = CaseStatusHelper.SavePersonWithCaseStatusToDatabase(DatabaseContext);

            var previousAnswers = TestHelpers.CreateCaseStatusAnswers(min: 2, max: 2, startDate: DateTime.Today.AddDays(-50), endDate: DateTime.Today.AddDays(-40), discardedAt: null, groupId: previousGroupId);
            var activeAnswers = TestHelpers.CreateCaseStatusAnswers(min: 2, max: 2, startDate: DateTime.Today.AddDays(-10), endDate: null, discardedAt: null, groupId: activeGroupId);

            caseStatus.Answers = new List<CaseStatusAnswer>();
            caseStatus.Answers.AddRange(previousAnswers);
            caseStatus.Answers.AddRange(activeAnswers);

            caseStatus.Type = "LAC";
            caseStatus.EndDate = null;

            DatabaseContext.SaveChanges();

            request.CaseStatusId = caseStatus.Id;
            request.StartDate = DateTime.Now.AddDays(-15);

            Action act = () => _caseStatusGateway.UpdateCaseStatus(request);

            act.Should().Throw<InvalidStartDateException>().WithMessage("Start date overlaps with previous status start date.");

            DatabaseContext.CaseStatuses.FirstOrDefault(x => x.Id == caseStatus.Id);
        }

        [Test]
        public void WhenTypeIsLACAndEndDateIsNotProvidedAndThereAreMoreThanOneGroupOfAnswersItDiscardsTheMostRecentAnswers()
        {
            var previousGroupId = Guid.NewGuid().ToString();
            var activeGroupId = Guid.NewGuid().ToString();

            var request = TestHelpers.CreateUpdateCaseStatusRequest();
            request.EndDate = null;

            var newRequestAnswer = new CaseStatusValue() { Option = "new option", Value = "new value" };
            var newRequestAnswer2 = new CaseStatusValue() { Option = "new option", Value = "new value" };

            request.Answers = new List<CaseStatusValue>() { newRequestAnswer, newRequestAnswer2 };

            var (caseStatus, _, _) = CaseStatusHelper.SavePersonWithCaseStatusToDatabase(DatabaseContext);

            var previousAnswers = TestHelpers.CreateCaseStatusAnswers(min: 2, max: 2, startDate: DateTime.Today.AddDays(-50), endDate: DateTime.Today.AddDays(-40), discardedAt: null, groupId: previousGroupId);
            var activeAnswers = TestHelpers.CreateCaseStatusAnswers(min: 2, max: 2, startDate: DateTime.Today.AddDays(-10), endDate: null, discardedAt: null, groupId: activeGroupId);

            caseStatus.Answers = new List<CaseStatusAnswer>();
            caseStatus.Answers.AddRange(previousAnswers);
            caseStatus.Answers.AddRange(activeAnswers);

            caseStatus.Type = "LAC";
            caseStatus.EndDate = null;

            DatabaseContext.SaveChanges();

            request.CaseStatusId = caseStatus.Id;
            request.StartDate = DateTime.Now.AddDays(-1);

            _caseStatusGateway.UpdateCaseStatus(request);

            var updatedCaseStatus = DatabaseContext.CaseStatuses.FirstOrDefault(x => x.Id == caseStatus.Id);

            updatedCaseStatus.Answers.Where(x => x.GroupId == activeGroupId).All(x => x.DiscardedAt != null).Should().BeTrue();
        }

        [Test]
        public void WhenTypeIsLACAndEndDateIsNotProvidedAndThereAreMoreThanOneGroupOfAnswersItAddsNewGroupsOfAnswersCopyingTheAnswersFromTheDiscardedPreviousAnswers()
        {
            var previousGroupId = Guid.NewGuid().ToString();
            var activeGroupId = Guid.NewGuid().ToString();

            var request = TestHelpers.CreateUpdateCaseStatusRequest();
            request.EndDate = null;

            var newRequestAnswer = new CaseStatusValue() { Option = "new option 1", Value = "new value 1" };
            var newRequestAnswer2 = new CaseStatusValue() { Option = "new option 2", Value = "new value 2" };

            request.Answers = new List<CaseStatusValue>() { newRequestAnswer, newRequestAnswer2 };

            var (caseStatus, _, _) = CaseStatusHelper.SavePersonWithCaseStatusToDatabase(DatabaseContext);

            var previousAnswers = TestHelpers.CreateCaseStatusAnswers(min: 2, max: 2, startDate: DateTime.Today.AddDays(-50), endDate: DateTime.Today.AddDays(-40), discardedAt: null, groupId: previousGroupId);
            var activeAnswers = TestHelpers.CreateCaseStatusAnswers(min: 2, max: 2, startDate: DateTime.Today.AddDays(-10), endDate: null, discardedAt: null, groupId: activeGroupId);

            activeAnswers.FirstOrDefault().Option = "Active answer 1 option";
            activeAnswers.FirstOrDefault().Value = "Active answer 1 value";

            activeAnswers.LastOrDefault().Option = "Active answer 2 option";
            activeAnswers.LastOrDefault().Value = "Active answer 2 value";

            caseStatus.Answers = new List<CaseStatusAnswer>();
            caseStatus.Answers.AddRange(previousAnswers);
            caseStatus.Answers.AddRange(activeAnswers);

            caseStatus.Type = "LAC";
            caseStatus.EndDate = null;

            DatabaseContext.SaveChanges();

            request.CaseStatusId = caseStatus.Id;
            request.StartDate = DateTime.Now.AddDays(-1);

            _caseStatusGateway.UpdateCaseStatus(request);

            var updatedCaseStatus = DatabaseContext.CaseStatuses.FirstOrDefault(x => x.Id == caseStatus.Id);

            var newAnswers = updatedCaseStatus.Answers.Where(x => x.GroupId != previousGroupId && x.GroupId != activeGroupId);
            var previousActiveAnswers = updatedCaseStatus.Answers.Where(x => x.GroupId == activeGroupId);

            newAnswers.Any(x => x.Option == activeAnswers.FirstOrDefault().Option).Should().BeTrue();
            newAnswers.Any(x => x.Value == activeAnswers.FirstOrDefault().Value).Should().BeTrue();
            newAnswers.Any(x => x.Option == activeAnswers.LastOrDefault().Option).Should().BeTrue();
            newAnswers.Any(x => x.Value == activeAnswers.LastOrDefault().Value).Should().BeTrue();
        }
    }
}
