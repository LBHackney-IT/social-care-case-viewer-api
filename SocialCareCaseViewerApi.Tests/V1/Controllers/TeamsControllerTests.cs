using System;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SocialCareCaseViewerApi.Tests.V1.Helpers;
using SocialCareCaseViewerApi.V1.Controllers;
using SocialCareCaseViewerApi.V1.Exceptions;
using SocialCareCaseViewerApi.V1.UseCase.Interfaces;

namespace SocialCareCaseViewerApi.Tests.V1.Controllers
{
    [TestFixture]
    public class TeamsControllerTests
    {
        private TeamController _teamController;
        private Mock<ITeamsUseCase> _teamsUseCase;

        [SetUp]
        public void Setup()
        {
            _teamsUseCase = new Mock<ITeamsUseCase>();
            _teamController = new TeamController(_teamsUseCase.Object);
        }

        [Test]
        public void CreateTeamReturns201StatusAndTeamWhenSuccessful()
        {
            var createTeamRequest = TestHelpers.CreateTeamRequest();
            var team = TestHelpers.CreateTeamResponse(name: createTeamRequest.Name, context: createTeamRequest.Context);
            _teamsUseCase.Setup(x => x.ExecutePost(createTeamRequest)).Returns(team);

            var response = _teamController.CreateTeam(createTeamRequest) as ObjectResult;

            _teamsUseCase.Verify(x => x.ExecutePost(createTeamRequest), Times.Once);
            if (response == null)
            {
                throw new NullReferenceException();
            }
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(201);
            response.Value.Should().BeEquivalentTo(team);
        }

        [Test]
        public void CreateTeamReturns400WhenValidationResultsIsNotValid()
        {

            var createTeamRequest = TestHelpers.CreateTeamRequest(name: "");

            var response = _teamController.CreateTeam(createTeamRequest) as BadRequestObjectResult;

            if (response == null)
            {
                throw new NullReferenceException();
            }

            response.Should().NotBeNull();
            response.StatusCode.Should().Be(400);
            response.Value.Should().Be("Team name must be provided");
        }

        [Test]
        public void CreateTeamReturns422StatusWhenPostTeamExceptionThrown()
        {
            const string errorMessage = "Failed to create team";
            var createTeamRequest = TestHelpers.CreateTeamRequest();
            _teamsUseCase.Setup(x => x.ExecutePost(createTeamRequest))
                .Throws(new PostTeamException(errorMessage));

            var response = _teamController.CreateTeam(createTeamRequest) as ObjectResult;

            if (response == null)
            {
                throw new NullReferenceException();
            }
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(422);
            response.Value.Should().Be(errorMessage);
        }
    }
}
