using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SocialCareCaseViewerApi.Tests.V1.Helpers;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Boundary.Response;
using SocialCareCaseViewerApi.V1.Factories;
using SocialCareCaseViewerApi.V1.Gateways;
using SocialCareCaseViewerApi.V1.Infrastructure;
using SocialCareCaseViewerApi.V1.UseCase;
using SocialCareCaseViewerApi.V1.UseCase.Interfaces;

namespace SocialCareCaseViewerApi.Tests.V1.UseCase
{
    [TestFixture]
    public class WorkersUseCaseTests
    {
        private Mock<IDatabaseGateway> _mockDatabaseGateway;
        private IWorkersUseCase _workersUseCase;

        [SetUp]
        public void SetUp()
        {
            _mockDatabaseGateway = new Mock<IDatabaseGateway>();
            _workersUseCase = new WorkersUseCase(_mockDatabaseGateway.Object);
        }

        [Test]
        public void ExecuteGetCallsDatabaseGateway()
        {
            var fakeWorker = TestHelpers.CreateWorker();
            const int teamId = 1;
            var request = new GetWorkersRequest { WorkerId = fakeWorker.Id, Email = fakeWorker.Email, TeamId = teamId };

            _mockDatabaseGateway.Setup(x => x.GetWorkerByWorkerId(fakeWorker.Id)).Returns(fakeWorker);

            _workersUseCase.ExecuteGet(request);

            _mockDatabaseGateway.Verify(x => x.GetWorkerByWorkerId(fakeWorker.Id), Times.Once);
            _mockDatabaseGateway.Verify(x => x.GetWorkerByEmail(fakeWorker.Email), Times.Once);
            _mockDatabaseGateway.Verify(x => x.GetTeamByTeamId(teamId), Times.Once);
        }

        [Test]
        public void GetWorkerByWorkerIdReturnsListWorkersWhenWorkerWithIdExists()
        {
            var fakeWorker = TestHelpers.CreateWorker();
            var request = new GetWorkersRequest { WorkerId = fakeWorker.Id };

            _mockDatabaseGateway.Setup(x => x.GetWorkerByWorkerId(fakeWorker.Id)).Returns(fakeWorker);
            var result = _workersUseCase.ExecuteGet(request);
            var worker = result.First();

            Assert.IsInstanceOf<List<WorkerResponse>>(result);
            worker.Id.Should().Be(fakeWorker.Id);
            worker.Email.Should().Be(fakeWorker.Email);
            worker.FirstName.Should().Be(fakeWorker.FirstName);
            worker.LastName.Should().Be(fakeWorker.LastName);
            worker.Role.Should().Be(fakeWorker.Role);
        }

        [Test]
        public void GetWorkerByWorkerIdReturnsEmptyListWhenWorkerWithIdDoesNotExist()
        {
            const int nonExistentWorkerId = 2;
            var request = new GetWorkersRequest { WorkerId = nonExistentWorkerId };

            var result = _workersUseCase.ExecuteGet(request);

            result.Should().BeEmpty();
        }

        [Test]
        public void GetWorkerByWorkerEmailReturnsListWorkers()
        {
            var fakeWorker = TestHelpers.CreateWorker();
            var request = new GetWorkersRequest { Email = fakeWorker.Email };
            _mockDatabaseGateway.Setup(x => x.GetWorkerByEmail(fakeWorker.Email)).Returns(fakeWorker);

            var result = _workersUseCase.ExecuteGet(request);
            var worker = result.First();

            Assert.IsInstanceOf<List<WorkerResponse>>(result);
            worker.Id.Should().Be(fakeWorker.Id);
            worker.Email.Should().Be(fakeWorker.Email);
            worker.FirstName.Should().Be(fakeWorker.FirstName);
            worker.LastName.Should().Be(fakeWorker.LastName);
            worker.Role.Should().Be(fakeWorker.Role);
        }

        [Test]
        public void GetWorkerByWorkerEmailReturnsEmptyListWhenWorkerWithEmailDoesNotExist()
        {
            const string nonExistentWorkerEmail = "notAWorkersEmail@example.com";
            var request = new GetWorkersRequest { Email = nonExistentWorkerEmail };

            var result = _workersUseCase.ExecuteGet(request);

            result.Should().BeEmpty();
        }

        [Test]
        public void GetWorkerByTeamIdReturnsListWorkers()
        {
            const int fakeWorkerTeamId = 12345;
            const int fakeTeamId = 5678;
            var fakeWorker = TestHelpers.CreateWorker();
            var fakeTeam = new Team
            {
                WorkerTeams = new List<WorkerTeam> {new WorkerTeam()
            {
                Id = fakeWorkerTeamId,
                WorkerId = fakeWorker.Id,
                TeamId = fakeTeamId,
                Worker = fakeWorker
            }}
            };
            var request = new GetWorkersRequest
            {
                TeamId = fakeTeamId
            };
            _mockDatabaseGateway.Setup(x => x.GetTeamByTeamId(fakeTeamId)).Returns(fakeTeam);

            var result = _workersUseCase.ExecuteGet(request);
            var worker = result.First();

            Assert.IsInstanceOf<List<WorkerResponse>>(result);
            worker.Id.Should().Be(fakeWorker.Id);
            worker.Email.Should().Be(fakeWorker.Email);
            worker.FirstName.Should().Be(fakeWorker.FirstName);
            worker.LastName.Should().Be(fakeWorker.LastName);
            worker.Role.Should().Be(fakeWorker.Role);
        }

