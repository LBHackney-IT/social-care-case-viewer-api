using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using SocialCareCaseViewerApi.Tests.V1.Helpers;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Exceptions;
using SocialCareCaseViewerApi.V1.Gateways;
using SocialCareCaseViewerApi.V1.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;

#nullable enable
namespace SocialCareCaseViewerApi.Tests.V1.Gateways.CaseStatusGatewayTests
{
    [TestFixture]
    public class CreateCaseStatusAnswerTests : DatabaseTests
    {
        private CaseStatusGateway _caseStatusCateway = null!;
        private Mock<ISystemTime> _mockSystemTime = null!;

        [SetUp]
        public void SetUp()
        {
            _mockSystemTime = new Mock<ISystemTime>();
            _caseStatusCateway = new CaseStatusGateway(DatabaseContext, _mockSystemTime.Object);
        }

        [Test]
        public void CreatesACaseStatusAnswer()
        {
            var caseStatus = TestHelpers.CreateCaseStatus(type: "LAC");
            DatabaseContext.CaseStatuses.Add(caseStatus);
            DatabaseContext.SaveChanges();

            var request = CaseStatusHelper.CreateCaseStatusAnswerRequest(caseStatusId: caseStatus.Id);

            _caseStatusCateway.CreateCaseStatusAnswer(request);

            var caseStatusAnswer = DatabaseContext.CaseStatuses.Include(x => x.Answers).FirstOrDefault(x => x.Id == caseStatus.Id).Answers.FirstOrDefault();

            caseStatusAnswer.CaseStatusId.Should().Be(caseStatus.Id);
            caseStatusAnswer.CreatedBy.Should().Be(request.CreatedBy);
            caseStatusAnswer.CreatedAt.Should().NotBeNull();
            caseStatusAnswer.Option.Should().Be(request.Answers.First().Option);
            caseStatusAnswer.Value.Should().Be(request.Answers.First().Value);
            caseStatusAnswer.StartDate.Should().Be(request.StartDate);
        }

        [Test]
        public void CreatesMultipleCaseStatusAnswers()
        {
            var caseStatus = TestHelpers.CreateCaseStatus(type: "LAC");
            DatabaseContext.CaseStatuses.Add(caseStatus);
            DatabaseContext.SaveChanges();

            var request = CaseStatusHelper.CreateCaseStatusAnswerRequest(caseStatusId: caseStatus.Id);
            request.Answers = CaseStatusHelper.CreateCaseStatusRequestAnswers(min: 2);

            _caseStatusCateway.CreateCaseStatusAnswer(request);

            var updatedCaseStatus = DatabaseContext.CaseStatuses.Include(x => x.Answers).FirstOrDefault();

            updatedCaseStatus.Answers.Count.Should().Be(request.Answers.Count);
        }

        [Test]
        public void ThrowsAnExceptionWhenCaseStatusNotFound()
        {
            var request = CaseStatusHelper.CreateCaseStatusAnswerRequest();

            Action act = () => _caseStatusCateway.CreateCaseStatusAnswer(request);

            act.Should().Throw<CaseStatusDoesNotExistException>().WithMessage($"Case status id {request.CaseStatusId} does not exist.");
        }

        [Test]
        public void ReplaceActiveCaseStatusAnswersThrowsAnExceptionWhenCaseStatusNotFound()
        {
            var request = CaseStatusHelper.CreateCaseStatusAnswerRequest();

            Action act = () => _caseStatusCateway.ReplaceCaseStatusAnswer(request);

            act.Should().Throw<CaseStatusDoesNotExistException>().WithMessage($"Case status id {request.CaseStatusId} does not exist.");
        }

        [Test]
        public void ReplacesCurrentScheduledCaseStatusAnswers()
        {
            var personId = 123;
            var groupId1 = Guid.NewGuid().ToString();
            var groupId2 = Guid.NewGuid().ToString();

            var caseStatus = TestHelpers.CreateCaseStatus(type: "LAC", personId: personId);

            var currentActiveAnswers = TestHelpers.CreateCaseStatusAnswers(caseStatusId: caseStatus.Id, startDate: new DateTime(2021, 10, 01), min: 2, max: 2, groupId: groupId1);
            var currentScheduledAnswers = TestHelpers.CreateCaseStatusAnswers(caseStatusId: caseStatus.Id, startDate: new DateTime(2021, 10, 10), min: 2, max: 2, groupId: groupId2);

            caseStatus.Answers.AddRange(currentActiveAnswers);
            caseStatus.Answers.AddRange(currentScheduledAnswers);
            DatabaseContext.CaseStatuses.Add(caseStatus);
            DatabaseContext.SaveChanges();

            var requestAnswers = new List<CaseStatusRequestAnswers>() {
                new CaseStatusRequestAnswers()
                    {
                        Option = "placementType",
                        Value = "P1"
                    },
                new CaseStatusRequestAnswers()
                    {
                        Option = "legalStatus",
                        Value = "L1"
                    },
            };

            var request = CaseStatusHelper.CreateCaseStatusAnswerRequest(caseStatusId: caseStatus.Id, answers: requestAnswers, startDate: new DateTime(2021, 10, 03));

            _caseStatusCateway.ReplaceCaseStatusAnswer(request);

            var newAnswers = DatabaseContext.CaseStatuses.Include(x => x.Answers).FirstOrDefault().Answers;

            newAnswers.Count.Should().Be(6);

            newAnswers.Where(x => x.StartDate == request.StartDate).Count().Should().Be(2);
            newAnswers.Where(x => x.DiscardedAt != null).Count().Should().Be(2);
            newAnswers.Where(x => x.DiscardedAt == null).Count().Should().Be(4);

            var discardedScheduledAnswers = newAnswers.Where(x => x.GroupId == groupId2);
            discardedScheduledAnswers.All(x => x.DiscardedAt != null).Should().BeTrue();

            var activeAnswers = newAnswers.Where(x => x.GroupId == groupId1);
            activeAnswers.All(x => x.DiscardedAt == null).Should().BeTrue();

            var newScheduledAnsewrs = newAnswers.Where(x => x.DiscardedAt == null && x.GroupId != groupId1);
            newScheduledAnsewrs.Any(x => x.Option == "placementType" && x.Value == "P1").Should().BeTrue();
            newScheduledAnsewrs.Any(x => x.Option == "legalStatus" && x.Value == "L1").Should().BeTrue();

        }
    }
}
