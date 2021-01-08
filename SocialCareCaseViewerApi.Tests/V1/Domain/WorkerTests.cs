using NUnit.Framework;
using SocialCareCaseViewerApi.V1.Domain;

namespace SocialCareCaseViewerApi.Tests.V1.Domain
{
    [TestFixture]
    public class WorkerTests
    {
        private Worker _worker = new Worker();

        [SetUp]
        public void SetUp()
        {
            _worker = new Worker();
        }

        [Test]
        public void WorkerHasId()
        {
            Assert.AreEqual(0, _worker.Id);
        }

        [Test]
        public void WorkerHasEmail()
        {
            Assert.IsNull(_worker.Email);
        }

        [Test]
        public void WorkerHasFirstName()
        {
            Assert.IsNull(_worker.FirstName);
        }

        [Test]
        public void WorkerHasLastName()
        {
            Assert.IsNull(_worker.LastName);
        }

        [Test]
        public void WorkerHasTeamId()
        {
            Assert.IsNull(_worker.TeamId);
        }

        [Test]
        public void WorkerHasRole()
        {
            Assert.IsNull(_worker.Role);
        }
    }
}
