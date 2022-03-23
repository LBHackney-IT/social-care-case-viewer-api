using System.Collections.Generic;
using System.Linq;
using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Boundary.Response;
using SocialCareCaseViewerApi.V1.Gateways;
using SocialCareCaseViewerApi.V1.Gateways.Interfaces;
using SocialCareCaseViewerApi.V1.UseCase;

namespace SocialCareCaseViewerApi.Tests.V1.UseCase
{
    [TestFixture]
    public class AllocationsUseCaseTests
    {
        private Mock<IDatabaseGateway> _mockDatabaseGateway;
        private AllocationsUseCase _allocationsUseCase;
        private readonly Fixture _fixture = new Fixture();

        [SetUp]
        public void SetUp()
        {
            _mockDatabaseGateway = new Mock<IDatabaseGateway>();
            _allocationsUseCase = new AllocationsUseCase(_mockDatabaseGateway.Object);
        }

        [Test]
        public void UpdateAllocationCallsDatabaseGateway()
        {
            UpdateAllocationRequest request = new UpdateAllocationRequest() { Id = 1, DeallocationReason = "reason", RagRating = null };

            _allocationsUseCase.ExecuteUpdate(request);

            _mockDatabaseGateway.Verify(x => x.UpdateAllocation(request));
        }

        [Test]
        public void UpdateAllocationCallsUpdateRagRatingInAllocationWhenRagRatingIsSet()
        {
            UpdateAllocationRequest request = new UpdateAllocationRequest() { Id = 1, RagRating = "green" };

            _allocationsUseCase.ExecuteUpdate(request);

            _mockDatabaseGateway.Verify(x => x.UpdateRagRatingInAllocation(It.Is<UpdateAllocationRequest>(x => x == request)), Times.Once);
        }

        [Test]
        public void UpdateAllocationCallsDatabaseGatewayWithParameters()
        {
            UpdateAllocationRequest request = new UpdateAllocationRequest() { Id = 1, RagRating = null };

            _allocationsUseCase.ExecuteUpdate(request);

            _mockDatabaseGateway.Verify(x => x.UpdateAllocation(It.Is<UpdateAllocationRequest>(x => x == request)), Times.Once);
        }

        [Test]
        public void UpdateAllocationReturnsCorrectCaseNoteId()
        {
            UpdateAllocationRequest request = new UpdateAllocationRequest() { Id = 1, RagRating = null };

            UpdateAllocationResponse expectedResponse = new UpdateAllocationResponse() { CaseNoteId = _fixture.Create<string>() };

            _mockDatabaseGateway.Setup(x => x.UpdateAllocation(It.Is<UpdateAllocationRequest>(x => x == request))).Returns(expectedResponse);

            var response = _allocationsUseCase.ExecuteUpdate(request);

            Assert.AreEqual(expectedResponse.CaseNoteId, response.CaseNoteId);
        }

        [Test]
        public void ExecuteReturnsTheResponse()
        {
            var responseObject = new CreateAllocationResponse() { CaseNoteId = _fixture.Create<string>() };
            _mockDatabaseGateway.Setup(x => x.CreateAllocation(It.IsAny<CreateAllocationRequest>()))
                .Returns(responseObject);

            var response = _allocationsUseCase.ExecutePost(new CreateAllocationRequest());

            response.Should().BeEquivalentTo(responseObject);
        }

        [Test]
        public void ListAllocationsByWorkerEmail()
        {
            var request = new ListAllocationsRequest() { WorkerEmail = "test@example.com" };
            var gatewayResponse = new List<Allocation> { new Allocation() { AllocatedWorker = "Test Worker" } };

            _mockDatabaseGateway.Setup(x => x.SelectAllocations(0, 0, "test@example.com"))
                .Returns(gatewayResponse);

            var response = _allocationsUseCase.Execute(request);

            response.Allocations.Should().BeEquivalentTo(gatewayResponse);
        }
    }
}