        [Test]
        public void GetSameWorkersMultipleWaysReturnsListOfDistinctWorkers()
        {
            const int fakeWorkerTeamId = 12345;
            const int fakeTeamId = 5678;
            var fakeWorker = TestHelpers.CreateWorker();
            var fakeTeam = new Team
            {
                WorkerTeams = new List<WorkerTeam> {new WorkerTeam()
                {
                    Id = fakeWorkerTeamId,
                    WorkerId = fakeWorker.Id,
                    TeamId = fakeTeamId,
                    Worker = fakeWorker
                }}
            };
            var request = new GetWorkersRequest
            {
                WorkerId = fakeWorker.Id,
                TeamId = fakeTeamId,
                Email = fakeWorker.Email
            };
            _mockDatabaseGateway.Setup(x => x.GetWorkerByWorkerId(fakeWorker.Id)).Returns(fakeWorker);
            _mockDatabaseGateway.Setup(x => x.GetWorkerByEmail(fakeWorker.Email)).Returns(fakeWorker);
            _mockDatabaseGateway.Setup(x => x.GetTeamByTeamId(fakeTeamId)).Returns(fakeTeam);

            var result = _workersUseCase.ExecuteGet(request);
            result.Count.Should().Be(1);
        }

        [Test]
        public void GetUniqueWorkersFromWorkerIdWorkerEmailAndTeamId()
        {
            var fakeWorkerId = new Worker()
            {
                Id = 1,
                Email = "fakeWorkerId@example.com",
                FirstName = "TestFirstName",
                LastName = "TestLastName",
                Role = "TestRole"
            };
            var fakeWorkerEmail = new Worker()
            {
                Id = 2,
                Email = "fakeWorkerEmail@example.com",
                FirstName = "TestFirstName",
                LastName = "TestLastName",
                Role = "TestRole"
            };
            var fakeWorkerTeam = new Worker()
            {
                Id = 3,
                Email = "fakeWorkerEmail@example.com",
                FirstName = "TestFirstName",
                LastName = "TestLastName",
                Role = "TestRole"
            };
            const int fakeWorkerTeamId = 12345;
            const int fakeTeamId = 5678;
            var fakeTeam = new Team
            {
                WorkerTeams = new List<WorkerTeam> {new WorkerTeam()
                {
                    Id = fakeWorkerTeamId,
                    WorkerId = fakeWorkerTeam.Id,
                    TeamId = fakeTeamId,
                    Worker = fakeWorkerTeam
                }}
            };

            _mockDatabaseGateway.Setup(x => x.GetWorkerByWorkerId(fakeWorkerId.Id)).Returns(fakeWorkerId);
            _mockDatabaseGateway.Setup(x => x.GetWorkerByEmail(fakeWorkerEmail.Email)).Returns(fakeWorkerEmail);
            _mockDatabaseGateway.Setup(x => x.GetTeamByTeamId(fakeTeamId)).Returns(fakeTeam);

            var request = new GetWorkersRequest
            {
                WorkerId = fakeWorkerId.Id,
                Email = fakeWorkerEmail.Email,
                TeamId = fakeTeamId
            };
            var result = _workersUseCase.ExecuteGet(request);

            result.Count.Should().Be(3);
        }

        [Test]
        public void ExecutePostCallsDatabaseGateway()
        {
            var createWorkerRequest = TestHelpers.CreateWorkerRequest();
            var worker = TestHelpers.CreateWorker(firstName: createWorkerRequest.FirstName,
                lastName: createWorkerRequest.LastName, email: createWorkerRequest.EmailAddress, role: createWorkerRequest.Role);
            _mockDatabaseGateway.Setup(x => x.CreateWorker(createWorkerRequest)).Returns(worker);

            _workersUseCase.ExecutePost(createWorkerRequest);

            _mockDatabaseGateway.Verify(x => x.CreateWorker(createWorkerRequest));
            _mockDatabaseGateway.Verify(x => x.CreateWorker(It.Is<CreateWorkerRequest>(w => w == createWorkerRequest)), Times.Once());
        }

        [Test]
        public void ExecutePostReturnsCreatedWorker()
        {
            var createWorkerRequest = TestHelpers.CreateWorkerRequest();
            var worker = TestHelpers.CreateWorker(firstName: createWorkerRequest.FirstName,
                lastName: createWorkerRequest.LastName, email: createWorkerRequest.EmailAddress, role: createWorkerRequest.Role);
            _mockDatabaseGateway.Setup(x => x.CreateWorker(createWorkerRequest)).Returns(worker);

            var response = _workersUseCase.ExecutePost(createWorkerRequest);

            response.Should().BeEquivalentTo(worker.ToDomain(true).ToResponse());
        }

        [Test]
        public void ExecutePatchCallsDatabaseGateway()
        {
            var updateWorkerRequest = TestHelpers.CreateUpdateWorkersRequest();

            _mockDatabaseGateway
                .Setup(x => x.GetWorkerByWorkerId(updateWorkerRequest.WorkerId))
                .Returns(new Worker());

            _mockDatabaseGateway.Setup(x => x.UpdateWorker(updateWorkerRequest));

            _workersUseCase.ExecutePatch(updateWorkerRequest);

            _mockDatabaseGateway.Verify(x => x.UpdateWorker(updateWorkerRequest));
            _mockDatabaseGateway.Verify(x => x.UpdateWorker(It.Is<UpdateWorkerRequest>(w => w == updateWorkerRequest)), Times.Once());
        }
    }
}
