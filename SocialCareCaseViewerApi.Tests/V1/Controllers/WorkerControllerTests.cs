using System;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SocialCareCaseViewerApi.Tests.V1.Helpers;
using SocialCareCaseViewerApi.V1.Controllers;
using SocialCareCaseViewerApi.V1.Exceptions;
using SocialCareCaseViewerApi.V1.UseCase.Interfaces;

namespace SocialCareCaseViewerApi.Tests.V1.Controllers
{
    [TestFixture]
    public class WorkerControllerTests
    {
        private WorkerController _workerController;
        private Mock<IWorkersUseCase> _workerUseCase;

        [SetUp]
        public void SetUp()
        {
            _workerUseCase = new Mock<IWorkersUseCase>();
            _workerController = new WorkerController(_workerUseCase.Object);
        }

        [Test]
        public void CreateWorkerReturns201StatusAndWorkerWhenSuccessful()
        {
            var createWorkerRequest = TestHelpers.CreateWorkerRequest();
            var worker = TestHelpers.CreateWorkerResponse(firstName: createWorkerRequest.FirstName,
                lastName: createWorkerRequest.LastName, email: createWorkerRequest.EmailAddress, role: createWorkerRequest.Role);
            _workerUseCase.Setup(x => x.ExecutePost(createWorkerRequest)).Returns(worker);

            var response = _workerController.CreateWorker(createWorkerRequest) as ObjectResult;

            if (response == null)
            {
                throw new NullReferenceException();
            }
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(201);
            response.Value.Should().BeEquivalentTo(worker);
        }

        [Test]
        public void CreateWorkerReturns400WhenValidationResultsIsNotValid()
        {

            var createWorkerRequest = TestHelpers.CreateWorkerRequest(firstName: "");

            var response = _workerController.CreateWorker(createWorkerRequest) as BadRequestObjectResult;

            if (response == null)
            {
                throw new NullReferenceException();
            }
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(400);
            response.Value.Should().Be("First name must be provided");
        }

        [Test]
        public void CreateWorkerReturns422StatusWhenCreateWorkerExceptionThrown()
        {
            const string errorMessage = "Failed to create worker";
            var createWorkerRequest = TestHelpers.CreateWorkerRequest();
            _workerUseCase.Setup(x => x.ExecutePost(createWorkerRequest))
                .Throws(new PostWorkerException(errorMessage));

            var response = _workerController.CreateWorker(createWorkerRequest) as ObjectResult;

            if (response == null)
            {
                throw new NullReferenceException();
            }
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(422);
            response.Value.Should().Be(errorMessage);
        }

        // [Test]
        // public void UpdateWorkerReturns201StatusAndWorkerWhenSuccessful()
        // {
        //     var updateWorkerRequest = TestHelpers.CreateUpdateWorkersRequest();
        //     _workerUseCase.Setup(x => x.ExecutePatch(updateWorkerRequest));

        //     var response = _workerController.EditWorker(updateWorkerRequest) as NoContentResult;

        //     _workerUseCase.Verify(x => x.ExecutePatch(updateWorkerRequest), Times.Once);
        //     if (response == null)
        //     {
        //         throw new NullReferenceException();
        //     }
        //     response.Should().NotBeNull();
        //     response.StatusCode.Should().Be(204);
        // }

        // [Test]
        // public void UpdateWorkerReturns400WhenValidationResultsIsNotValid()
        // {
        //     var updateWorkerRequest = TestHelpers.CreateUpdateWorkersRequest(firstName: "");

        //     var response = _workerController.EditWorker(updateWorkerRequest) as BadRequestObjectResult;

        //     if (response == null)
        //     {
        //         throw new NullReferenceException();
        //     }
        //     response.Should().NotBeNull();
        //     response.StatusCode.Should().Be(400);
        // }

        // [Test]
        // public void UpdateWorkerReturns422StatusWhenUpdateWorkerExceptionThrown()
        // {
        //     const string errorMessage = "Failed to update worker";
        //     var updateWorkerRequest = TestHelpers.CreateUpdateWorkersRequest();
        //     _workerUseCase.Setup(x => x.ExecutePatch(updateWorkerRequest))
        //         .Throws(new PatchWorkerException(errorMessage));

        //     var response = _workerController.EditWorker(updateWorkerRequest) as ObjectResult;

        //     if (response == null)
        //     {
        //         throw new NullReferenceException();
        //     }
        //     response.Should().NotBeNull();
        //     response.StatusCode.Should().Be(422);
        //     response.Value.Should().Be(errorMessage);
        // }
    }
}
