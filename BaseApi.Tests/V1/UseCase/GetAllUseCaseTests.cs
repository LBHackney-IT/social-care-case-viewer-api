using System.Linq;
using AutoFixture;
using SocialCareCaseViewerApi.V1.Boundary.Response;
using SocialCareCaseViewerApi.V1.Domain;
using SocialCareCaseViewerApi.V1.Factories;
using SocialCareCaseViewerApi.V1.Gateways;
using SocialCareCaseViewerApi.V1.UseCase;
using FluentAssertions;
using Moq;
using NUnit.Framework;

namespace SocialCareCaseViewerApi.Tests.V1.UseCase
{
    public class GetAllUseCaseTests
    {
        private Mock<IExampleGateway> _mockGateway;
        private GetAllUseCase _classUnderTest;
        private Fixture _fixture;

        [SetUp]
        public void SetUp()
        {
            _mockGateway = new Mock<IExampleGateway>();
            _classUnderTest = new GetAllUseCase(_mockGateway.Object);
            _fixture = new Fixture();
        }

        [Test]
        public void GetsAllFromTheGateway()
        {
            var stubbedEntities = _fixture.CreateMany<Entity>().ToList();
            _mockGateway.Setup(x => x.GetAll()).Returns(stubbedEntities);

            var expectedResponse = new ResponseObjectList { ResponseObjects = stubbedEntities.ToResponse() };

            _classUnderTest.Execute().Should().BeEquivalentTo(expectedResponse);
        }

        //TODO: Add extra tests here for extra functionality added to the use case
    }
}
