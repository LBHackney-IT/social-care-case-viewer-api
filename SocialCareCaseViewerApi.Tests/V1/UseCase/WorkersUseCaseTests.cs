using System.Security.Cryptography;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SocialCareCaseViewerApi.Tests.V1.Helpers;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Factories;
using SocialCareCaseViewerApi.V1.Gateways;
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

            var response =  _workersUseCase.ExecutePost(createWorkerRequest);

            response.Should().BeEquivalentTo(worker.ToDomain(true).ToResponse());
        }
    }
}
