using System;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SocialCareCaseViewerApi.Tests.V1.Helpers;
using SocialCareCaseViewerApi.V1.Controllers;
using SocialCareCaseViewerApi.V1.Exceptions;
using SocialCareCaseViewerApi.V1.Factories;
using SocialCareCaseViewerApi.V1.UseCase.Interfaces;

namespace SocialCareCaseViewerApi.Tests.V1.Controllers.CaseStatus
{
    [TestFixture]
    public class UpdateCaseStatusControllerTest
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
        public void UpdateCaseStatusReturns200andUpdateCaseStatusOnSuccess()
        {
            var resident = TestHelpers.CreatePerson();
            var request = TestHelpers.CreateUpdateCaseStatusRequest();
            var mockResponse = TestHelpers.CreateCaseStatus(personId: resident.Id, startDate: DateTime.Now.AddDays(-1),
                endDate: DateTime.Now.AddDays(1)).ToDomain().ToResponse();

            _mockCaseStatusesUseCase
                .Setup(x => x.ExecuteUpdate(request))
                .Returns(mockResponse);

            var response = _caseStatusController.UpdateCaseStatus(request) as ObjectResult;

            response?.StatusCode.Should().Be(200);
            response?.Value.Should().BeEquivalentTo(mockResponse);
        }

        [Test]
        public void UpdateCaseStatusReturns404WithErrorMessageWhenCaseStatusDoesNotExistExceptionThrown()
        {
            var resident = TestHelpers.CreatePerson();
            var request = TestHelpers.CreateUpdateCaseStatusRequest();
            const string errorMessage = "error-message";

            _mockCaseStatusesUseCase
                .Setup(x => x.ExecuteUpdate(request))
                .Throws(new CaseStatusDoesNotExistException(errorMessage));

            var response = _caseStatusController.UpdateCaseStatus(request) as ObjectResult;

            response?.StatusCode.Should().Be(404);
            response?.Value.Should().Be(errorMessage);
        }

        [Test]
        public void UpdateCaseStatusReturns400WithErrorMessageWhenWorkerNotFoundExceptionThrown()
        {
            var resident = TestHelpers.CreatePerson();
            var request = TestHelpers.CreateUpdateCaseStatusRequest();
            const string errorMessage = "error-message";

            _mockCaseStatusesUseCase
                .Setup(x => x.ExecuteUpdate(request))
                .Throws(new WorkerNotFoundException(errorMessage));

            var response = _caseStatusController.UpdateCaseStatus(request) as ObjectResult;

            response?.StatusCode.Should().Be(400);
            response?.Value.Should().Be(errorMessage);
        }

        [Test]
        public void UpdateCaseStatusReturns400WithErrorMessageWhenPersonNotFoundExceptionThrown()
        {
            var resident = TestHelpers.CreatePerson();
            var request = TestHelpers.CreateUpdateCaseStatusRequest();
            const string errorMessage = "error-message";

            _mockCaseStatusesUseCase
                .Setup(x => x.ExecuteUpdate(request))
                .Throws(new PersonNotFoundException(errorMessage));

            var response = _caseStatusController.UpdateCaseStatus(request) as ObjectResult;

            response?.StatusCode.Should().Be(400);
            response?.Value.Should().Be(errorMessage);
        }

        [Test]
        public void UpdateCaseStatusReturns400WithErrorMessageWhenCaseStatusDoesNotMatchPersonExceptionThrown()
        {
            var resident = TestHelpers.CreatePerson();
            var request = TestHelpers.CreateUpdateCaseStatusRequest();
            const string errorMessage = "error-message";

            _mockCaseStatusesUseCase
                .Setup(x => x.ExecuteUpdate(request))
                .Throws(new CaseStatusDoesNotMatchPersonException(errorMessage));

            var response = _caseStatusController.UpdateCaseStatus(request) as ObjectResult;

            response?.StatusCode.Should().Be(400);
            response?.Value.Should().Be(errorMessage);
        }
    }
}
