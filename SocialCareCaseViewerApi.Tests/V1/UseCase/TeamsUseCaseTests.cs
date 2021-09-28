using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SocialCareCaseViewerApi.Tests.V1.Helpers;
using SocialCareCaseViewerApi.V1.Exceptions;
using SocialCareCaseViewerApi.V1.Factories;
using SocialCareCaseViewerApi.V1.Gateways;
using SocialCareCaseViewerApi.V1.Gateways.Interfaces;
using SocialCareCaseViewerApi.V1.UseCase;
using DbTeam = SocialCareCaseViewerApi.V1.Infrastructure.Team;

namespace SocialCareCaseViewerApi.Tests.V1.UseCase
{
    [TestFixture]
    public class TeamsUseCaseTests
    {
        private readonly Mock<IDatabaseGateway> _mockDatabaseGateway = new Mock<IDatabaseGateway>();
        private readonly Mock<ITeamGateway> _mockTeamGateway = new Mock<ITeamGateway>();
        private TeamsUseCase _teamsUseCase;

        [SetUp]
        public void SetUp()
        {
            _teamsUseCase = new TeamsUseCase(_mockDatabaseGateway.Object, _mockTeamGateway.Object);
        }

        [Test]
        public void GetTeamsByTeamIdReturnsTeamResponseWhenTeamExists()
        {
            var team = TestHelpers.CreateTeam();
            _mockDatabaseGateway.Setup(x => x.GetTeamByTeamId(team.Id)).Returns(team);

            var response = _teamsUseCase.ExecuteGetById(team.Id);

            response.Should().BeEquivalentTo(team.ToDomain().ToResponse());
        }

        [Test]
        public void GetTeamsByTeamIdReturnsNullWhenTeamDoesNotExists()
        {
            var response = _teamsUseCase.ExecuteGetById(1);

            response.Should().BeNull();
        }

        [Test]
        public void GetTeamsByTeamNameReturnsTeamResponseWhenTeamExists()
        {
            var team = TestHelpers.CreateTeam();
            _mockDatabaseGateway.Setup(x => x.GetTeamByTeamName(team.Name)).Returns(team);

            var response = _teamsUseCase.ExecuteGetByName(team.Name);

            response.Should().BeEquivalentTo(team.ToDomain().ToResponse());
        }

        [Test]
        public void GetTeamsByTeamNameReturnsNullWhenTeamDoesNotExists()
        {
            var response = _teamsUseCase.ExecuteGetByName("fake name");

            response.Should().BeNull();
        }

        [Test]
        public void GetTeamsByContextReturnsListTeamsResponse()
        {
            var request = TestHelpers.CreateGetTeamsRequest();
            var team = TestHelpers.CreateTeam();

            _mockDatabaseGateway.Setup(x => x.GetTeamsByTeamContextFlag(request.ContextFlag)).Returns(new List<DbTeam> { team });

            var result = _teamsUseCase.ExecuteGet(request);

            _mockDatabaseGateway.Verify(x => x.GetTeamsByTeamContextFlag(request.ContextFlag), Times.Once);

            var firstTeamResponse = result.Teams.FirstOrDefault();

            firstTeamResponse?.Id.Should().Be(team.Id);
            firstTeamResponse?.Context.Should().BeEquivalentTo(team.Context);
            firstTeamResponse?.Name.Should().BeEquivalentTo(team.Name);
        }


        [Test]
        public void ExecutePostCallsDatabaseGateway()
        {
            var createTeamRequest = TestHelpers.CreateTeamRequest();
            var team = TestHelpers.CreateTeam(name: createTeamRequest.Name, context: createTeamRequest.Context);
            _mockTeamGateway.Setup(x => x.CreateTeam(createTeamRequest)).Returns(team.ToDomain());

            _teamsUseCase.ExecutePost(createTeamRequest);

            _mockTeamGateway.Verify(x => x.CreateTeam(createTeamRequest), Times.Once);
        }

        [Test]
        public void ExecutePostReturnsCreatedTeam()
        {
            var createTeamRequest = TestHelpers.CreateTeamRequest();
            var team = TestHelpers.CreateTeam(name: createTeamRequest.Name, context: createTeamRequest.Context).ToDomain();
            _mockTeamGateway.Setup(x => x.CreateTeam(createTeamRequest)).Returns(team);

            var response = _teamsUseCase.ExecutePost(createTeamRequest);

            response.Should().BeEquivalentTo(team.ToResponse());
        }

        [Test]
        public void ExecutePostThrowsPostTeamExceptionIfTeamNameAlreadyInUse()
        {
            var createTeamRequest = TestHelpers.CreateTeamRequest();
            var team = TestHelpers.CreateTeam();
            _mockDatabaseGateway.Setup(x => x.GetTeamByTeamName(createTeamRequest.Name)).Returns(team);

            Action act = () => _teamsUseCase.ExecutePost(createTeamRequest);

            act.Should().Throw<PostTeamException>()
                .WithMessage($"Team with name \"{createTeamRequest.Name}\" already exists");
        }
    }
}
