using System.Collections.Generic;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SocialCareCaseViewerApi.Tests.V1.Helpers;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Boundary.Response;
using SocialCareCaseViewerApi.V1.Domain;
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
            var request = new GetTeamsRequest();

            _mockDatabaseGateway.Setup(x => x.GetTeams(It.IsAny<string>())).Returns(new List<DbTeam>());

            var result = _teamsUseCase.ExecuteGet(request);

            Assert.IsInstanceOf<ListTeamsResponse>(result);
            Assert.IsInstanceOf<List<Team>>(result.Teams);

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

        // todo once we add GET team via team name
        // [Test]
        // public void ExecutePostThrowsPostTeamExceptionIfTeamNameAlreadyInUse()
        // {
        //     var createTeamRequest = TestHelpers.CreateTeamRequest();
        //     var team = TestHelpers.CreateTeam(name: createTeamRequest.Name, context: createTeamRequest.Context);
        // }
    }
}
