using NUnit.Framework;
using SocialCareCaseViewerApi.V1.Infrastructure;

namespace SocialCareCaseViewerApi.Tests.V1.Infrastructure
{
    [TestFixture]
    public class TeamTests
    {
        private Team _team;

        [SetUp]
        public void SetUp()
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
            Assert.AreEqual(null, _team.Name);
        }

        [Test]
        public void TeamHasContext()
        {
            Assert.AreEqual(null, _team.Context);
        }
    }
}
