using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SocialCareCaseViewerApi.Tests.V1.Helpers;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Controllers;
using SocialCareCaseViewerApi.V1.Exceptions;
using SocialCareCaseViewerApi.V1.Factories;
using SocialCareCaseViewerApi.V1.UseCase.Interfaces;

#nullable enable
namespace SocialCareCaseViewerApi.Tests.V1.Controllers.CaseStatus
{
    [TestFixture]
    public class CreateCaseStatusAnswerTests
    {
        private CaseStatusController _caseStatusController = null!;
        private Mock<ICaseStatusesUseCase> _mockCaseStatusesUseCase = null!;
        private CreateCaseStatusAnswerRequest _request = null!;

        [SetUp]
        public void SetUp()
        {
            _mockCaseStatusesUseCase = new Mock<ICaseStatusesUseCase>();
            _caseStatusController = new CaseStatusController(_mockCaseStatusesUseCase.Object);
            _request = CaseStatusHelper.CreateCaseStatusAnswerRequest(answers: CaseStatusHelper.CreateCaseStatusRequestAnswers(min: 2, max: 2)); //TODO: use DI for validator setup for easy mocking
        }

        [Test]
        public void CallsCaseStatusesUseCaseToCreateCaseStatusAnswers()
        {
            _mockCaseStatusesUseCase.Setup(x => x.ExecutePostCaseStatusAnswer(It.IsAny<CreateCaseStatusAnswerRequest>()));

            _caseStatusController.CreateCaseStatusAnswers(_request);

            _mockCaseStatusesUseCase.Verify(x => x.ExecutePostCaseStatusAnswer(_request));
        }

        [Test]
        public void WhenWorkerNotFoundExceptionIsThrownReturns400()
        {
            var exceptionMessage = "worker not found";
            _mockCaseStatusesUseCase.Setup(x => x.ExecutePostCaseStatusAnswer(It.IsAny<CreateCaseStatusAnswerRequest>())).Throws(new WorkerNotFoundException(exceptionMessage));

            var response = _caseStatusController.CreateCaseStatusAnswers(_request) as BadRequestObjectResult;

            response?.StatusCode.Should().Be(400);

            response?.Value.Should().Be(exceptionMessage);
        }

        [Test]
        public void WhenCaseStatusDoesNotExistExceptionIsThrownReturns400()
        {
            var exceptionMessage = "case status not found";
            _mockCaseStatusesUseCase.Setup(x => x.ExecutePostCaseStatusAnswer(It.IsAny<CreateCaseStatusAnswerRequest>())).Throws(new CaseStatusDoesNotExistException(exceptionMessage));

            var response = _caseStatusController.CreateCaseStatusAnswers(_request) as BadRequestObjectResult;

            response?.StatusCode.Should().Be(400);
            response?.Value.Should().Be(exceptionMessage);
        }

        [Test]
        public void WhenInvalidCaseStatusTypeExceptionIsThrownReturn400()
        {
            var exceptionMessage = "invalid case status type";

            _mockCaseStatusesUseCase.Setup(x => x.ExecutePostCaseStatusAnswer(It.IsAny<CreateCaseStatusAnswerRequest>())).Throws(new InvalidCaseStatusTypeException(exceptionMessage));

            var response = _caseStatusController.CreateCaseStatusAnswers(_request) as BadRequestObjectResult;

            response?.StatusCode.Should().Be(400);
            response?.Value.Should().Be(exceptionMessage);
        }

        [Test]
        public void WhenRequestIsValidReturnsSuccessfulResponse()
        {
            var caseStatusDomain = CaseStatusHelper.CreateCaseStatus().ToDomain();

            _mockCaseStatusesUseCase.Setup(x => x.ExecutePostCaseStatusAnswer(_request)).Returns(caseStatusDomain.ToResponse());

            var response = _caseStatusController.CreateCaseStatusAnswers(_request) as ObjectResult;

            response?.StatusCode.Should().Be(201);
            response?.Value.Should().Be("Successfully created case status answers.");
        }
    }
}
