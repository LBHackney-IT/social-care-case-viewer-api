using System.Collections.Generic;
using Bogus;
using Moq;
using NUnit.Framework;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Boundary.Response;
using SocialCareCaseViewerApi.V1.Domain;
using SocialCareCaseViewerApi.V1.Gateways;
using SocialCareCaseViewerApi.V1.UseCase;
using DbWorker = SocialCareCaseViewerApi.V1.Infrastructure.Worker;
using dbTeam = SocialCareCaseViewerApi.V1.Infrastructure.Team;

namespace SocialCareCaseViewerApi.Tests.V1.UseCase
{
    [TestFixture]
    public class WorkersUseCaseTests
    {
        private Mock<IDatabaseGateway> _mockDataBaseGateway;
        private WorkersUseCase _workersUseCase;
        private Faker _faker;

        [SetUp]
        public void SetUp()
        {
            _mockDataBaseGateway = new Mock<IDatabaseGateway>();
            _workersUseCase = new WorkersUseCase(_mockDataBaseGateway.Object);
            _faker = new Faker();
        }

        [Test]
        public void GetWorkerByWorkerIdReturnsListWorkersReponseObject()
        {
            var request = new ListWorkersRequest();

            var response = new ListWorkersResponse();

            _mockDataBaseGateway.Setup(x => x.GetWorker(It.IsAny<int>())).Returns(new DbWorker());

            var result = _workersUseCase.ExecuteGet(request);

            Assert.IsInstanceOf<ListWorkersResponse>(result);
            Assert.IsInstanceOf<List<Worker>>(result.Workers);
        }

        [Test]
        public void GetWorkersByTeamIdReturnsListWorkersReponseObject()
        {
            var request = new ListWorkersRequest();

            var response = new ListWorkersResponse();

            _mockDataBaseGateway.Setup(x => x.GetWorkersByTeamId(It.IsAny<int>())).Returns(new List<dbTeam>());

            var result = _workersUseCase.ExecuteGet(request);

            Assert.IsInstanceOf<ListWorkersResponse>(result);
            Assert.IsInstanceOf<List<Worker>>(result.Workers);
        }
    }
}
