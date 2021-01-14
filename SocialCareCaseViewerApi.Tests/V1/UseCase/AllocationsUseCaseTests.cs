using Moq;
using NUnit.Framework;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Boundary.Response;
using SocialCareCaseViewerApi.V1.Gateways;
using SocialCareCaseViewerApi.V1.UseCase;

namespace SocialCareCaseViewerApi.Tests.V1.UseCase
{
    [TestFixture]
    public class AllocationsUseCaseTests
    {
        private Mock<IDatabaseGateway> _mockDatabaseGateway;
        private AllocationsUseCase _allocationsUseCase;

        [SetUp]
        public void SetUp()
        {
            _mockDatabaseGateway = new Mock<IDatabaseGateway>();
            _allocationsUseCase = new AllocationsUseCase(_mockDatabaseGateway.Object);
        }

        [Test]
        public void UpdateAllocationCallsDatabaseGateway()
        {
            UpdateAllocationRequest request = new UpdateAllocationRequest() { Id = 1, DeallocationReason = "reason" };

            _allocationsUseCase.ExecuteUpdate(request);

            _mockDatabaseGateway.Verify(x => x.UpdateAllocation(request));
        }

        [Test]
        public void UpdateAllocationCallsDatabaseGatewayWithParameters()
        {
            UpdateAllocationRequest request = new UpdateAllocationRequest() { Id = 1 };

            _allocationsUseCase.ExecuteUpdate(request);

            _mockDatabaseGateway.Verify(x => x.UpdateAllocation(It.Is<UpdateAllocationRequest>(x => x == request)), Times.Once);
        }

        [Test]
        public void UpdateAllocationReturnsCorrectIntValue()
        {
            UpdateAllocationRequest request = new UpdateAllocationRequest() { Id = 1 };

            _mockDatabaseGateway.Setup(x => x.UpdateAllocation(It.Is<UpdateAllocationRequest>(x => x == request))).Returns(new UpdateAllocationResponse());

            var response = _allocationsUseCase.ExecuteUpdate(request);

            Assert.AreEqual(1, response);
        }
    }
}
