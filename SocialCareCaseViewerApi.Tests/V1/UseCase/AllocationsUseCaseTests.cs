using System;
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
            UpdateAllocationRequest request = new UpdateAllocationRequest() { Id = 1, RagRating = "low" };

            _allocationsUseCase.ExecuteUpdate(request);

            _mockDatabaseGateway.Verify(x => x.UpdateRagRatingInAllocation(It.Is<UpdateAllocationRequest>(x => x == request)), Times.Once);
        }

        [Test]
        public void UpdateAllocationCallsUpdateRagRatingInAllocationWhenRagRatingIsSetToNull()
        {
            UpdateAllocationRequest request = new UpdateAllocationRequest() { Id = 1, RagRating = null };

            _allocationsUseCase.ExecuteUpdate(request);

            _mockDatabaseGateway.Verify(x => x.UpdateRagRatingInAllocation(It.Is<UpdateAllocationRequest>(x => x == request)), Times.Once);
        }

        [Test]
        public void UpdateAllocationCallsDatabaseGatewayWithParameters()
        {
            UpdateAllocationRequest request = new UpdateAllocationRequest() { Id = 1, RagRating = null, DeallocationDate = DateTime.Now, DeallocationReason = "reason" };

            _allocationsUseCase.ExecuteUpdate(request);

            _mockDatabaseGateway.Verify(x => x.UpdateAllocation(It.Is<UpdateAllocationRequest>(x => x == request)), Times.Once);
        }

        [Test]
        public void UpdateAllocationReturnsCorrectCaseNoteId()
        {
            UpdateAllocationRequest request = new UpdateAllocationRequest() { Id = 1, RagRating = null, DeallocationDate = DateTime.Now, DeallocationReason = "reason" };

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

            _mockDatabaseGateway.Setup(x => x.SelectAllocations(0, 0, "test@example.com", 0, null, 0, null, null))
                .Returns((gatewayResponse, 0));

            var response = _allocationsUseCase.Execute(request);

            response.Allocations.Should().BeEquivalentTo(gatewayResponse);
        }

        [Test]
        public void ListAllocationsByTeamIdReturnsExpectedList()
        {
            var request = new ListAllocationsRequest() { TeamId = 10 };
            var gatewayResponse = new List<Allocation> { new Allocation() { Id = 1, PersonId = 2, AllocatedWorker = "Test Worker" } };

            _mockDatabaseGateway.Setup(x => x.SelectAllocations(0, 0, null, 10, null, 0, null, null))
                .Returns((gatewayResponse, 0));

            var response = _allocationsUseCase.Execute(request);

            response.Allocations.Should().BeEquivalentTo(gatewayResponse);
        }

        [Test]
        public void ListAllocationsByTeamIdReturnsNothingIfNoMatches()
        {
            var request = new ListAllocationsRequest() { TeamId = 8 };
            var response = _allocationsUseCase.Execute(request);
            response.Allocations.Should().BeNullOrEmpty();
        }

        [Test]
        public void ListAllocationsByMosaicIdReturnsExpectedList()
        {
            var request = new ListAllocationsRequest() { MosaicId = 3 };
            var gatewayResponse = new List<Allocation> { new Allocation() { Id = 1, PersonId = 2, AllocatedWorker = "Test Worker" } };

            _mockDatabaseGateway.Setup(x => x.SelectAllocations(3, 0, null, 0, null, 0, null, null))
                .Returns((gatewayResponse, 0));

            var response = _allocationsUseCase.Execute(request);

            response.Allocations.Should().BeEquivalentTo(gatewayResponse);
        }

        [Test]
        public void ListAllocationsByMosaicIdReturnsNothingIfNoMatches()
        {
            var request = new ListAllocationsRequest() { MosaicId = 3 };
            var response = _allocationsUseCase.Execute(request);
            response.Allocations.Should().BeNullOrEmpty();
        }
        [Test]
        public void ListAllocationsByWorkerIdReturnsExpectedList()
        {
            var request = new ListAllocationsRequest() { WorkerId = 5 };
            var gatewayResponse = new List<Allocation> { new Allocation() { Id = 1, PersonId = 2, AllocatedWorker = "Test Worker" } };

            _mockDatabaseGateway.Setup(x => x.SelectAllocations(0, 5, null, 0, null, 0, null, null))
                .Returns((gatewayResponse, 0));

            var response = _allocationsUseCase.Execute(request);

            response.Allocations.Should().BeEquivalentTo(gatewayResponse);
        }

        [Test]
        public void ListAllocationsByWorkerIdReturnsNothingIfNoMatches()
        {
            var request = new ListAllocationsRequest() { MosaicId = 5 };
            var response = _allocationsUseCase.Execute(request);
            response.Allocations.Should().BeNullOrEmpty();
        }

        [Test]
        public void ExecuteCallsSelectAllocationMethodOnGatewayWithSortPropertyWhenProvided()
        {
            var request = new ListAllocationsRequest() { MosaicId = 1, SortBy = "rag_rating" };

            _mockDatabaseGateway.Setup(x => x.SelectAllocations(It.IsAny<long>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>())).Returns((new List<Allocation>(), 0));

            _allocationsUseCase.Execute(request);

            _mockDatabaseGateway.Verify(x => x.SelectAllocations(request.MosaicId, 0, null, request.TeamId, request.SortBy, 0, request.TeamAllocationStatus, request.Status), Times.Once);
        }

        [Test]
        public void ExecuteCallsSelectAllocationMethodOnGatewayWithCursorWhenProvided()
        {
            var request = new ListAllocationsRequest() { MosaicId = 1, Cursor = 20 };

            _mockDatabaseGateway.Setup(x => x.SelectAllocations(It.IsAny<long>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>())).Returns((new List<Allocation>(), 0));

            _allocationsUseCase.Execute(request);

            _mockDatabaseGateway.Verify(x => x.SelectAllocations(request.MosaicId, 0, null, request.TeamId, null, request.Cursor, request.TeamAllocationStatus, request.Status), Times.Once);
        }

        [Test]
        public void ListAllocationsByTeamIdWithStatusReturnsExpectedList()
        {
            var request = new ListAllocationsRequest() { TeamId = 10, Status = "closed" };
            var gatewayResponse = new List<Allocation> { new Allocation() { Id = 1, PersonId = 2, AllocatedWorker = "Test Worker", CaseStatus = "closed", AllocatedWorkerTeamId = 10 } };

            _mockDatabaseGateway.Setup(x => x.SelectAllocations(0, 0, null, 10, null, 0, null, "closed"))
                .Returns((gatewayResponse, 0));

            var response = _allocationsUseCase.Execute(request);

            response.Allocations.Should().BeEquivalentTo(gatewayResponse);
        }

    }
}
