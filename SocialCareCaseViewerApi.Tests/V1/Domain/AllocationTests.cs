using NUnit.Framework;
using SocialCareCaseViewerApi.V1.Boundary.Response;

namespace SocialCareCaseViewerApi.Tests.V1.Domain
{
    [TestFixture]
    public class AllocationTests
    {
        private Allocation _allocation;

        [SetUp]
        public void SetUp()
        {
            _allocation = new Allocation();
        }

        [Test]
        public void AllocationHasPersonId()
        {
            Assert.AreEqual(0, _allocation.PersonId);
        }


        [Test]
        public void AllocationHasAllocatedWorker()
        {
            Assert.IsNull(_allocation.AllocatedWorker);
        }

        [Test]
        public void AllocationHasWorkerType()
        {
            Assert.IsNull(_allocation.WorkerType);
        }

        [Test]
        public void AllocationHasAllocatedWorkerTeam()
        {
            Assert.IsNull(_allocation.AllocatedWorkerTeam);
        }

        [Test]
        public void AllocationHasAllocationStartDate()
        {
            Assert.IsNull(_allocation.AllocationStartDate);
        }

        [Test]
        public void AllocationHasAllocationEndDate()
        {
            Assert.IsNull(_allocation.AllocationEndDate);
        }

        [Test]
        public void AllocationHasCaseStatus()
        {
            Assert.IsNull(_allocation.CaseStatus);
        }
    }
}
