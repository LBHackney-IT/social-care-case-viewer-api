using System;
using System.Collections.Generic;
using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SocialCareCaseViewerApi.Tests.V1.Helpers;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Boundary.Response;
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
        private Mock<IGetWorkersUseCase> _getWorkersUseCase;
        private Fixture _fixture;

        [SetUp]
        public void SetUp()
        {
            _workerUseCase = new Mock<IWorkersUseCase>();
            _getWorkersUseCase = new Mock<IGetWorkersUseCase>();
            _workerController = new WorkerController(_workerUseCase.Object, _getWorkersUseCase.Object);
            _fixture = new Fixture();
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

        [Test]
        public void UpdateWorkerReturns204StatusAndWorkerWhenSuccessful()
        {
            var updateWorkerRequest = TestHelpers.CreateUpdateWorkersRequest();
            _workerUseCase.Setup(x => x.ExecutePatch(updateWorkerRequest));

            var response = _workerController.EditWorker(updateWorkerRequest) as NoContentResult;

            _workerUseCase.Verify(x => x.ExecutePatch(updateWorkerRequest), Times.Once);
            if (response == null)
            {
                throw new NullReferenceException();
            }
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(204);
        }

        [Test]
        public void UpdateWorkerReturns400WhenValidationResultsIsNotValid()
        {
            var updateWorkerRequest = TestHelpers.CreateUpdateWorkersRequest(firstName: "");

            var response = _workerController.EditWorker(updateWorkerRequest) as BadRequestObjectResult;

            if (response == null)
            {
                throw new NullReferenceException();
            }
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(400);
        }

        [Test]
        public void UpdateWorkerReturns422StatusWhenUpdateWorkerExceptionThrown()
        {
            const string errorMessage = "Failed to update worker";
            var updateWorkerRequest = TestHelpers.CreateUpdateWorkersRequest();
            _workerUseCase.Setup(x => x.ExecutePatch(updateWorkerRequest))
                .Throws(new PatchWorkerException(errorMessage));

            var response = _workerController.EditWorker(updateWorkerRequest) as ObjectResult;

            if (response == null)
            {
                throw new NullReferenceException();
            }
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(422);
            response.Value.Should().Be(errorMessage);
        }

        [Test]
        public void GetWorkersReturns200WhenMatchingWorker()
        {
            var request = new GetWorkersRequest() { TeamId = 5 };
            var workersList = _fixture.Create<List<WorkerResponse>>();
            _getWorkersUseCase.Setup(x => x.Execute(request)).Returns(workersList);

            var response = _workerController.GetWorkers(request) as OkObjectResult;

            response.Should().NotBeNull();
            response.StatusCode.Should().Be(200);

            var responseValue = response.Value as List<WorkerResponse>;

            responseValue.Should().BeOfType<List<WorkerResponse>>();
            responseValue.Count.Should().Be(workersList.Count);
        }

        [Test]
        public void GetWorkersReturns404WhenNoWorkersFound()
        {
            var workers = new List<WorkerResponse>();
            var request = new GetWorkersRequest() { TeamId = 5 };
            _getWorkersUseCase.Setup(x => x.Execute(request)).Returns(workers);

            var response = _workerController.GetWorkers(request) as NotFoundResult;

            response.StatusCode.Should().Be(404);
        }
    }
}
