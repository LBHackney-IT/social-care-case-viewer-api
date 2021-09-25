using FluentAssertions;
using NUnit.Framework;
using SocialCareCaseViewerApi.Tests.V1.Helpers;
using SocialCareCaseViewerApi.V1.Domain;
using SocialCareCaseViewerApi.V1.Gateways;

#nullable enable
namespace SocialCareCaseViewerApi.Tests.V1.Gateways.TeamGatewayTests
{
    [TestFixture]
    public class CreateTeamTests : DatabaseTests
    {
        private TeamGateway _teamGateway = null!;

        [SetUp]
        public void Setup()
        {
            _teamGateway = new TeamGateway(DatabaseContext);
        }

        [Test]
        public void CreateTeamInsertsTeamIntoDatabaseAndReturnsCreatedTeam()
        {
            var createTeamRequest = TestHelpers.CreateTeamRequest();

            var returnedTeam = _teamGateway.CreateTeam(createTeamRequest);

            returnedTeam.Should().BeEquivalentTo(new Team
            {
                Context = createTeamRequest.Context,
                Name = createTeamRequest.Name,
                Id = returnedTeam.Id
            });
        }
    }
}
