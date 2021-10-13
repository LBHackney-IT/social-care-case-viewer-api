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

namespace SocialCareCaseViewerApi.Tests.V1.Controllers.CaseStatus
{
    [TestFixture]
    public class CaseStatusCreateTests
    {
        private CaseStatusController _caseStatusController;
        private Mock<ICaseStatusesUseCase> _mockCaseStatusesUseCase;

        [SetUp]
        public void SetUp()
        {
            _mockCaseStatusesUseCase = new Mock<ICaseStatusesUseCase>();

            _caseStatusController = new CaseStatusController(_mockCaseStatusesUseCase.Object);
        }


        [Test]
        public void CallsCaseStatusUseCaseToCreateCaseStatus()
        {
            _mockCaseStatusesUseCase.Setup(x => x.ExecutePost(It.IsAny<CreateCaseStatusRequest>()));
            var request = CaseStatusHelper.CreateCaseStatusRequest();

            _caseStatusController.CreateCaseStatus(request);

            _mockCaseStatusesUseCase.Verify(x => x.ExecutePost(request));
        }

        [Test]
        public void WhenPersonNotFoundExceptionIsThrownReturns400WithMessage()
        {
            var exceptionMessage = "error message";
            _mockCaseStatusesUseCase.Setup(x => x.ExecutePost(It.IsAny<CreateCaseStatusRequest>()))
                .Throws(new PersonNotFoundException(exceptionMessage));

            var request = CaseStatusHelper.CreateCaseStatusRequest();

            var response = _caseStatusController.CreateCaseStatus(request) as BadRequestObjectResult;

            response?.StatusCode.Should().Be(400);
            response?.Value.Should().Be(exceptionMessage);
        }

        [Test]
        public void WhenCaseTypeAlreadyExistsExceptionIsThrownReturns400WithMessage()
        {
            var exceptionMessage = "error message";
            _mockCaseStatusesUseCase.Setup(x => x.ExecutePost(It.IsAny<CreateCaseStatusRequest>()))
                .Throws(new CaseStatusAlreadyExistsException(exceptionMessage));
            var request = CaseStatusHelper.CreateCaseStatusRequest();

            var response = _caseStatusController.CreateCaseStatus(request) as BadRequestObjectResult;

            response?.StatusCode.Should().Be(400);
            response?.Value.Should().Be(exceptionMessage);
        }

        [Test]
        public void WhenRequestIsValidReturnsSuccessfulResponse()
        {
            var caseStatus = CaseStatusHelper.CreateCaseStatus();
            var caseStatusDomain = caseStatus.ToDomain();
            var request = CaseStatusHelper.CreateCaseStatusRequest(caseStatus.PersonId);
            _mockCaseStatusesUseCase
                .Setup(x => x.ExecutePost(request))
                .Returns(caseStatusDomain);

            var response = _caseStatusController.CreateCaseStatus(request) as ObjectResult;

            _mockCaseStatusesUseCase.Verify(x => x.ExecutePost(request));
            response?.StatusCode.Should().Be(201);
            response?.Value.Should().Be("Successfully created case status.");
        }
    }
}
