using AutoFixture;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Boundary.Response;
using SocialCareCaseViewerApi.V1.Controllers;
using SocialCareCaseViewerApi.V1.Exceptions;
using SocialCareCaseViewerApi.V1.UseCase.Interfaces;
using SocialCareCaseViewerApi.Tests.V1.Helpers;

namespace SocialCareCaseViewerApi.Tests.V1.Controllers.Relationship
{
    [TestFixture]
    public class RelationshipControllerCreatePersonalRelationshipTests
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
        public void CallsPersonalRelationshipsUseCaseToCreatePersonalRelationship()
        {
            _mockPersonalRelationshipsUseCase.Setup(x => x.ExecutePost(It.IsAny<CreatePersonalRelationshipRequest>()));
            var request = PersonalRelationshipsHelper.CreatePersonalRelationshipRequest();

            var response = _relationshipController.CreatePersonalRelationship(request) as BadRequestObjectResult;

            _mockPersonalRelationshipsUseCase.Verify(x => x.ExecutePost(request));
        }

        [Test]
        public void WhenPersonNotFoundExceptionIsThrownReturns400WithMessage()
        {
            var exceptionMessage = "error message";
            _mockPersonalRelationshipsUseCase.Setup(x => x.ExecutePost(It.IsAny<CreatePersonalRelationshipRequest>()))
                .Throws(new PersonNotFoundException(exceptionMessage));
            var request = PersonalRelationshipsHelper.CreatePersonalRelationshipRequest();

            var response = _relationshipController.CreatePersonalRelationship(request) as BadRequestObjectResult;

            response?.StatusCode.Should().Be(400);
            response?.Value.Should().Be(exceptionMessage);
        }

        [Test]
        public void WhenPersonalRelationshipTypeNotFoundExceptionIsThrownReturns400WithMessage()
        {
            var exceptionMessage = "error message";
            _mockPersonalRelationshipsUseCase.Setup(x => x.ExecutePost(It.IsAny<CreatePersonalRelationshipRequest>()))
                .Throws(new PersonalRelationshipTypeNotFoundException(exceptionMessage));
            var request = PersonalRelationshipsHelper.CreatePersonalRelationshipRequest();

            var response = _relationshipController.CreatePersonalRelationship(request) as BadRequestObjectResult;

            response?.StatusCode.Should().Be(400);
            response?.Value.Should().Be(exceptionMessage);
        }

        [Test]
        public void WhenPersonalRelationshipAlreadyExistsExceptionIsThrownReturns400WithMessage()
        {
            var exceptionMessage = "error message";
            _mockPersonalRelationshipsUseCase.Setup(x => x.ExecutePost(It.IsAny<CreatePersonalRelationshipRequest>()))
                .Throws(new PersonalRelationshipAlreadyExistsException(exceptionMessage));
            var request = PersonalRelationshipsHelper.CreatePersonalRelationshipRequest();

            var response = _relationshipController.CreatePersonalRelationship(request) as BadRequestObjectResult;

            response?.StatusCode.Should().Be(400);
            response?.Value.Should().Be(exceptionMessage);
        }

        [Test]
        public void WhenRequestIsValidReturnsSuccessfulResponse()
        {
            var request = PersonalRelationshipsHelper.CreatePersonalRelationshipRequest();

            var response = _relationshipController.CreatePersonalRelationship(request);

            response.Should().BeOfType<CreatedAtActionResult>();
            var createdAtAction = response as CreatedAtActionResult;
            createdAtAction.StatusCode.Should().Be(201);
            createdAtAction.Value.Should().Be("Successfully created personal relationship.");
        }

        [Test]
        public void WhenRequestIsInValidReturnsBadRequest400WithMessage()
        {
            var badRequest = PersonalRelationshipsHelper.CreatePersonalRelationshipRequest(isMainCarer: "invalid");

            var response = _relationshipController.CreatePersonalRelationship(badRequest);

            response.Should().BeOfType<BadRequestObjectResult>();
            var badRequestObject = response as BadRequestObjectResult;
            badRequestObject.StatusCode.Should().Be(400);
            badRequestObject.Value.Should().Be("'isMainCarer' must be 'Y' or 'N'.");
        }
    }
}
