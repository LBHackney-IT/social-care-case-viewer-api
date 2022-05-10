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

namespace SocialCareCaseViewerApi.Tests.V1.Gateways.CaseStatusGatewayTests.CP
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
        public void WhenTypeIsCPAndValidEndDateIsProvidedItUpdatesTheStatusAndTheCurrentActiveAnswersWithEndDate()
        {
            var request = TestHelpers.CreateUpdateCaseStatusRequest(min: 1, max: 1);
            var activeGroupId = Guid.NewGuid().ToString();

            var (caseStatus, _, _) = CaseStatusHelper.SavePersonWithCaseStatusToDatabase(DatabaseContext);
            var createdAt = DateTime.Today.AddDays(-10);

            var answers = TestHelpers.CreateCaseStatusAnswers(min: 1, max: 1, endDate: null, discardedAt: null, groupId: activeGroupId, startDate: DateTime.Today.AddDays(-10));

            caseStatus.Answers = answers;
            caseStatus.Type = "CP";
            caseStatus.EndDate = null;
            caseStatus.StartDate = DateTime.Today.AddDays(-10);

            DatabaseContext.SaveChanges();

            foreach (var a in caseStatus.Answers)
            {
                a.CreatedAt = createdAt;
            }

            DatabaseContext.SaveChanges();

            request.CaseStatusId = caseStatus.Id;

            _caseStatusGateway.UpdateCaseStatus(request);

            var updatedCaseStatus = DatabaseContext.CaseStatuses.FirstOrDefault(x => x.Id == caseStatus.Id);

            updatedCaseStatus.EndDate.Should().NotBeNull();
            updatedCaseStatus.LastModifiedBy.Should().Be(request.EditedBy);

            updatedCaseStatus.Answers.Count.Should().Be(2);
            updatedCaseStatus.Answers.All(x => x.EndDate != null).Should().BeTrue();
            updatedCaseStatus.Answers.All(x => x.DiscardedAt == null).Should().BeTrue();

            var activeAnswers = updatedCaseStatus.Answers.Where(x => x.GroupId == activeGroupId && x.CreatedAt == createdAt);
            activeAnswers.All(x => x.LastModifiedBy == request.EditedBy).Should().BeTrue();
        }

        [Test]
        public void WhenTypeIsCPAndValidEndDateIsProvidedItAddsTheEpisodeEndingReasonAnswer()
        {
            var request = TestHelpers.CreateUpdateCaseStatusRequest(min: 1, max: 1);
            var newRequestAnswer = request.Answers.FirstOrDefault();

            var groupId = Guid.NewGuid().ToString();

            var (caseStatus, _, _) = CaseStatusHelper.SavePersonWithCaseStatusToDatabase(DatabaseContext);
            var activeAnswersStartDate = DateTime.Today.AddDays(-10);

            var currentAnswers = TestHelpers.CreateCaseStatusAnswers(min: 1, max: 1, endDate: null, discardedAt: null, groupId: groupId, startDate: activeAnswersStartDate);

            caseStatus.Answers = currentAnswers;
            caseStatus.Type = "CP";
            caseStatus.EndDate = null;
            caseStatus.StartDate = DateTime.Today.AddDays(-10);

            DatabaseContext.SaveChanges();

            foreach (var a in caseStatus.Answers)
            {
                a.CreatedAt = DateTime.Today.AddDays(-10);
            }

            DatabaseContext.SaveChanges();

            request.CaseStatusId = caseStatus.Id;

            _caseStatusGateway.UpdateCaseStatus(request);

            var updatedCaseStatus = DatabaseContext.CaseStatuses.FirstOrDefault(x => x.Id == caseStatus.Id);

            updatedCaseStatus.EndDate.Should().NotBeNull();
            updatedCaseStatus.LastModifiedBy.Should().Be(request.EditedBy);

            updatedCaseStatus.Answers.Count.Should().Be(2);
            updatedCaseStatus.Answers.All(x => x.EndDate != null).Should().BeTrue();
            updatedCaseStatus.Answers.All(x => x.DiscardedAt == null).Should().BeTrue();

            var episodeEndingAnswer = updatedCaseStatus.Answers.Where(x => x.GroupId == groupId).OrderByDescending(x => x.CreatedAt).FirstOrDefault();

            episodeEndingAnswer.Option.Should().Be(newRequestAnswer.Option);
            episodeEndingAnswer.Value.Should().Be(newRequestAnswer.Value);
            episodeEndingAnswer.EndDate.Value.Date.Should().Be(request.EndDate.Value.Date);
            episodeEndingAnswer.StartDate?.Date.Should().Be(activeAnswersStartDate);
            episodeEndingAnswer.CreatedBy.Should().Be(request.EditedBy);
            episodeEndingAnswer.GroupId.Should().Be(groupId);
        }

        [Test]
        public void WhenTypeIsCPAndValidEndDateIsProvidedAndThereAreScheduledAnswersItDiscardsTheScheduledAnswers()
        {
            var scheduledGroupId = Guid.NewGuid().ToString();

            var request = TestHelpers.CreateUpdateCaseStatusRequest(min: 1, max: 1);

            var (caseStatus, _, _) = CaseStatusHelper.SavePersonWithCaseStatusToDatabase(DatabaseContext);
            var scheduledAnswersStartDate = DateTime.Today.AddDays(5);

            var currentAnswers = TestHelpers.CreateCaseStatusAnswers(min: 1, max: 1, endDate: scheduledAnswersStartDate, discardedAt: null, startDate: DateTime.Today.AddDays(-10));
            var scheduledAnswers = TestHelpers.CreateCaseStatusAnswers(min: 1, max: 1, endDate: null, discardedAt: null, startDate: scheduledAnswersStartDate, groupId: scheduledGroupId);

            caseStatus.Answers = new List<CaseStatusAnswer>();
            caseStatus.Answers.AddRange(scheduledAnswers);
            caseStatus.Answers.AddRange(currentAnswers);
            caseStatus.Type = "CP";
            caseStatus.EndDate = null;

            DatabaseContext.SaveChanges();
            request.CaseStatusId = caseStatus.Id;

            _caseStatusGateway.UpdateCaseStatus(request);

            var updatedCaseStatus = DatabaseContext.CaseStatuses.FirstOrDefault(x => x.Id == caseStatus.Id);

            updatedCaseStatus.EndDate.Should().NotBeNull();
            updatedCaseStatus.LastModifiedBy.Should().Be(request.EditedBy);

            updatedCaseStatus.Answers.Count.Should().Be(3);

            updatedCaseStatus.Answers.Where(x => x.GroupId == scheduledGroupId).All(x => x.EndDate == null).Should().BeTrue();
            updatedCaseStatus.Answers.Where(x => x.GroupId == scheduledGroupId).All(x => x.DiscardedAt != null).Should().BeTrue();
            updatedCaseStatus.Answers.Where(x => x.GroupId == scheduledGroupId).All(x => x.LastModifiedBy == request.EditedBy).Should().BeTrue();
        }

        [Test]
        public void WhenTypeIsCPAndEndDateIsNotProvidedItUpdatesTheCaseStartDateWhenThereIsOnlyOneSetOfAnswers()
        {
            var activeGroupId = Guid.NewGuid().ToString();

            var request = TestHelpers.CreateUpdateCaseStatusRequest(min: 1, max: 1);
            request.EndDate = null;
            request.StartDate = DateTime.Today.AddDays(-1);

            var (caseStatus, _, _) = CaseStatusHelper.SavePersonWithCaseStatusToDatabase(DatabaseContext);

            var answers = TestHelpers.CreateCaseStatusAnswers(min: 1, max: 1, endDate: null, discardedAt: null, groupId: activeGroupId);

            caseStatus.Answers = answers;
            caseStatus.Type = "CP";
            caseStatus.EndDate = null;

            DatabaseContext.SaveChanges();

            request.CaseStatusId = caseStatus.Id;
            request.StartDate = DateTime.Now.AddDays(-1);

            _caseStatusGateway.UpdateCaseStatus(request);

            var updatedCaseStatus = DatabaseContext.CaseStatuses.FirstOrDefault(x => x.Id == caseStatus.Id);
            updatedCaseStatus.LastModifiedBy.Should().Be(request.EditedBy);
            updatedCaseStatus.StartDate.Should().Be(request.StartDate.Value);
        }

        [Test]
        public void WhenTypeIsCPAndEndDateIsNotProvidedItUpdatesTheCaseStartDateWhenThereIsOneActiveSetOfAnswersAndAlsoScheduledAnswers()
        {
            var activeGroupId = Guid.NewGuid().ToString();
            var scheduledGroupId = Guid.NewGuid().ToString();

            var (caseStatus, _, _) = CaseStatusHelper.SavePersonWithCaseStatusToDatabase(DatabaseContext);
            caseStatus.Type = "CP";
            caseStatus.EndDate = null;
            caseStatus.StartDate = new DateTime(2021, 11, 01);

            var activeAnswers = TestHelpers.CreateCaseStatusAnswers(min: 1, max: 1, startDate: DateTime.Today.AddDays(-20), endDate: DateTime.Today.AddDays(50), groupId: activeGroupId);
            var scheduledAnswers = TestHelpers.CreateCaseStatusAnswers(min: 1, max: 1, startDate: DateTime.Today.AddDays(50), endDate: null, groupId: scheduledGroupId);

            caseStatus.Answers = new List<CaseStatusAnswer>();
            caseStatus.Answers.AddRange(activeAnswers);
            caseStatus.Answers.AddRange(scheduledAnswers);
            DatabaseContext.SaveChanges();

            var request = TestHelpers.CreateUpdateCaseStatusRequest(min: 1, max: 1);
            request.EndDate = null;
            request.CaseStatusId = caseStatus.Id;
            request.StartDate = DateTime.Today.AddDays(-17);

            _caseStatusGateway.UpdateCaseStatus(request);

            var updatedCaseStatus = DatabaseContext.CaseStatuses.FirstOrDefault(x => x.Id == caseStatus.Id);
            updatedCaseStatus.StartDate.Should().Be(request.StartDate.Value);
            updatedCaseStatus.Answers.Where(x => x.GroupId != activeGroupId && x.GroupId != scheduledGroupId).All(x => x.StartDate == request.StartDate).Should().BeTrue();
        }

        [Test]
        public void WhenTypeIsCPAndEndDateIsNotProvidedItCopiesTheCaseStatusAnswersEndDateWhenThereIsOneActiveSetOfAnswersAndAlsoScheduledAnswers()
        {
            var activeGroupId = Guid.NewGuid().ToString();
            var scheduledGroupId = Guid.NewGuid().ToString();

            var (caseStatus, _, _) = CaseStatusHelper.SavePersonWithCaseStatusToDatabase(DatabaseContext);
            caseStatus.Type = "CP";
            caseStatus.EndDate = null;
            caseStatus.StartDate = new DateTime(2021, 11, 01);

            var activeAnswers = TestHelpers.CreateCaseStatusAnswers(min: 1, max: 1, startDate: DateTime.Today.AddDays(-20), endDate: DateTime.Today.AddDays(50), groupId: activeGroupId);
            var scheduledAnswers = TestHelpers.CreateCaseStatusAnswers(min: 1, max: 1, startDate: DateTime.Today.AddDays(50), endDate: null, groupId: scheduledGroupId);

            caseStatus.Answers = new List<CaseStatusAnswer>();
            caseStatus.Answers.AddRange(activeAnswers);
            caseStatus.Answers.AddRange(scheduledAnswers);
            DatabaseContext.SaveChanges();

            var request = TestHelpers.CreateUpdateCaseStatusRequest(min: 1, max: 1);
            request.EndDate = null;
            request.CaseStatusId = caseStatus.Id;
            request.StartDate = DateTime.Today.AddDays(-17);

            _caseStatusGateway.UpdateCaseStatus(request);

            var updatedCaseStatus = DatabaseContext.CaseStatuses.FirstOrDefault(x => x.Id == caseStatus.Id);
            updatedCaseStatus.StartDate.Should().Be(request.StartDate.Value);
            var newCaseStatusAnswers = updatedCaseStatus.Answers.Where(x => x.GroupId != activeGroupId && x.GroupId != scheduledGroupId && x.StartDate <= DateTime.Today);
            newCaseStatusAnswers.All(x => x.EndDate == activeAnswers.FirstOrDefault().EndDate).Should().BeTrue();
        }

        [Test]
        public void WhenTypeIsCPAndEndDateIsNotProvidedItDoesNotUpdateTheCaseStartDateWhenThereIsMoreThanOneSetOfAnswers()
        {
            var activeGroupId = Guid.NewGuid().ToString();
            var activeGroupId2 = Guid.NewGuid().ToString();

            var request = TestHelpers.CreateUpdateCaseStatusRequest(min: 1, max: 1);

            var (caseStatus, _, _) = CaseStatusHelper.SavePersonWithCaseStatusToDatabase(DatabaseContext);

            var answers = TestHelpers.CreateCaseStatusAnswers(min: 1, max: 1, startDate: DateTime.Today.AddDays(-3), endDate: DateTime.Today.AddDays(-2), discardedAt: null, groupId: activeGroupId);
            var answers2 = TestHelpers.CreateCaseStatusAnswers(min: 1, max: 1, startDate: DateTime.Today.AddDays(-2), endDate: null, discardedAt: null, groupId: activeGroupId2);

            caseStatus.Answers = answers;
            caseStatus.Type = "CP";
            caseStatus.EndDate = null;

            DatabaseContext.SaveChanges();

            request.EndDate = null;
            request.CaseStatusId = caseStatus.Id;
            request.StartDate = DateTime.Now.AddDays(-1);

            _caseStatusGateway.UpdateCaseStatus(request);

            var updatedCaseStatus = DatabaseContext.CaseStatuses.FirstOrDefault(x => x.Id == caseStatus.Id);
            updatedCaseStatus.LastModifiedBy.Should().Be(request.EditedBy);
            updatedCaseStatus.StartDate.Should().Be(caseStatus.StartDate);
        }

        [Test]
        public void WhenTypeIsCPAndEndDateIsNotProvidedAndTheStartDateOverlapsWithThePreviousStartDateItThrowsInvalidStartDateException()
        {
            var previousGroupId = Guid.NewGuid().ToString();
            var activeGroupId = Guid.NewGuid().ToString();

            var request = TestHelpers.CreateUpdateCaseStatusRequest(min: 1, max: 1);
            request.EndDate = null;

            var (caseStatus, _, _) = CaseStatusHelper.SavePersonWithCaseStatusToDatabase(DatabaseContext);

            var previousAnswers = TestHelpers.CreateCaseStatusAnswers(min: 1, max: 1, startDate: DateTime.Today.AddDays(-50), endDate: DateTime.Today.AddDays(-40), discardedAt: null, groupId: previousGroupId);
            var activeAnswers = TestHelpers.CreateCaseStatusAnswers(min: 1, max: 1, startDate: DateTime.Today.AddDays(-40), endDate: null, discardedAt: null, groupId: activeGroupId);

            caseStatus.Answers = new List<CaseStatusAnswer>();
            caseStatus.Answers.AddRange(previousAnswers);
            caseStatus.Answers.AddRange(activeAnswers);

            caseStatus.Type = "CP";
            caseStatus.EndDate = null;

            DatabaseContext.SaveChanges();

            request.CaseStatusId = caseStatus.Id;
            request.StartDate = DateTime.Now.AddDays(-50).Date;

            Action act = () => _caseStatusGateway.UpdateCaseStatus(request);

            act.Should().Throw<InvalidStartDateException>().WithMessage("Start date overlaps with previous status start date.");

            DatabaseContext.CaseStatuses.FirstOrDefault(x => x.Id == caseStatus.Id);
        }

        [Test]
        public void WhenTypeIsCPAndEndDateIsNotProvidedAndTheStartDateDoesNotOverlapWithThePreviousStartDateItIsValid()
        {
            var previousGroupId = Guid.NewGuid().ToString();
            var activeGroupId = Guid.NewGuid().ToString();

            var request = TestHelpers.CreateUpdateCaseStatusRequest(min: 1, max: 1);
            request.EndDate = null;

            var (caseStatus, _, _) = CaseStatusHelper.SavePersonWithCaseStatusToDatabase(DatabaseContext);

            var previousAnswers = TestHelpers.CreateCaseStatusAnswers(min: 1, max: 1, startDate: DateTime.Today.AddDays(-50), endDate: DateTime.Today.AddDays(-40), discardedAt: null, groupId: previousGroupId);
            var activeAnswers = TestHelpers.CreateCaseStatusAnswers(min: 1, max: 1, startDate: DateTime.Today.AddDays(-40), endDate: null, discardedAt: null, groupId: activeGroupId);

            caseStatus.Answers = new List<CaseStatusAnswer>();
            caseStatus.Answers.AddRange(previousAnswers);
            caseStatus.Answers.AddRange(activeAnswers);

            caseStatus.Type = "CP";
            caseStatus.EndDate = null;

            DatabaseContext.SaveChanges();

            request.CaseStatusId = caseStatus.Id;
            request.StartDate = DateTime.Now.AddDays(-49).Date;

            _caseStatusGateway.UpdateCaseStatus(request);

            var updatedCaseStatus = DatabaseContext.CaseStatuses.FirstOrDefault(x => x.Id == caseStatus.Id);

            var newAnswers = updatedCaseStatus.Answers.Where(x => x.GroupId != activeGroupId && x.GroupId != previousGroupId && x.EndDate == null);

            newAnswers.All(x => x.StartDate == request.StartDate).Should().BeTrue();
        }

        [Test]
        public void WhenTypeIsCPAndEndDateIsNotProvidedAndThereAreMoreThanOneGroupOfAnswersItDiscardsThePastAndActiveAnswers()
        {
            var previousGroupId = Guid.NewGuid().ToString();
            var activeGroupId = Guid.NewGuid().ToString();

            var request = TestHelpers.CreateUpdateCaseStatusRequest(min: 1, max: 1);
            request.EndDate = null;

            var (caseStatus, _, _) = CaseStatusHelper.SavePersonWithCaseStatusToDatabase(DatabaseContext);

            var previousAnswers = TestHelpers.CreateCaseStatusAnswers(min: 1, max: 1, startDate: DateTime.Today.AddDays(-50), endDate: DateTime.Today.AddDays(-40), discardedAt: null, groupId: previousGroupId);
            var activeAnswers = TestHelpers.CreateCaseStatusAnswers(min: 1, max: 1, startDate: DateTime.Today.AddDays(-10), endDate: null, discardedAt: null, groupId: activeGroupId);

            caseStatus.Answers = new List<CaseStatusAnswer>();
            caseStatus.Answers.AddRange(previousAnswers);
            caseStatus.Answers.AddRange(activeAnswers);

            caseStatus.Type = "CP";
            caseStatus.EndDate = null;

            DatabaseContext.SaveChanges();

            request.CaseStatusId = caseStatus.Id;
            request.StartDate = DateTime.Now.AddDays(-1);

            _caseStatusGateway.UpdateCaseStatus(request);

            var updatedCaseStatus = DatabaseContext.CaseStatuses.FirstOrDefault(x => x.Id == caseStatus.Id);
            updatedCaseStatus.LastModifiedBy.Should().Be(request.EditedBy);

            updatedCaseStatus.Answers.Where(x => x.GroupId == activeGroupId).All(x => x.DiscardedAt != null).Should().BeTrue();
            updatedCaseStatus.Answers.Where(x => x.GroupId == activeGroupId).All(x => x.LastModifiedBy == request.EditedBy).Should().BeTrue();
            updatedCaseStatus.Answers.Where(x => x.GroupId == previousGroupId).All(x => x.DiscardedAt != null).Should().BeTrue();
            updatedCaseStatus.Answers.Where(x => x.GroupId == previousGroupId).All(x => x.LastModifiedBy == request.EditedBy).Should().BeTrue();
        }

        [Test]
        public void WhenTypeIsCPAndEndDateIsNotProvidedItAddsNewAnswersWithTheUpdatedAnswers()
        {
            var activeGroupId = Guid.NewGuid().ToString();

            var request = TestHelpers.CreateUpdateCaseStatusRequest(min: 1, max: 1);
            request.EndDate = null;

            var (caseStatus, _, _) = CaseStatusHelper.SavePersonWithCaseStatusToDatabase(DatabaseContext);

            var activeAnswers = TestHelpers.CreateCaseStatusAnswers(min: 1, max: 1, startDate: DateTime.Today.AddDays(-10), endDate: null, discardedAt: null, groupId: activeGroupId);

            caseStatus.Answers = new List<CaseStatusAnswer>();
            caseStatus.Answers.AddRange(activeAnswers);
            caseStatus.Type = "CP";
            caseStatus.EndDate = null;

            DatabaseContext.SaveChanges();

            request.CaseStatusId = caseStatus.Id;
            request.StartDate = DateTime.Now.AddDays(-1);

            _caseStatusGateway.UpdateCaseStatus(request);

            var updatedCaseStatus = DatabaseContext.CaseStatuses.FirstOrDefault(x => x.Id == caseStatus.Id);
            updatedCaseStatus.LastModifiedBy.Should().Be(request.EditedBy);

            var newAnswers = updatedCaseStatus.Answers.Where(x => x.GroupId != activeGroupId);

            newAnswers.Any(x => x.Option == request.Answers.FirstOrDefault().Option).Should().BeTrue();
            newAnswers.Any(x => x.Value == request.Answers.FirstOrDefault().Value).Should().BeTrue();
            newAnswers.Any(x => x.Option == request.Answers.LastOrDefault().Option).Should().BeTrue();
            newAnswers.Any(x => x.Value == request.Answers.LastOrDefault().Value).Should().BeTrue();
            newAnswers.All(x => x.StartDate == request.StartDate).Should().BeTrue();

            var previousActiveAnswers = updatedCaseStatus.Answers.Where(x => x.GroupId == activeGroupId);

            previousActiveAnswers.All(x => x.LastModifiedBy == request.EditedBy).Should().BeTrue();
            previousActiveAnswers.All(x => x.DiscardedAt != null).Should().BeTrue();
        }

        [Test]
        public void WhenTypeIsCPAndEndDateIsNotProvidedAndThereAreMoreThanOneGroupOfAnswersItUpdatesTheEndDateOfThePreviousAnswers()
        {
            var previousGroupId = Guid.NewGuid().ToString();
            var activeGroupId = Guid.NewGuid().ToString();

            var request = TestHelpers.CreateUpdateCaseStatusRequest(min: 1, max: 1);

            var (caseStatus, _, _) = CaseStatusHelper.SavePersonWithCaseStatusToDatabase(DatabaseContext);

            var previousAnswers = TestHelpers.CreateCaseStatusAnswers(min: 1, max: 1, startDate: DateTime.Today.AddDays(-50), endDate: DateTime.Today.AddDays(-40), discardedAt: null, groupId: previousGroupId);
            var activeAnswers = TestHelpers.CreateCaseStatusAnswers(min: 1, max: 1, startDate: DateTime.Today.AddDays(-40), endDate: null, discardedAt: null, groupId: activeGroupId);

            caseStatus.Answers = new List<CaseStatusAnswer>();
            caseStatus.Answers.AddRange(previousAnswers);
            caseStatus.Answers.AddRange(activeAnswers);

            caseStatus.Type = "CP";
            caseStatus.EndDate = null;

            DatabaseContext.SaveChanges();

            request.EndDate = null;
            request.CaseStatusId = caseStatus.Id;
            request.StartDate = DateTime.Now.AddDays(-1);

            _caseStatusGateway.UpdateCaseStatus(request);

            var updatedCaseStatus = DatabaseContext.CaseStatuses.FirstOrDefault(x => x.Id == caseStatus.Id);

            var newPreviousAnswers = updatedCaseStatus.Answers.Where(x => x.GroupId != activeGroupId && x.GroupId != previousGroupId && x.EndDate != null);

            newPreviousAnswers.Any(x => x.Option == previousAnswers.FirstOrDefault().Option).Should().BeTrue();
            newPreviousAnswers.Any(x => x.Value == previousAnswers.FirstOrDefault().Value).Should().BeTrue();
            newPreviousAnswers.Any(x => x.Option == previousAnswers.LastOrDefault().Option).Should().BeTrue();
            newPreviousAnswers.Any(x => x.Value == previousAnswers.LastOrDefault().Value).Should().BeTrue();
            newPreviousAnswers.All(x => x.EndDate == request.StartDate).Should().BeTrue();
        }

        [Test]
        public void WhenTypeIsCPAndEndDateIsNotProvidedAndThereArePreviousAndScheduledAnswersEndDateOfPreviousAnswersIsUpdated()
        {
            var previousGroupId = Guid.NewGuid().ToString();
            var activeGroupId = Guid.NewGuid().ToString();
            var scheduledGroupId = Guid.NewGuid().ToString();

            var request = TestHelpers.CreateUpdateCaseStatusRequest(min: 1, max: 1);

            var (caseStatus, _, _) = CaseStatusHelper.SavePersonWithCaseStatusToDatabase(DatabaseContext);

            var previousAnswers = TestHelpers.CreateCaseStatusAnswers(min: 1, max: 1, startDate: DateTime.Today.AddDays(-50), endDate: DateTime.Today.AddDays(-40), discardedAt: null, groupId: previousGroupId);
            var activeAnswers = TestHelpers.CreateCaseStatusAnswers(min: 1, max: 1, startDate: DateTime.Today.AddDays(-40), endDate: DateTime.Today.AddDays(+40), discardedAt: null, groupId: activeGroupId);
            var scheduledAnswers = TestHelpers.CreateCaseStatusAnswers(min: 1, max: 1, startDate: DateTime.Today.AddDays(+40), endDate: null, discardedAt: null, groupId: scheduledGroupId);

            caseStatus.Answers = new List<CaseStatusAnswer>();
            caseStatus.Answers.AddRange(previousAnswers);
            caseStatus.Answers.AddRange(activeAnswers);
            caseStatus.Answers.AddRange(scheduledAnswers);

            caseStatus.Type = "CP";
            caseStatus.EndDate = null;

            DatabaseContext.SaveChanges();

            request.EndDate = null;
            request.CaseStatusId = caseStatus.Id;
            request.StartDate = DateTime.Now.AddDays(-1);

            _caseStatusGateway.UpdateCaseStatus(request);

            var updatedCaseStatus = DatabaseContext.CaseStatuses.FirstOrDefault(x => x.Id == caseStatus.Id);

            var newPreviousAnswers = updatedCaseStatus.Answers.Where(x => x.GroupId != activeGroupId && x.GroupId != previousGroupId && x.GroupId != scheduledGroupId && x.EndDate <= DateTime.Today);

            newPreviousAnswers.Any(x => x.Option == previousAnswers.FirstOrDefault().Option).Should().BeTrue();
            newPreviousAnswers.Any(x => x.Value == previousAnswers.FirstOrDefault().Value).Should().BeTrue();
            newPreviousAnswers.Any(x => x.Option == previousAnswers.LastOrDefault().Option).Should().BeTrue();
            newPreviousAnswers.Any(x => x.Value == previousAnswers.LastOrDefault().Value).Should().BeTrue();
            newPreviousAnswers.All(x => x.EndDate == request.StartDate).Should().BeTrue();
        }
    }
}
