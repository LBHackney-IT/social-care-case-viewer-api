using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using SocialCareCaseViewerApi.V1.Gateways;
using SocialCareCaseViewerApi.Tests.V1.Helpers;
using SocialCareCaseViewerApi.V1.Infrastructure;
using Allocation = SocialCareCaseViewerApi.V1.Infrastructure.AllocationSet;
using dbAddress = SocialCareCaseViewerApi.V1.Infrastructure.Address;
using PhoneNumberInfrastructure = SocialCareCaseViewerApi.V1.Infrastructure.PhoneNumber;


namespace SocialCareCaseViewerApi.Tests.V1.Gateways.Database
{
    [TestFixture]
    public class GetTeamListTests : DatabaseTests
    {
        private TeamGateway _teamGateway;

        [SetUp]
        public void Setup()
        {
            _teamGateway = new TeamGateway(DatabaseContext);
        }

        [Test]
        public void GetTeamsByTeamContextFlagReturnsTeamsOrderedByName()
        {
            DatabaseContext.Teams.AddRange(new List<Team>()
            {
                TestHelpers.CreateTeam(context: "A", name: "B"),
                TestHelpers.CreateTeam(context: "A", name: "C"),
                TestHelpers.CreateTeam(context: "A", name: "A")
            });
            DatabaseContext.SaveChanges();
            var teams = _teamGateway.GetTeamsByTeamContextFlag("A");

            teams.Should().BeInAscendingOrder(x => x.Name);
        }
    }
}
