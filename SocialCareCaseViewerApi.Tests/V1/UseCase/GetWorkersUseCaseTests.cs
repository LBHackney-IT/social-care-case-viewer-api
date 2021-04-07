using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Boundary.Response;
using SocialCareCaseViewerApi.V1.Gateways;
using SocialCareCaseViewerApi.V1.Infrastructure;
using SocialCareCaseViewerApi.V1.UseCase;
using SocialCareCaseViewerApi.V1.UseCase.Interfaces;

namespace SocialCareCaseViewerApi.Tests.V1.UseCase
{
    [TestFixture]
    public class GetWorkersUseCaseTests
    {
        private Mock<IDatabaseGateway> _mockDataBaseGateway;
        private IGetWorkersUseCase _getWorkersUseCase;

        [SetUp]
        public void SetUp()
        {
            _mockDataBaseGateway = new Mock<IDatabaseGateway>();
            _getWorkersUseCase = new GetWorkersUseCase(_mockDataBaseGateway.Object);
        }

        [Test]
        public void GetWorkerByWorkerIdReturnsListWorkersWhenWorkerWithIdExists()
        {
            var fakeWorker = CreateFakeWorker();
            var request = new GetWorkersRequest { WorkerId = fakeWorker.Id };

            _mockDataBaseGateway.Setup(x => x.GetWorkerByWorkerId(fakeWorker.Id)).Returns(fakeWorker);
            var result = _getWorkersUseCase.Execute(request);
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

            var result = _getWorkersUseCase.Execute(request);

            result.Should().BeEmpty();
        }

        [Test]
        public void GetWorkerByWorkerEmailReturnsListWorkers()
        {
            var fakeWorker = CreateFakeWorker();
            var request = new GetWorkersRequest { Email = fakeWorker.Email };
            _mockDataBaseGateway.Setup(x => x.GetWorkerByEmail(fakeWorker.Email)).Returns(fakeWorker);

            var result = _getWorkersUseCase.Execute(request);
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

            var result = _getWorkersUseCase.Execute(request);

            result.Should().BeEmpty();
        }

        [Test]
        public void GetWorkerByTeamIdReturnsListWorkers()
        {
            const int fakeWorkerTeamId = 12345;
            const int fakeTeamId = 5678;
            var fakeWorker = CreateFakeWorker();
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
            _mockDataBaseGateway.Setup(x => x.GetTeamsByTeamId(fakeTeamId)).Returns(new List<Team> { fakeTeam });

            var result = _getWorkersUseCase.Execute(request);
            var worker = result.First();

            Assert.IsInstanceOf<List<WorkerResponse>>(result);
            worker.Id.Should().Be(fakeWorker.Id);
            worker.Email.Should().Be(fakeWorker.Email);
            worker.FirstName.Should().Be(fakeWorker.FirstName);
            worker.LastName.Should().Be(fakeWorker.LastName);
            worker.Role.Should().Be(fakeWorker.Role);
        }

        [Test]
        public void GetWorkerByTeamIdReturnsEmptyListWhenWorkerTeamIdDoesNotExist()
        {
            const int fakeWorkerTeamId = 12345;
            const int fakeTeamId = 5678;
            const int nonExistentTeamId = 98765;
            var fakeWorker = CreateFakeWorker();
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
            _mockDataBaseGateway.Setup(x => x.GetTeamsByTeamId(nonExistentTeamId)).Returns(new List<Team> { fakeTeam });

            var result = _getWorkersUseCase.Execute(request);

            result.Should().BeEmpty();
        }

        [Test]
        public void GetSameWorkersMultipleWaysReturnsListOfDistinctWorkers()
        {
            const int fakeWorkerTeamId = 12345;
            const int fakeTeamId = 5678;
            var fakeWorker = CreateFakeWorker();
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
            _mockDataBaseGateway.Setup(x => x.GetWorkerByWorkerId(fakeWorker.Id)).Returns(fakeWorker);
            _mockDataBaseGateway.Setup(x => x.GetWorkerByEmail(fakeWorker.Email)).Returns(fakeWorker);
            _mockDataBaseGateway.Setup(x => x.GetTeamsByTeamId(fakeTeamId)).Returns(new List<Team> { fakeTeam });

            var result = _getWorkersUseCase.Execute(request);
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

            _mockDataBaseGateway.Setup(x => x.GetWorkerByWorkerId(fakeWorkerId.Id)).Returns(fakeWorkerId);
            _mockDataBaseGateway.Setup(x => x.GetWorkerByEmail(fakeWorkerEmail.Email)).Returns(fakeWorkerEmail);
            _mockDataBaseGateway.Setup(x => x.GetTeamsByTeamId(fakeTeamId)).Returns(new List<Team> { fakeTeam });

            var request = new GetWorkersRequest
            {
                WorkerId = fakeWorkerId.Id,
                Email = fakeWorkerEmail.Email,
                TeamId = fakeTeamId
            };
            var result = _getWorkersUseCase.Execute(request);

            result.Count.Should().Be(3);
        }

        private static Worker CreateFakeWorker(
            int id = 1,
            string email = "fakeemail@example.com",
            string firstName = "TestFirstName",
            string lastName = "TestLastName",
            string role = "TestRole",
            ICollection<WorkerTeam> workerTeams = null,
            ICollection<AllocationSet> allocations = null
            )
        {
            return new Worker()
            {
                Id = id,
                Email = email,
                FirstName = firstName,
                LastName = lastName,
                Role = role,
                WorkerTeams = workerTeams,
                Allocations = allocations
            };
        }
    }
}
