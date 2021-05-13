using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SocialCareCaseViewerApi.Tests.V1.Helpers;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Exceptions;
using SocialCareCaseViewerApi.V1.Factories;
using SocialCareCaseViewerApi.V1.Gateways;
using SocialCareCaseViewerApi.V1.UseCase;
using DbTeam = SocialCareCaseViewerApi.V1.Infrastructure.Team;

namespace SocialCareCaseViewerApi.Tests.V1.UseCase
{
    [TestFixture]
    public class TeamsUseCaseTests
    {
        private Mock<IDatabaseGateway> _mockDatabaseGateway;
        private TeamsUseCase _teamsUseCase;

        [SetUp]
        public void SetUp()
        {
            _mockDatabaseGateway = new Mock<IDatabaseGateway>();
            _teamsUseCase = new TeamsUseCase(_mockDatabaseGateway.Object);
        }

        [Test]
        public void GetTeamsByContextReturnsListTeamsResponse()
        {
            var request = TestHelpers.CreateGetTeamsRequest();
            var team = TestHelpers.CreateTeam();
            request.Id = null;
            request.Name = null;

            _mockDatabaseGateway.Setup(x => x.GetTeamsByTeamContextFlag(request.ContextFlag)).Returns(new List<DbTeam>{team});

            var result = _teamsUseCase.ExecuteGet(request);

            _mockDatabaseGateway.Verify(x => x.GetTeamsByTeamContextFlag(request.ContextFlag), Times.Once);
            _mockDatabaseGateway.Verify(x => x.GetTeamByTeamId(request.Id ?? 0), Times.Never);
            _mockDatabaseGateway.Verify(x => x.GetTeamByTeamName(request.Name), Times.Never);

            result.Teams.FirstOrDefault().Id.Should().Be(team.Id);
            result.Teams.FirstOrDefault().Context.Should().BeEquivalentTo(team.Context);
            result.Teams.FirstOrDefault().Name.Should().BeEquivalentTo(team.Name);
        }

        [Test]
        public void GetTeamsByIdReturnsListTeamsResponse()
        {
            var request = TestHelpers.CreateGetTeamsRequest();
            var team = TestHelpers.CreateTeam();
            request.ContextFlag = null;
            request.Name = null;

            _mockDatabaseGateway.Setup(x => x.GetTeamByTeamId(request.Id ?? 0)).Returns(team);

            var result = _teamsUseCase.ExecuteGet(request);

            _mockDatabaseGateway.Verify(x => x.GetTeamsByTeamContextFlag(request.ContextFlag), Times.Never);
            _mockDatabaseGateway.Verify(x => x.GetTeamByTeamId(request.Id ?? 0), Times.Once);
            _mockDatabaseGateway.Verify(x => x.GetTeamByTeamName(request.Name), Times.Never);

            result.Teams.FirstOrDefault().Id.Should().Be(team.Id);
            result.Teams.FirstOrDefault().Context.Should().BeEquivalentTo(team.Context);
            result.Teams.FirstOrDefault().Name.Should().BeEquivalentTo(team.Name);
        }

        [Test]
        public void GetTeamsByTeamNameReturnsListTeamsResponse()
        {
            var request = TestHelpers.CreateGetTeamsRequest();
            var team = TestHelpers.CreateTeam();
            request.Id = null;
            request.ContextFlag = null;

            _mockDatabaseGateway.Setup(x => x.GetTeamByTeamName(request.Name)).Returns(team);

            var result = _teamsUseCase.ExecuteGet(request);

            _mockDatabaseGateway.Verify(x => x.GetTeamsByTeamContextFlag(request.ContextFlag), Times.Never);
            _mockDatabaseGateway.Verify(x => x.GetTeamByTeamId(request.Id ?? 0), Times.Never);
            _mockDatabaseGateway.Verify(x => x.GetTeamByTeamName(request.Name), Times.Once);

            result.Teams.FirstOrDefault().Id.Should().Be(team.Id);
            result.Teams.FirstOrDefault().Context.Should().BeEquivalentTo(team.Context);
            result.Teams.FirstOrDefault().Name.Should().BeEquivalentTo(team.Name);
        }

        [Test]
        public void GetTeamsBadRequestReturnsEmptyList()
        {
            var request = TestHelpers.CreateGetTeamsRequest(setFieldsNull: true);

            var result = _teamsUseCase.ExecuteGet(request);

            _mockDatabaseGateway.Verify(x => x.GetTeamsByTeamContextFlag(request.ContextFlag), Times.Never);
            _mockDatabaseGateway.Verify(x => x.GetTeamByTeamId(request.Id ?? 0), Times.Never);
            _mockDatabaseGateway.Verify(x => x.GetTeamByTeamName(request.Name), Times.Never);

            result.Teams.Should().BeEmpty();
        }

        [Test]
        public void GetTeamsPrioritisesIdOverNameAndContext()
        {
            var request = TestHelpers.CreateGetTeamsRequest();

            _teamsUseCase.ExecuteGet(request);

            _mockDatabaseGateway.Verify(x => x.GetTeamsByTeamContextFlag(request.ContextFlag), Times.Never);
            _mockDatabaseGateway.Verify(x => x.GetTeamByTeamId(request.Id ?? 0), Times.Once);
            _mockDatabaseGateway.Verify(x => x.GetTeamByTeamName(request.Name), Times.Never);
        }

        [Test]
        public void GetTeamsPrioritisesNameOverContext()
        {
            var request = TestHelpers.CreateGetTeamsRequest();
            request.Id = null;

            _teamsUseCase.ExecuteGet(request);

            _mockDatabaseGateway.Verify(x => x.GetTeamsByTeamContextFlag(request.ContextFlag), Times.Never);
            _mockDatabaseGateway.Verify(x => x.GetTeamByTeamId(request.Id ?? 0), Times.Never);
            _mockDatabaseGateway.Verify(x => x.GetTeamByTeamName(request.Name), Times.Once);
        }


        [Test]
        public void ExecutePostCallsDatabaseGateway()
        {
            var createTeamRequest = TestHelpers.CreateTeamRequest();
            var team = TestHelpers.CreateTeam(name: createTeamRequest.Name, context: createTeamRequest.Context);
            _mockDatabaseGateway.Setup(x => x.CreateTeam(createTeamRequest)).Returns(team);

            _teamsUseCase.ExecutePost(createTeamRequest);

            _mockDatabaseGateway.Verify(x => x.CreateTeam(createTeamRequest));
            _mockDatabaseGateway.Verify(x => x.CreateTeam(It.Is<CreateTeamRequest>(w => w == createTeamRequest)), Times.Once());
        }

        [Test]
        public void ExecutePostReturnsCreatedTeam()
        {
            var createTeamRequest = TestHelpers.CreateTeamRequest();
            var team = TestHelpers.CreateTeam(name: createTeamRequest.Name, context: createTeamRequest.Context);
            _mockDatabaseGateway.Setup(x => x.CreateTeam(createTeamRequest)).Returns(team);

            var response = _teamsUseCase.ExecutePost(createTeamRequest);

            response.Should().BeEquivalentTo(team.ToDomain().ToResponse());
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
