using AutoFixture;
using Bogus;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Boundary.Response;
using SocialCareCaseViewerApi.V1.Controllers;
using SocialCareCaseViewerApi.V1.Exceptions;
using SocialCareCaseViewerApi.V1.UseCase.Interfaces;

namespace SocialCareCaseViewerApi.Tests.V1.Controllers
{
    [TestFixture]
    public class RelationshipControllerTests
    {
        private RelationshipController _classUnderTest;
        private Mock<IRelationshipsV1UseCase> _mockRelationshipsUseCase;
        private readonly Fixture _fixture = new Fixture();
        private readonly Faker _faker = new Faker();

        [SetUp]
        public void SetUp()
        {
            _mockRelationshipsUseCase = new Mock<IRelationshipsV1UseCase>();

            _classUnderTest = new RelationshipController(_mockRelationshipsUseCase.Object);
        }

        [Test]
        public void ListRelationshipsV1Returns200WhenPersonIsFound()
        {
            var request = new ListRelationshipsV1Request() { PersonId = _faker.Random.Long() };

            _mockRelationshipsUseCase.Setup(x => x.ExecuteGet(It.IsAny<ListRelationshipsV1Request>())).Returns(new ListRelationshipsV1Response());

            var response = _classUnderTest.ListRelationshipsV1(request) as ObjectResult;

            response?.StatusCode.Should().Be(200);
        }

        [Test]
        public void ListRelationshipsV1Returns404WithCorrectErrorMessageWhenPersonIsNotFound()
        {
            var request = new ListRelationshipsV1Request() { PersonId = _faker.Random.Long() };

            _mockRelationshipsUseCase.Setup(x => x.ExecuteGet(It.IsAny<ListRelationshipsV1Request>())).Throws(new GetRelationshipsException("Person not found"));

            var response = _classUnderTest.ListRelationshipsV1(request) as NotFoundObjectResult;

            response?.StatusCode.Should().Be(404);
            response?.Value.Should().Be("Person not found");
        }

        [Test]
        public void ListRelationshipsV1Returns200AndRelationshipsWhenSuccessful()
        {
            var request = new ListRelationshipsV1Request() { PersonId = _faker.Random.Long() };

            var listRelationShipsResponse = _fixture.Create<ListRelationshipsV1Response>();

            _mockRelationshipsUseCase.Setup(x => x.ExecuteGet(It.IsAny<ListRelationshipsV1Request>())).Returns(listRelationShipsResponse);

            var response = _classUnderTest.ListRelationshipsV1(request) as ObjectResult;

            response?.Value.Should().BeOfType<ListRelationshipsV1Response>();
            response?.Value.Should().BeEquivalentTo(listRelationShipsResponse);
        }

        [Test]
        public void ListRelationshipsReturns200WhenPersonIsFound()
        {
            var request = new ListRelationshipsV1Request() { PersonId = _faker.Random.Long() };

            _mockRelationshipsUseCase.Setup(x => x.ExecuteGet(It.IsAny<ListRelationshipsV1Request>())).Returns(new ListRelationshipsV1Response());

            var response = _classUnderTest.ListRelationships(request) as ObjectResult;

            response?.StatusCode.Should().Be(200);
        }

        [Test]
        public void ListRelationshipsReturns404WithCorrectErrorMessageWhenPersonIsNotFound()
        {
            var request = new ListRelationshipsV1Request() { PersonId = _faker.Random.Long() };

            _mockRelationshipsUseCase.Setup(x => x.ExecuteGet(It.IsAny<ListRelationshipsV1Request>())).Throws(new GetRelationshipsException("Person not found"));

            var response = _classUnderTest.ListRelationships(request) as NotFoundObjectResult;

            response?.StatusCode.Should().Be(404);
            response?.Value.Should().Be("Person not found");
        }

        [Test]
        public void ListRelationshipsReturns200AndRelationshipsWhenSuccessful()
        {
            var request = new ListRelationshipsV1Request() { PersonId = _faker.Random.Long() };

            var listRelationShipsResponse = _fixture.Create<ListRelationshipsV1Response>();

            _mockRelationshipsUseCase.Setup(x => x.ExecuteGet(It.IsAny<ListRelationshipsV1Request>())).Returns(listRelationShipsResponse);

            var response = _classUnderTest.ListRelationships(request) as ObjectResult;

            response?.Value.Should().BeOfType<ListRelationshipsV1Response>();
            response?.Value.Should().BeEquivalentTo(listRelationShipsResponse);
        }
    }
}
