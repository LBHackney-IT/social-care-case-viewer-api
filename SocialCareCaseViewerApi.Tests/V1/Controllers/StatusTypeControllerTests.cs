using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SocialCareCaseViewerApi.V1.Controllers;
using SocialCareCaseViewerApi.V1.Exceptions;
using SocialCareCaseViewerApi.V1.UseCase.Interfaces;
using SocialCareCaseViewerApi.V1.Boundary.Response;
using AutoFixture;

namespace SocialCareCaseViewerApi.Tests.V1.Controllers
{
    [TestFixture]
    public class StatusTypeControllerTests
    {
        private StatusTypeController _statusTypeController;
        private Mock<ICaseStatusesUseCase> _mockCaseStatusesUseCase;
        private readonly Fixture _fixture = new Fixture();

        [SetUp]
        public void SetUp()
        {
            _mockCaseStatusesUseCase = new Mock<ICaseStatusesUseCase>();

            _statusTypeController = new StatusTypeController(_mockCaseStatusesUseCase.Object);

        }

        [Test]
        public void ListCaseStatusesReturns200WhenPersonIsFound()
        {
            _mockCaseStatusesUseCase.Setup(x => x.ExecuteGet(It.IsAny<long>())).Returns(new ListCaseStatusesResponse());

            var response = _statusTypeController.ListCaseStatuses(123456789) as ObjectResult;

            response?.StatusCode.Should().Be(200);
        }

        [Test]
        public void ListCaseStatuses404WithCorrectErrorMessageWhenPersonIsNotFound()
        {
            _mockCaseStatusesUseCase.Setup(x => x.ExecuteGet(It.IsAny<long>())).Throws(new GetCaseStatusesException("Person not found"));

            var response = _statusTypeController.ListCaseStatuses(123456789) as NotFoundObjectResult;

            response?.StatusCode.Should().Be(404);
            response?.Value.Should().Be("Person not found");
        }

        [Test]
        public void ListCaseStatuses200AndCaseStatusesWhenSuccessful()
        {
            var listRelationShipsResponse = _fixture.Create<ListCaseStatusesResponse>();
            _mockCaseStatusesUseCase.Setup(x => x.ExecuteGet(It.IsAny<long>())).Returns(listRelationShipsResponse);

            var response = _statusTypeController.ListCaseStatuses(123456789) as ObjectResult;

            response?.Value.Should().BeOfType<ListCaseStatusesResponse>();
            response?.Value.Should().BeEquivalentTo(listRelationShipsResponse);
        }
    }
}
