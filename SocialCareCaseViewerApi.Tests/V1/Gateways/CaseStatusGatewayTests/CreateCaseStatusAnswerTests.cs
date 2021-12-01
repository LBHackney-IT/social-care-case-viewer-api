using FluentAssertions;
using Microsoft.EntityFrameworkCore;
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
using CaseStatusAnswerInfrastructure = SocialCareCaseViewerApi.V1.Infrastructure.CaseStatusAnswer;

#nullable enable
namespace SocialCareCaseViewerApi.Tests.V1.Gateways.CaseStatusGatewayTests
{
    [TestFixture]
    public class CreateCaseStatusAnswerTests : DatabaseTests
    {
        private CaseStatusGateway _caseStatusGateway = null!;
        private Mock<ISystemTime> _mockSystemTime = null!;

        [SetUp]
        public void SetUp()
        {
            _mockSystemTime = new Mock<ISystemTime>();
            _caseStatusGateway = new CaseStatusGateway(DatabaseContext, _mockSystemTime.Object);
        }

        [Test]
        public void CreateCaseStatusAnswerClassImplementsIAuditEntityInterface()
        {
            var caseStatusAnswer = new CaseStatusAnswerInfrastructure();
            (caseStatusAnswer is IAuditEntity).Should().BeTrue();
        }

        [Test]
        public void CreatesACaseStatusAnswer()
        {
            var caseStatus = TestHelpers.CreateCaseStatus(type: "LAC");
            DatabaseContext.CaseStatuses.Add(caseStatus);
            DatabaseContext.SaveChanges();

            var request = CaseStatusHelper.CreateCaseStatusAnswerRequest(caseStatusId: caseStatus.Id);

            _caseStatusGateway.CreateCaseStatusAnswer(request);

            var caseStatusAnswer = DatabaseContext.CaseStatuses.Include(x => x.Answers).FirstOrDefault(x => x.Id == caseStatus.Id).Answers.FirstOrDefault();

            caseStatusAnswer.CaseStatusId.Should().Be(caseStatus.Id);
            caseStatusAnswer.CreatedBy.Should().Be(request.CreatedBy);
            caseStatusAnswer.CreatedAt.Should().NotBeNull();
            caseStatusAnswer.Option.Should().Be(request.Answers.First().Option);
            caseStatusAnswer.Value.Should().Be(request.Answers.First().Value);
            caseStatusAnswer.StartDate.Should().Be(request.StartDate);
            caseStatusAnswer.CreatedAt.Should().NotBeNull();
        }

        [Test]
        public void CreatesMultipleCaseStatusAnswers()
        {
            var caseStatus = TestHelpers.CreateCaseStatus(type: "LAC");
            DatabaseContext.CaseStatuses.Add(caseStatus);
            DatabaseContext.SaveChanges();

            var request = CaseStatusHelper.CreateCaseStatusAnswerRequest(caseStatusId: caseStatus.Id);
            request.Answers = CaseStatusHelper.CreateCaseStatusRequestAnswers(min: 2);

            _caseStatusGateway.CreateCaseStatusAnswer(request);

            var updatedCaseStatus = DatabaseContext.CaseStatuses.Include(x => x.Answers).FirstOrDefault();

            updatedCaseStatus.Answers.Count.Should().Be(request.Answers.Count);
        }

        [Test]
        public void ThrowsAnExceptionWhenCaseStatusNotFound()
        {
            var request = CaseStatusHelper.CreateCaseStatusAnswerRequest();

            Action act = () => _caseStatusGateway.CreateCaseStatusAnswer(request);

            act.Should().Throw<CaseStatusDoesNotExistException>().WithMessage($"Case status id {request.CaseStatusId} does not exist.");
        }

        [Test]
        public void ReplaceActiveCaseStatusAnswersThrowsAnExceptionWhenCaseStatusNotFound()
        {
            var request = CaseStatusHelper.CreateCaseStatusAnswerRequest();

            Action act = () => _caseStatusGateway.ReplaceCaseStatusAnswers(request);

            act.Should().Throw<CaseStatusDoesNotExistException>().WithMessage($"Case status id {request.CaseStatusId} does not exist.");
        }

        [Test]
        public void UpdatesTheEndDateOnActiveAnswersAndAddsNewAnswers()
        {
            var personId = 123;
            var groupId = Guid.NewGuid().ToString();
            var caseStatus = TestHelpers.CreateCaseStatus(type: "LAC", personId: personId);

            var currentActiveAnswers = TestHelpers.CreateCaseStatusAnswers(caseStatusId: caseStatus.Id, startDate: DateTime.Today.AddDays(-10), min: 2, max: 2, groupId: groupId);
            caseStatus.Answers.AddRange(currentActiveAnswers);
            DatabaseContext.CaseStatuses.Add(caseStatus);
            DatabaseContext.SaveChanges();

            var requestAnswer1 = new CaseStatusRequestAnswers()
            {
                Option = "placementType",
                Value = "P1"
            };

            var requestAnswer2 = new CaseStatusRequestAnswers()
            {
                Option = "legalStatus",
                Value = "L1"
            };

            var requestAnswers = new List<CaseStatusRequestAnswers>() { requestAnswer1, requestAnswer2 };

            var request = CaseStatusHelper.CreateCaseStatusAnswerRequest(caseStatusId: caseStatus.Id, answers: requestAnswers, startDate: DateTime.Today.AddDays(-2));

            _caseStatusGateway.ReplaceCaseStatusAnswers(request);

            var caseStatusAnswers = DatabaseContext.CaseStatuses.FirstOrDefault().Answers;

            caseStatusAnswers.Count.Should().Be(5);

            var previousActiveAnswers = caseStatusAnswers.Where(x => x.GroupId == groupId && x.Option != CaseStatusAnswerOption.EpisodeReason);

            previousActiveAnswers.All(x => x.EndDate != null).Should().BeTrue();
            previousActiveAnswers.All(x => x.EndDate == request.StartDate);
            previousActiveAnswers.All(x => x.DiscardedAt == null).Should().BeTrue();
            previousActiveAnswers.All(x => x.LastModifiedBy == request.CreatedBy).Should().BeTrue();

            var newActiveAnswers = caseStatusAnswers.Where(x => x.GroupId != groupId);

            newActiveAnswers.All(x => x.EndDate == null).Should().BeTrue();
            newActiveAnswers.All(x => x.StartDate == request.StartDate).Should().BeTrue();
            newActiveAnswers.All(x => x.DiscardedAt == null).Should().BeTrue();
        }

