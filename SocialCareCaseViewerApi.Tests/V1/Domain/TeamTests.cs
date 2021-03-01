using NUnit.Framework;
using SocialCareCaseViewerApi.V1.Domain;

namespace SocialCareCaseViewerApi.Tests.V1.Domain
{
    [TestFixture]
    public class TeamTests
    {
        private Team _team;

        [SetUp]
        public void Setup()
        {
            _team = new Team();
        }

        [Test]
        public void TeamHasId()
        {
            Assert.AreEqual(0, _team.Id);
        }

        [Test]
        public void TeamHasName()
        {
            Assert.IsNull(_team.Name);
        }
    }
}
