using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using SocialCareCaseViewerApi.Tests.V1.Helpers;
using SocialCareCaseViewerApi.V1.Exceptions;
using SocialCareCaseViewerApi.V1.Gateways;
using SocialCareCaseViewerApi.V1.Helpers;
using System;
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
    }
}
