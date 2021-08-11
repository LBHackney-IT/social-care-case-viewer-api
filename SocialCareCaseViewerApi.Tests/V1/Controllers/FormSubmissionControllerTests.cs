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

            response?.StatusCode.Should().Be(201);
            response?.Value.Should().BeEquivalentTo(createdSubmissionResponse);
        }

        [Test]
        public void PostSubmissionWithInvalidRequestReturns400Status()
        {
            var invalidRequest = TestHelpers.CreateCaseSubmissionRequest(createdBy: "invalid email");

            var response = _formSubmissionController.CreateSubmission(invalidRequest) as BadRequestObjectResult;

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

            response?.StatusCode.Should().Be(422);
            response?.Value.Should().Be(errorMessage);
        }

        [Test]
        public void GetSubmissionByIdReturns200WhenACaseIsFound()
        {
            var submissionResponse = TestHelpers.CreateCaseSubmission().ToDomain().ToResponse();
            _submissionsUseCaseMock.Setup(x => x.ExecuteGetById(submissionResponse.SubmissionId)).Returns(submissionResponse);

            var response = _formSubmissionController.GetSubmissionById(submissionResponse.SubmissionId) as ObjectResult;

            response?.StatusCode.Should().Be(200);
            response?.Value.Should().BeEquivalentTo(submissionResponse);
        }

        [Test]
        public void GetSubmissionByIdReturns404WhenACaseIsNotFound()
        {
            var response = _formSubmissionController.GetSubmissionById("1234") as NotFoundResult;

            response?.StatusCode.Should().Be(404);
        }

        [Test]
        public void UpdateSubmissionReturns201WhenACaseIsSuccessfullyFinished()
        {
            var request = TestHelpers.UpdateCaseSubmissionRequest();
            var createdSubmission = TestHelpers.CreateCaseSubmission();
            var submissionResponse = createdSubmission.ToDomain().ToResponse();
            _submissionsUseCaseMock.Setup(x => x.ExecuteUpdateSubmission(createdSubmission.SubmissionId.ToString(), request)).Returns(submissionResponse);

            var response = _formSubmissionController.UpdateSubmission(createdSubmission.SubmissionId.ToString(), request) as ObjectResult;

            response?.StatusCode.Should().Be(200);
            response?.Value.Should().BeEquivalentTo(submissionResponse);
        }

        [Test]
        public void UpdateSubmissionWithInvalidRequestReturns400Status()
        {
            var createdSubmission = TestHelpers.CreateCaseSubmission();
            var invalidRequest = TestHelpers.UpdateCaseSubmissionRequest(updatedBy: "invalid email");

            var response = _formSubmissionController.UpdateSubmission(createdSubmission.SubmissionId.ToString(), invalidRequest) as BadRequestObjectResult;

            response?.StatusCode.Should().Be(400);
            response?.Value.Should().Be("Provide a valid email address for who is updating the submission");
        }

        [Test]
        public void UpdateSubmissionReturns422WhenWorkerNotFoundExceptionThrown()
        {
            const string errorMessage = "Failed to find worker";
            var createdSubmission = TestHelpers.CreateCaseSubmission();
            var request = TestHelpers.UpdateCaseSubmissionRequest();
            _submissionsUseCaseMock.Setup(x => x.ExecuteUpdateSubmission(createdSubmission.SubmissionId.ToString(), request)).Throws(new WorkerNotFoundException(errorMessage));

            var response = _formSubmissionController.UpdateSubmission(createdSubmission.SubmissionId.ToString(), request) as ObjectResult;

            response?.StatusCode.Should().Be(422);
            response?.Value.Should().Be(errorMessage);
        }

        [Test]
        public void UpdateSubmissionReturns422WhenUpdateSubmissionExceptionThrown()
        {
            const string errorMessage = "Resident not found";
            var createdSubmission = TestHelpers.CreateCaseSubmission();
            var request = TestHelpers.UpdateCaseSubmissionRequest();
            _submissionsUseCaseMock.Setup(x => x.ExecuteUpdateSubmission(createdSubmission.SubmissionId.ToString(), request)).Throws(new UpdateSubmissionException(errorMessage));

            var response = _formSubmissionController.UpdateSubmission(createdSubmission.SubmissionId.ToString(), request) as ObjectResult;

            response?.StatusCode.Should().Be(422);
            response?.Value.Should().Be(errorMessage);
        }

        [Test]
        public void EditSubmissionAnswersReturns200AndUpdatedResponse()
        {
            var createdSubmission = TestHelpers.CreateCaseSubmission().ToDomain().ToResponse();
            const string stepId = "1";
            var updateFormSubmissionAnswersRequest = TestHelpers.CreateUpdateFormSubmissionAnswersRequest();
            _submissionsUseCaseMock.Setup(x => x.UpdateAnswers(createdSubmission.SubmissionId, stepId, updateFormSubmissionAnswersRequest)).Returns(createdSubmission);

            var response = _formSubmissionController.EditSubmissionAnswers(createdSubmission.SubmissionId, stepId, updateFormSubmissionAnswersRequest) as ObjectResult;

            response?.StatusCode.Should().Be(200);
            response?.Value.Should().BeEquivalentTo(createdSubmission);
        }

        [Test]
        public void EditSubmissionAnswersReturns400WhenGivenInvalidRequest()
        {
            var createdSubmission = TestHelpers.CreateCaseSubmission().ToDomain().ToResponse();
            const string stepId = "1";
            var updateFormSubmissionAnswersRequest = TestHelpers.CreateUpdateFormSubmissionAnswersRequest(editedBy: "not_a_valid_email");
            _submissionsUseCaseMock.Setup(x => x.UpdateAnswers(createdSubmission.SubmissionId, stepId, updateFormSubmissionAnswersRequest)).Returns(createdSubmission);

            var response = _formSubmissionController.EditSubmissionAnswers(createdSubmission.SubmissionId, stepId, updateFormSubmissionAnswersRequest) as ObjectResult;

            response?.StatusCode.Should().Be(400);
        }

        [Test]
        public void EditSubmissionAnswersReturns422WhenGetSubmissionExceptionThrown()
        {
            var createdSubmission = TestHelpers.CreateCaseSubmission();
            const string errorMessage = "Failed to find submission";
            var updateFormSubmissionAnswersRequest = TestHelpers.CreateUpdateFormSubmissionAnswersRequest();
            const string stepId = "1";
            _submissionsUseCaseMock.Setup(x => x.UpdateAnswers(createdSubmission.SubmissionId.ToString(), stepId, updateFormSubmissionAnswersRequest))
                .Throws(new GetSubmissionException(errorMessage));

            var response = _formSubmissionController.EditSubmissionAnswers(createdSubmission.SubmissionId.ToString(), stepId, updateFormSubmissionAnswersRequest) as ObjectResult;

            response?.StatusCode.Should().Be(422);
            response?.Value.Should().Be(errorMessage);
        }

        [Test]
        public void FinishSubmissionReturns422WhenGetSubmissionExceptionThrown()
        {
            const string errorMessage = "Failed to find submission";
            var createdSubmission = TestHelpers.CreateCaseSubmission();
            var request = TestHelpers.UpdateCaseSubmissionRequest();
            _submissionsUseCaseMock.Setup(x => x.ExecuteUpdateSubmission(createdSubmission.SubmissionId.ToString(), request)).Throws(new GetSubmissionException(errorMessage));

            var response = _formSubmissionController.UpdateSubmission(createdSubmission.SubmissionId.ToString(), request) as ObjectResult;

            response?.StatusCode.Should().Be(422);
            response?.Value.Should().Be(errorMessage);
        }

        [Test]
        public void EditSubmissionAnswersReturns422WhenGetWorkerNotFoundExceptionThrown()
        {
            var createdSubmission = TestHelpers.CreateCaseSubmission();
            const string errorMessage = "Failed to find worker";
            var updateFormSubmissionAnswersRequest = TestHelpers.CreateUpdateFormSubmissionAnswersRequest();
            const string stepId = "1";
            _submissionsUseCaseMock.Setup(x => x.UpdateAnswers(createdSubmission.SubmissionId.ToString(), stepId, updateFormSubmissionAnswersRequest))
                .Throws(new WorkerNotFoundException(errorMessage));

            var response = _formSubmissionController.EditSubmissionAnswers(createdSubmission.SubmissionId.ToString(), stepId, updateFormSubmissionAnswersRequest) as ObjectResult;

            response?.StatusCode.Should().Be(422);
            response?.Value.Should().Be(errorMessage);
        }
    }
}
