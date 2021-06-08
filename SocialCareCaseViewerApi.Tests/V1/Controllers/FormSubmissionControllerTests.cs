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

namespace SocialCareCaseViewerApi.Tests.V1.Controllers
{
    [TestFixture]
    public class FormSubmissionControllerTests
    {
        private Mock<IFormSubmissionsUseCase> _submissionsUseCaseMock;
        private FormSubmissionController _formSubmissionController;

        [SetUp]
        public void SetUp()
        {
            _submissionsUseCaseMock = new Mock<IFormSubmissionsUseCase>();
            _formSubmissionController = new FormSubmissionController(_submissionsUseCaseMock.Object);
        }

        [Test]
        public void PostSubmissionWithValidRequestReturns201StatusAndCreatedObject()
        {
            var request = TestHelpers.CreateCaseSubmissionRequest();
            var createdSubmission = TestHelpers.CreateCaseSubmission();
            var createdSubmissionResponse = createdSubmission.ToDomain().ToResponse();
            _submissionsUseCaseMock.Setup(x => x.ExecutePost(request)).Returns((createdSubmissionResponse, createdSubmission));

            var response = _formSubmissionController.CreateSubmission(request) as ObjectResult;

            response.Should().NotBeNull();
            response?.StatusCode.Should().Be(201);
            response?.Value.Should().BeEquivalentTo(createdSubmissionResponse);
        }

        [Test]
        public void PostSubmissionWithInvalidRequestReturns400Status()
        {
            var invalidRequest = TestHelpers.CreateCaseSubmissionRequest(createdBy: "invalid email");

            var response = _formSubmissionController.CreateSubmission(invalidRequest) as BadRequestObjectResult;

            response.Should().NotBeNull();
            response?.StatusCode.Should().Be(400);
            response?.Value.Should().Be("Provide a valid email address for who created the submission");
        }

        [Test]
        public void PostSubmissionReturns422WhenWorkerNotFoundExceptionThrown()
        {
            const string errorMessage = "Failed to find worker";
            var createCaseSubmissionRequest = TestHelpers.CreateCaseSubmissionRequest();
            _submissionsUseCaseMock.Setup(x => x.ExecutePost(createCaseSubmissionRequest))
                .Throws(new WorkerNotFoundException(errorMessage));

            var response = _formSubmissionController.CreateSubmission(createCaseSubmissionRequest) as ObjectResult;

            response.Should().NotBeNull();
            response?.StatusCode.Should().Be(422);
            response?.Value.Should().Be(errorMessage);
        }

        [Test]
        public void PostSubmissionReturns422WhenPersonNotFoundExceptionThrown()
        {
            const string errorMessage = "Failed to find person";
            var createCaseSubmissionRequest = TestHelpers.CreateCaseSubmissionRequest();
            _submissionsUseCaseMock.Setup(x => x.ExecutePost(createCaseSubmissionRequest))
                .Throws(new PersonNotFoundException(errorMessage));

            var response = _formSubmissionController.CreateSubmission(createCaseSubmissionRequest) as ObjectResult;

            response.Should().NotBeNull();
            response?.StatusCode.Should().Be(422);
            response?.Value.Should().Be(errorMessage);
        }

        [Test]
        public void GetSubmissionByIdReturns200WhenACaseIsFound()
        {
            var submissionResponse = TestHelpers.CreateCaseSubmission().ToDomain().ToResponse();
            _submissionsUseCaseMock.Setup(x => x.ExecuteGetById(submissionResponse.SubmissionId)).Returns(submissionResponse);

            var response = _formSubmissionController.GetSubmissionById(submissionResponse.SubmissionId) as ObjectResult;

            response.Should().NotBeNull();
            response?.StatusCode.Should().Be(200);
            response?.Value.Should().BeEquivalentTo(submissionResponse);
        }

        [Test]
        public void GetSubmissionByIdReturns404WhenACaseIsNotFound()
        {
            var nonExistentSubmissionId = new Guid();

            var response = _formSubmissionController.GetSubmissionById(nonExistentSubmissionId) as NotFoundResult;

            response.Should().NotBeNull();
            response?.StatusCode.Should().Be(404);
        }

        [Test]
        public void FinishSubmissionReturns204WhenACaseIsSuccesfullyFinished()
        {
            var request = TestHelpers.FinishCaseSubmissionRequest();
            var createdSubmission = TestHelpers.CreateCaseSubmission();

            var response = _formSubmissionController.FinishSubmission(createdSubmission.SubmissionId, request) as NoContentResult;

            response.Should().NotBeNull();
            response?.StatusCode.Should().Be(204);
        }

        [Test]
        public void FinishSubmissionWithInvalidRequestReturns400Status()
        {
            var createdSubmission = TestHelpers.CreateCaseSubmission();
            var invalidRequest = TestHelpers.FinishCaseSubmissionRequest(createdBy: "invalid email");

            var response = _formSubmissionController.FinishSubmission(createdSubmission.SubmissionId, invalidRequest) as BadRequestObjectResult;

            response.Should().NotBeNull();
            response?.StatusCode.Should().Be(400);
            response?.Value.Should().Be("Provide a valid email address for who is finishing the submission");
        }

        [Test]
        public void FinishSubmissionReturns422WhenWorkerNotFoundExecpetionThrown()
        {
            const string errorMessage = "Failed to find worker";
            var createdSubmission = TestHelpers.CreateCaseSubmission();
            var request = TestHelpers.FinishCaseSubmissionRequest();
            _submissionsUseCaseMock.Setup(x => x.ExecuteFinishSubmission(createdSubmission.SubmissionId, request)).Throws(new WorkerNotFoundException(errorMessage));

            var response = _formSubmissionController.FinishSubmission(createdSubmission.SubmissionId, request) as ObjectResult;

            response.Should().NotBeNull();
            response?.StatusCode.Should().Be(422);
            response?.Value.Should().Be(errorMessage);
        }

        public void FinishSubmissionReturns422WhenGetSubmissionExecpetionThrown()
        {
            const string errorMessage = "Failed to find submission";
            var createdSubmission = TestHelpers.CreateCaseSubmission();
            var request = TestHelpers.FinishCaseSubmissionRequest();
            _submissionsUseCaseMock.Setup(x => x.ExecuteFinishSubmission(createdSubmission.SubmissionId, request)).Throws(new GetSubmissionException(errorMessage));

            var response = _formSubmissionController.FinishSubmission(createdSubmission.SubmissionId, request) as ObjectResult;

            response.Should().NotBeNull();
            response?.StatusCode.Should().Be(422);
            response?.Value.Should().Be(errorMessage);
        }
    }
}
