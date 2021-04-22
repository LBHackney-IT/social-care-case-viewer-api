using System;
using Bogus;
using FluentAssertions;
using Microsoft.AspNetCore.Connections;
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
        private Faker _faker;
        private Mock<IWorkersUseCase> _workerUseCase;

        [SetUp]
        public void SetUp()
        {
            _faker = new Faker();
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
        public void CreateWorkerReturns404StatusWhenCreateWorkerExceptionThrown()
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
            response.StatusCode.Should().Be(404);
            response.Value.Should().Be(errorMessage);
        }

        [Test]
        public void CreateWorkerReturns400WhenNoEmailAddress()
        {
            var createWorkerRequest = TestHelpers.CreateWorkerRequest(email: "");
            var response = _workerController.CreateWorker(createWorkerRequest) as BadRequestObjectResult;

            if (response == null)
            {
                throw new NullReferenceException();
            }

            response.Should().NotBeNull();
            response.StatusCode.Should().Be(400);
        }

        [Test]
        public void CreateWorkerReturns400WhenEmailAddressTooLong()
        {
            const string longEmail = "thisEmailIsLongerThan62CharactersAndAlsoValid@HereIAmJustCreatingMoreCharacters.com";
            var createWorkerRequest = TestHelpers.CreateWorkerRequest(email: longEmail);
            var response = _workerController.CreateWorker(createWorkerRequest) as BadRequestObjectResult;

            if (response == null)
            {
                throw new NullReferenceException();
            }

            response.Should().NotBeNull();
            response.StatusCode.Should().Be(400);
        }

        [Test]
        public void CreateWorkerReturns400WhenNoFirstName()
        {
            var createWorkerRequest = TestHelpers.CreateWorkerRequest(firstName: "");
            var response = _workerController.CreateWorker(createWorkerRequest) as BadRequestObjectResult;

            if (response == null)
            {
                throw new NullReferenceException();
            }

            response.Should().NotBeNull();
            response.StatusCode.Should().Be(400);
        }

        [Test]
        public void CreateWorkerReturns400WhenFirstNameLongerThan100Character()
        {
            var createWorkerRequest = TestHelpers.CreateWorkerRequest(firstName: _faker.Random.String2(101));
            var response = _workerController.CreateWorker(createWorkerRequest) as BadRequestObjectResult;

            if (response == null)
            {
                throw new NullReferenceException();
            }

            response.Should().NotBeNull();
            response.StatusCode.Should().Be(400);
        }

        [Test]
        public void CreateWorkerReturns400WhenNoLastName()
        {
            var createWorkerRequest = TestHelpers.CreateWorkerRequest(lastName: "");
            var response = _workerController.CreateWorker(createWorkerRequest) as BadRequestObjectResult;

            if (response == null)
            {
                throw new NullReferenceException();
            }

            response.Should().NotBeNull();
            response.StatusCode.Should().Be(400);
        }

        [Test]
        public void CreateWorkerReturns400WhenLastNameLongerThan100Character()
        {
            var createWorkerRequest = TestHelpers.CreateWorkerRequest(lastName: _faker.Random.String2(101));
            var response = _workerController.CreateWorker(createWorkerRequest) as BadRequestObjectResult;

            if (response == null)
            {
                throw new NullReferenceException();
            }

            response.Should().NotBeNull();
            response.StatusCode.Should().Be(400);
        }

        [Test]
        public void CreateWorkerReturns400WhenNoContextFlag()
        {
            var createWorkerRequest = TestHelpers.CreateWorkerRequest(contextFlag: "");
            var response = _workerController.CreateWorker(createWorkerRequest) as BadRequestObjectResult;

            if (response == null)
            {
                throw new NullReferenceException();
            }

            response.Should().NotBeNull();
            response.StatusCode.Should().Be(400);
        }

        [Test]
        public void CreateWorkerReturns400WhenContextFlagLongerThan100Character()
        {
            var createWorkerRequest = TestHelpers.CreateWorkerRequest(contextFlag: _faker.Random.String2(101));
            var response = _workerController.CreateWorker(createWorkerRequest) as BadRequestObjectResult;

            if (response == null)
            {
                throw new NullReferenceException();
            }

            response.Should().NotBeNull();
            response.StatusCode.Should().Be(400);
        }

        [Test]
        public void CreateWorkerReturns400WhenNoRole()
        {
            var createWorkerRequest = TestHelpers.CreateWorkerRequest(role: "");
            var response = _workerController.CreateWorker(createWorkerRequest) as BadRequestObjectResult;

            if (response == null)
            {
                throw new NullReferenceException();
            }

            response.Should().NotBeNull();
            response.StatusCode.Should().Be(400);
        }

        [Test]
        public void CreateWorkerReturns400WhenRoleLongerThan200Character()
        {
            var createWorkerRequest = TestHelpers.CreateWorkerRequest(role: _faker.Random.String2(201));
            var response = _workerController.CreateWorker(createWorkerRequest) as BadRequestObjectResult;

            if (response == null)
            {
                throw new NullReferenceException();
            }

            response.Should().NotBeNull();
            response.StatusCode.Should().Be(400);
        }

        [Test]
        public void CreateWorkerReturns400WhenNoTeamName()
        {
            var createWorkerRequest = TestHelpers.CreateWorkerRequest(teamName: "");
            var response = _workerController.CreateWorker(createWorkerRequest) as BadRequestObjectResult;

            if (response == null)
            {
                throw new NullReferenceException();
            }

            response.Should().NotBeNull();
            response.StatusCode.Should().Be(400);
        }

        [Test]
        public void CreateWorkerReturns400WhenTeamNameLongerThan200Character()
        {
            var createWorkerRequest = TestHelpers.CreateWorkerRequest(teamName: _faker.Random.String2(201));
            var response = _workerController.CreateWorker(createWorkerRequest) as BadRequestObjectResult;

            if (response == null)
            {
                throw new NullReferenceException();
            }

            response.Should().NotBeNull();
            response.StatusCode.Should().Be(400);
        }

        [Test]
        public void CreateWorkerReturns400WhenTeamIdIsLessThan1()
        {
            var createWorkerRequest = TestHelpers.CreateWorkerRequest(teamId: 0);
            var response = _workerController.CreateWorker(createWorkerRequest) as BadRequestObjectResult;

            if (response == null)
            {
                throw new NullReferenceException();
            }

            response.Should().NotBeNull();
            response.StatusCode.Should().Be(400);
        }

        [Test]
        public void CreateWorkerReturns400WhenNoTeamsAreProvided()
        {
            var createWorkerRequest = TestHelpers.CreateWorkerRequest(createATeam: false);
            var response = _workerController.CreateWorker(createWorkerRequest) as BadRequestObjectResult;

            if (response == null)
            {
                throw new NullReferenceException();
            }

            response.Should().NotBeNull();
            response.StatusCode.Should().Be(400);
        }

        [Test]
        public void CreateWorkerReturns400WhenNoCreatedByEmailAddress()
        {
            var createWorkerRequest = TestHelpers.CreateWorkerRequest(createdByEmail: "");
            var response = _workerController.CreateWorker(createWorkerRequest) as BadRequestObjectResult;

            if (response == null)
            {
                throw new NullReferenceException();
            }

            response.Should().NotBeNull();
            response.StatusCode.Should().Be(400);
        }

        [Test]
        public void CreateWorkerReturns400WhenCreatedByEmailAddressTooLong()
        {
            const string longEmail = "thisEmailIsLongerThan62CharactersAndAlsoValid@HereIAmJustCreatingMoreCharacters.com";
            var createWorkerRequest = TestHelpers.CreateWorkerRequest(createdByEmail: longEmail);
            var response = _workerController.CreateWorker(createWorkerRequest) as BadRequestObjectResult;

            if (response == null)
            {
                throw new NullReferenceException();
            }

            response.Should().NotBeNull();
            response.StatusCode.Should().Be(400);
        }
    }
}
