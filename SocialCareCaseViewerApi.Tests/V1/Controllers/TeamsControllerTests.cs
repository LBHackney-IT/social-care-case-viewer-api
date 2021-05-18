using System;
using System.Collections.Generic;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using SocialCareCaseViewerApi.Tests.V1.Helpers;
using SocialCareCaseViewerApi.V1.Boundary.Response;
using SocialCareCaseViewerApi.V1.Controllers;
using SocialCareCaseViewerApi.V1.Exceptions;
using SocialCareCaseViewerApi.V1.Factories;
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
        public void GetTeamByTeamIdReturns200AndTeamWhenSuccessful()
        {
            var team = TestHelpers.CreateTeam();
            _teamsUseCase.Setup(x => x.ExecuteGetById(team.Id)).Returns(team.ToDomain().ToResponse());

            var response = _teamController.GetTeamById(team.Id) as ObjectResult;

            if (response == null)
            {
                throw new NullReferenceException();
            }
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(200);
            response.Value.Should().BeEquivalentTo(team.ToDomain().ToResponse());
        }

        [Test]
        public void GetTeamByTeamIdReturns404WhenTeamNotFound()
        {
            var response = _teamController.GetTeamById(1) as NotFoundResult;

            if (response == null)
            {
                throw new NullReferenceException();
            }
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(404);
        }

        [Test]
        public void GetTeamByTeamNameReturns200AndTeamWhenSuccessful()
        {
            var team = TestHelpers.CreateTeam();
            _teamsUseCase.Setup(x => x.ExecuteGetByName(team.Name)).Returns(team.ToDomain().ToResponse());

            var response = _teamController.GetTeamByName(team.Name) as ObjectResult;

            if (response == null)
            {
                throw new NullReferenceException();
            }
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(200);
            response.Value.Should().BeEquivalentTo(team.ToDomain().ToResponse());
        }

        [Test]
        public void GetTeamByTeamNameReturns404WhenTeamNotFound()
        {
            var response = _teamController.GetTeamByName("fake team name") as NotFoundResult;

            if (response == null)
            {
                throw new NullReferenceException();
            }
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(404);
        }

        [Test]
        public void GetTeamsReturns200AndTeamsWhenSuccessful()
        {
            var request = TestHelpers.CreateGetTeamsRequest();
            var teamsList = new ListTeamsResponse()
            {
                Teams = new List<TeamResponse> { TestHelpers.CreateTeam().ToDomain().ToResponse() }
            };
            _teamsUseCase.Setup(x => x.ExecuteGet(request)).Returns(teamsList);
            var response = _teamController.GetTeams(request) as ObjectResult;

            if (response == null)
            {
                throw new NullReferenceException();
            }
            response.Should().NotBeNull();
            response.StatusCode.Should().Be(200);
            response.Value.Should().BeEquivalentTo(teamsList);
        }

        [Test]
        public void GetTeamsReturns200AndEmptyListWhenNoTeamsFound()
        {
            var request = TestHelpers.CreateGetTeamsRequest();
            var teamsList = new ListTeamsResponse()
            {
                Teams = new List<TeamResponse>()
            };
            _teamsUseCase.Setup(x => x.ExecuteGet(request)).Returns(teamsList);
            var response = _teamController.GetTeams(request) as OkObjectResult;

            if (response == null)
            {
                throw new NullReferenceException();
            }
            response.StatusCode.Should().Be(200);
            ((ListTeamsResponse) response.Value).Teams.Should().BeEmpty();
        }

        [Test]
        public void GetTeamsReturns400WhenValidationResultsIsNotValid()
        {

            var getTeamsRequest = TestHelpers.CreateGetTeamsRequest(contextFlag: "invalid");

            var response = _teamController.GetTeams(getTeamsRequest) as BadRequestObjectResult;

            if (response == null)
            {
                throw new NullReferenceException();
            }

            response.Should().NotBeNull();
            response.StatusCode.Should().Be(400);
            response.Value.Should().Be("Context flag must be 1 character in length\nContext flag must be 'A' or 'C'");
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
