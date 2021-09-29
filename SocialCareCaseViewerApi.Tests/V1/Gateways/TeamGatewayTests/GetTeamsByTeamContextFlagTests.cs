using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using SocialCareCaseViewerApi.Tests.V1.Helpers;
using SocialCareCaseViewerApi.V1.Gateways;
using SocialCareCaseViewerApi.V1.Infrastructure;

#nullable enable
namespace SocialCareCaseViewerApi.Tests.V1.Gateways.TeamGatewayTests
{
    [TestFixture]
    public class GetTeamsByTeamContextFlagTests : DatabaseTests
    {
        private TeamGateway _teamGateway = null!;

        [SetUp]
        public void Setup()
        {
            _teamGateway = new TeamGateway(DatabaseContext);
        }

        [Test]
        public void GetTeamByContextReturnsEmptyListWhenNoTeamsMatchContextFlag()
        {
            var response = _teamGateway.GetTeamsByTeamContextFlag("A");

            response.Count().Should().Be(0);
        }

        [Test]
        public void GetTeamByContextReturnsListOfTeamsThatMatchContextFlag()
        {
            var workerTeamA1 = TestHelpers.CreateTeam(context: "a");
            var workerTeamC1 = TestHelpers.CreateTeam(context: "c");
            var workerTeamC2 = TestHelpers.CreateTeam(context: "C");

            SaveTeamToDatabase(workerTeamA1);
            SaveTeamToDatabase(workerTeamC1);
            SaveTeamToDatabase(workerTeamC2);

            var responseAdults = _teamGateway.GetTeamsByTeamContextFlag("A");
            var responseChildrens = _teamGateway.GetTeamsByTeamContextFlag("C");

            responseAdults.Should().BeEquivalentTo(new List<Team> { workerTeamA1 });
            responseChildrens.Should().BeEquivalentTo(new List<Team> { workerTeamC1, workerTeamC2 });
        }

        private void SaveTeamToDatabase(Team team)
        {
            DatabaseContext.Teams.Add(team);
            DatabaseContext.SaveChanges();
        }
    }
}
