using System.Collections.Generic;
using Moq;
using NUnit.Framework;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Boundary.Response;
using SocialCareCaseViewerApi.V1.Domain;
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
            var request = new ListTeamsRequest();

            var response = new ListTeamsResponse();

            _mockDatabaseGateway.Setup(x => x.GetTeams(It.IsAny<string>())).Returns(new List<DbTeam>());

            var result = _teamsUseCase.ExecuteGet(request);

            Assert.IsInstanceOf<ListTeamsResponse>(result);
            Assert.IsInstanceOf<List<Team>>(result.Teams);

        }
    }
}
