using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Controllers;
using SocialCareCaseViewerApi.V1.Exceptions;
using SocialCareCaseViewerApi.V1.UseCase.Interfaces;
using SocialCareCaseViewerApi.Tests.V1.Helpers;

namespace SocialCareCaseViewerApi.Tests.V1.Controllers.Relationship
{
    [TestFixture]
    public class RelationshipControllerDeletePersonalRelationshipTests
    {
        private RelationshipController _relationshipController;
        private Mock<IPersonalRelationshipsUseCase> _mockPersonalRelationshipsUseCase;
        private readonly Mock<IRelationshipsUseCase> _mockRelationshipsUseCase = new Mock<IRelationshipsUseCase>();

        [SetUp]
        public void SetUp()
        {
            _mockPersonalRelationshipsUseCase = new Mock<IPersonalRelationshipsUseCase>();

            _relationshipController = new RelationshipController(_mockRelationshipsUseCase.Object, _mockPersonalRelationshipsUseCase.Object);
        }

        [Test]
        public void CallsPersonalRelationshipsUseCaseToDeletePersonalRelationship()
        {
            _mockPersonalRelationshipsUseCase.Setup(x => x.ExecuteDelete(It.IsAny<long>()));

            var response = _relationshipController.RemovePersonalRelationship(12345) as BadRequestObjectResult;

            _mockPersonalRelationshipsUseCase.Verify(x => x.ExecuteDelete(12345));
        }
        [Test]
        public void WhenRelationshipNotFoundExceptionIsThrownReturns400WithMessage()
        {
            var exceptionMessage = "error message";
            _mockPersonalRelationshipsUseCase.Setup(x => x.ExecuteDelete(It.IsAny<long>()))
                .Throws(new PersonalRelationshipNotFoundException(exceptionMessage));

            var response = _relationshipController.RemovePersonalRelationship(12345) as BadRequestObjectResult;

            response?.StatusCode.Should().Be(400);
            response?.Value.Should().Be(exceptionMessage);
        }
        [Test]
        public void WhenRequestIsValidReturnsSuccessfulResponse()
        {
            var response = _relationshipController.RemovePersonalRelationship(1) as ObjectResult;

            response?.StatusCode.Should().Be(200);
        }
    }
}