        [Test]
        public void UpdatesTheEndDateOnActiveAnswersAndReplacesCurrentScheduledCaseStatusAnswers()
        {
            var personId = 123;
            var groupId = Guid.NewGuid().ToString();

            var caseStatus = TestHelpers.CreateCaseStatus(type: "LAC", personId: personId);

            var currentScheduledAnswers = TestHelpers.CreateCaseStatusAnswers(caseStatusId: caseStatus.Id, startDate: DateTime.Today.AddDays(10).Date, min: 2, max: 2, groupId: groupId);
            caseStatus.Answers.AddRange(currentScheduledAnswers);
            DatabaseContext.CaseStatuses.Add(caseStatus);
            DatabaseContext.SaveChanges();

            var requestAnswer1 = new CaseStatusRequestAnswers()
            {
                Option = "placementType",
                Value = "P1"
            };

            var requestAnswer2 = new CaseStatusRequestAnswers()
            {
                Option = "legalStatus",
                Value = "L1"
            };

            var requestAnswers = new List<CaseStatusRequestAnswers>() { requestAnswer1, requestAnswer2 };

            var request = CaseStatusHelper.CreateCaseStatusAnswerRequest(caseStatusId: caseStatus.Id, answers: requestAnswers, startDate: DateTime.Today.AddDays(20));

            _caseStatusGateway.ReplaceCaseStatusAnswers(request);

            var caseStatusAnswers = DatabaseContext.CaseStatuses.Include(x => x.Answers).FirstOrDefault().Answers;

            caseStatusAnswers.Count.Should().Be(4);

            var discardedScheduledAnswers = caseStatusAnswers.Where(x => x.GroupId == groupId);

            discardedScheduledAnswers.All(x => x.DiscardedAt != null).Should().BeTrue();
            discardedScheduledAnswers.All(x => x.EndDate == null).Should().BeTrue();
            discardedScheduledAnswers.All(x => x.LastModifiedBy == request.CreatedBy).Should().BeTrue();

            var newScheduledAnswers = caseStatusAnswers.Where(x => x.GroupId != groupId);

            newScheduledAnswers.All(x => x.StartDate == request.StartDate).Should().BeTrue();
            newScheduledAnswers.Any(x => x.Option == requestAnswer1.Option && x.Value == requestAnswer1.Value).Should().BeTrue();
            newScheduledAnswers.Any(x => x.Option == requestAnswer2.Option && x.Value == requestAnswer2.Value).Should().BeTrue();
        }

        [Test]
        public void AddsNewEpisodeReasonAnswerUsingTheStartAndEndDateOfTheCurrentAnswers()
        {
            var personId = 123;
            var groupId = Guid.NewGuid().ToString();
            var currentActiveAnswersStartDate = DateTime.Today.AddDays(-10);

            var caseStatus = TestHelpers.CreateCaseStatus(type: "LAC", personId: personId);

            var currentActiveAnswers = TestHelpers.CreateCaseStatusAnswers(caseStatusId: caseStatus.Id, startDate: currentActiveAnswersStartDate, min: 2, max: 2, groupId: groupId);
            caseStatus.Answers = new List<CaseStatusAnswerInfrastructure>();
            caseStatus.Answers.AddRange(currentActiveAnswers);
            DatabaseContext.CaseStatuses.Add(caseStatus);
            DatabaseContext.SaveChanges();

            var requestAnswer = CaseStatusHelper.CreateCaseStatusRequestAnswers(min: 1, max: 1);

            var request = CaseStatusHelper.CreateCaseStatusAnswerRequest(caseStatusId: caseStatus.Id, answers: requestAnswer, startDate: DateTime.Today.AddDays(-2));

            _caseStatusGateway.ReplaceCaseStatusAnswers(request);

            var updatedCaseStatus = DatabaseContext.CaseStatuses.FirstOrDefault();
            updatedCaseStatus.Answers.Where(x => x.GroupId == groupId).Count().Should().Be(3);

            var episodeEndReasonAnswer = updatedCaseStatus.Answers.Where(x => x.GroupId == groupId && x.Option == CaseStatusAnswerOption.EpisodeReason && x.Value == CaseStatusAnswerValue.X1).FirstOrDefault();
            episodeEndReasonAnswer.Should().NotBeNull();
            episodeEndReasonAnswer.StartDate.Should().Be(currentActiveAnswersStartDate);
            episodeEndReasonAnswer.EndDate.Should().Be(request.StartDate);
            episodeEndReasonAnswer.CreatedBy = request.CreatedBy;
        }
    }
}
