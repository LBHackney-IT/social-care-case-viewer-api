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
    public class SubmissionControllerTests
    {
        private Mock<ISubmissionsUseCase> _submissionsUseCaseMock;

        private SubmissionController _submissionController;

        [SetUp]
        public void SetUp()
        {
            _submissionsUseCaseMock = new Mock<ISubmissionsUseCase>();
            _submissionController =
                new SubmissionController(_submissionsUseCaseMock.Object);
        }

        [Test]
        public void PostSubmissionWithValidRequestReturns201StatusAndCreatedObject()
        {
            var request = TestHelpers.CreateCaseSubmissionRequest();
            var createdSubmission = TestHelpers.CreateCaseSubmission();
            var createdSubmissionResponse = createdSubmission.ToDomain().ToResponse();
            _submissionsUseCaseMock.Setup(x => x.ExecutePost(request)).Returns((createdSubmissionResponse, createdSubmission));

            var response =
                _submissionController.CreateSubmission(request) as ObjectResult;

            response.Should().NotBeNull();
            response?.StatusCode.Should().Be(201);
            response?.Value.Should().BeEquivalentTo(createdSubmissionResponse);
        }

        [Test]
        public void PostSubmissionWithInvalidRequestReturns400Status()
        {
            var invalidRequest =
                TestHelpers
                    .CreateCaseSubmissionRequest(createdBy: "invalid email");

            var response =
                _submissionController.CreateSubmission(invalidRequest) as
                BadRequestObjectResult;

            response.Should().NotBeNull();
            response?.StatusCode.Should().Be(400);
            response?
                .Value
                .Should()
                .Be("Provide a valid email address for who created the submission");
        }

        [Test]
        public void PostSubmissionReturns422WhenWorkerNotFoundExceptionThrown()
        {
            const string errorMessage = "Failed to find worker";
            var createCaseSubmissionRequest =
                TestHelpers.CreateCaseSubmissionRequest();
            _submissionsUseCaseMock
                .Setup(x => x.ExecutePost(createCaseSubmissionRequest))
                .Throws(new WorkerNotFoundException(errorMessage));

            var response =
                _submissionController
                    .CreateSubmission(createCaseSubmissionRequest) as
                ObjectResult;

            response.Should().NotBeNull();
            response?.StatusCode.Should().Be(422);
            response?.Value.Should().Be(errorMessage);
        }

        [Test]
        public void PostSubmissionReturns422WhenPersonNotFoundExceptionThrown()
        {
            const string errorMessage = "Failed to find person";
            var createCaseSubmissionRequest =
                TestHelpers.CreateCaseSubmissionRequest();
            _submissionsUseCaseMock
                .Setup(x => x.ExecutePost(createCaseSubmissionRequest))
                .Throws(new PersonNotFoundException(errorMessage));

            var response =
                _submissionController
                    .CreateSubmission(createCaseSubmissionRequest) as
                ObjectResult;

            response.Should().NotBeNull();
            response?.StatusCode.Should().Be(422);
            response?.Value.Should().Be(errorMessage);
        }

        [Test]
        public void GetSubmissionByIdReturns200WhenACaseIsFound()
        {
            var submissionResponse = TestHelpers.CreateCaseSubmission().ToDomain().ToResponse();

            _submissionsUseCaseMock.Setup(x => x.ExecuteGetById(submissionResponse.SubmissionId)).Returns(submissionResponse);

            var response = _submissionController.GetSubmissionById(submissionResponse.SubmissionId) as ObjectResult;

            response.Should().NotBeNull();
            response?.StatusCode.Should().Be(200);
            response?.Value.Should().BeEquivalentTo(submissionResponse);
        }

        [Test]
        public void GetSubmissionByIdReturns404WhenACaseIsNotFound()
        {
            var nonExsistentCaseId = new Guid();
            var response = _submissionController.GetSubmissionById(nonExsistentCaseId) as NotFoundResult;
            response.Should().NotBeNull();
            response?.StatusCode.Should().Be(404);
        }
    }
}
