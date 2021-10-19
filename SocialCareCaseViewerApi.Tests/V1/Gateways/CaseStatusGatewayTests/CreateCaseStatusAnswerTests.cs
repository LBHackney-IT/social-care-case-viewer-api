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
        public void ReplacesActiveCaseStatusAnswers()
        {
            var personId = 123;
            var groupId1 = Guid.NewGuid().ToString();
            var groupId2 = Guid.NewGuid().ToString();

            var caseStatus = TestHelpers.CreateCaseStatus(type: "LAC", personId: personId);

            var currentActiveAnswers = TestHelpers.CreateCaseStatusAnswers(caseStatusId: caseStatus.Id, startDate: DateTime.Now.AddDays(-10), min: 2, max: 2, groupId: groupId1);
            var currentFutureAnswers = TestHelpers.CreateCaseStatusAnswers(caseStatusId: caseStatus.Id, startDate: DateTime.Now.AddDays(5), min: 2, max: 2, groupId: groupId2);

            caseStatus.Answers.AddRange(currentActiveAnswers);
            caseStatus.Answers.AddRange(currentFutureAnswers);
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

            var request = CaseStatusHelper.CreateCaseStatusAnswerRequest(caseStatusId: caseStatus.Id, answers: requestAnswers);

            _caseStatusCateway.ReplaceCaseStatusAnswer(request);

            var newAnswers = DatabaseContext.CaseStatuses.Include(x => x.Answers).FirstOrDefault().Answers;

            newAnswers.Count.Should().Be(6); //2 new, 2 originals, 2 discardded

            newAnswers.Where(x => x.StartDate == request.StartDate).Count().Should().Be(2);
            newAnswers.Where(x => x.DiscardedAt != null).Count().Should().Be(2);
            newAnswers.Where(x => x.DiscardedAt == null).Count().Should().Be(4);
        }
    }
}
