using NUnit.Framework;
using SocialCareCaseViewerApi.V1.Infrastructure;

namespace SocialCareCaseViewerApi.Tests.V1.Infrastructure
{
    [TestFixture]
    public class AllocationTests
    {
        private AllocationSet _allocationSet;

        [SetUp]
        public void SetUp()
        {
            _allocationSet = new AllocationSet();
        }

        [Test]
        public void AllocationHasId()
        {
            Assert.AreEqual(0, _allocationSet.Id);
        }

        [Test]
        public void AllocationHasMosaicId()
        {
            Assert.AreEqual(0, _allocationSet.MosaicId);
        }

        [Test]
        public void AllocationHasWorkerId()
        {
            Assert.AreEqual(null, _allocationSet.WorkerId);
        }

        [Test]
        public void AllocationHasStartDate()
        {
            Assert.IsNull(_allocationSet.AllocationStartDate);
        }

        [Test]
        public void AllocationHasEndDate()
        {
            Assert.IsNull(_allocationSet.AllocationEndDate);
        }

        [Test]
        public void AllocationHasCaseStatus()
        {
            Assert.IsNull(_allocationSet.CaseStatus);
        }

        [Test]
        public void AllocationHasClosureDate()
        {
            Assert.IsNull(_allocationSet.CaseClosureDate);
        }
    }
}
