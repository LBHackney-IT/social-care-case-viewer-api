using NUnit.Framework;
using SocialCareCaseViewerApi.V1.Infrastructure;

namespace SocialCareCaseViewerApi.Tests.V1.Infrastructure
{
    [TestFixture]
    public class WorkerTeamTests
    {
        private WorkerTeam _workerTeam;

        [SetUp]
        public void SetUp()
        {
            _workerTeam = new WorkerTeam();
        }

        [Test]
        public void WorkerTeamHasWorkerId()
        {
            Assert.IsNull(_workerTeam.WorkerId);
        }

        [Test]
        public void WorkerTeamHasTeamId()
        {
            Assert.IsNull(_workerTeam.TeamId);
        }

        [Test]
        public void WorkerTeamHasWorker()
        {
            Assert.IsNull(_workerTeam.Worker);
        }

        [Test]
        public void WorkerTeamHasTeam()
        {
            Assert.IsNull(_workerTeam.Team);
        }
    }
}
