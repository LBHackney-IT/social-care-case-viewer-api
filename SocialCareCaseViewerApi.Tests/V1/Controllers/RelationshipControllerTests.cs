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
        private Mock<IRelationshipsUseCase> _mockRelationshipsUseCase;
        private readonly Fixture _fixture = new Fixture();

        [SetUp]
        public void SetUp()
        {
            _mockRelationshipsUseCase = new Mock<IRelationshipsUseCase>();

            _classUnderTest = new RelationshipController(_mockRelationshipsUseCase.Object);
        }

        [Test]
        public void ListRelationshipsReturns200WhenPersonIsFound()
        {
            _mockRelationshipsUseCase.Setup(x => x.ExecuteGet(It.IsAny<long>())).Returns(new ListRelationshipsResponse());

            var response = _classUnderTest.ListRelationships(123456789) as ObjectResult;

            response?.StatusCode.Should().Be(200);
        }

        [Test]
        public void ListRelationshipsReturns404WithCorrectErrorMessageWhenPersonIsNotFound()
        {
            _mockRelationshipsUseCase.Setup(x => x.ExecuteGet(It.IsAny<long>())).Throws(new GetRelationshipsException("Person not found"));

            var response = _classUnderTest.ListRelationships(123456789) as NotFoundObjectResult;

            response?.StatusCode.Should().Be(404);
            response?.Value.Should().Be("Person not found");
        }

        [Test]
        public void ListRelationshipsReturns200AndRelationshipsWhenSuccessful()
        {
            var listRelationShipsResponse = _fixture.Create<ListRelationshipsResponse>();
            _mockRelationshipsUseCase.Setup(x => x.ExecuteGet(It.IsAny<long>())).Returns(listRelationShipsResponse);

            var response = _classUnderTest.ListRelationships(123456789) as ObjectResult;

            response?.Value.Should().BeOfType<ListRelationshipsResponse>();
            response?.Value.Should().BeEquivalentTo(listRelationShipsResponse);
        }
    }
}
