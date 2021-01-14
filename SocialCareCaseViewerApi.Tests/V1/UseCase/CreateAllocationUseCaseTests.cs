using AutoFixture;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Gateways;
using SocialCareCaseViewerApi.V1.UseCase;

namespace SocialCareCaseViewerApi.Tests.V1.UseCase
{
    public class CreateAllocationUseCaseTests
    {
        private Mock<IDatabaseGateway> _mockDatabaseGateway;
        private CreateAllocationUseCase _classUnderTest;

        private Fixture _fixture = new Fixture();

        [SetUp]
        public void SetUp()
        {
            _mockDatabaseGateway = new Mock<IDatabaseGateway>();
            _classUnderTest = new CreateAllocationUseCase(_mockDatabaseGateway.Object);
        }

        [Test]
        public void ExecuteReturnsTheRequest()
        {
            var stubbedRequest = _fixture.Create<CreateAllocationRequest>();

            _mockDatabaseGateway.Setup(x => x.CreateAllocation(It.IsAny<CreateAllocationRequest>()))
                .Returns(stubbedRequest);

            var response = _classUnderTest.Execute(new CreateAllocationRequest());

            response.Should().NotBeNull();
            response.Should().BeEquivalentTo(stubbedRequest);
        }
    }
}
