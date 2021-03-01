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
        public void AllocationIsAuditEntity()
        {
            Assert.IsInstanceOf<IAuditEntity>(_allocationSet);
        }

        [Test]
        public void AllocationHasId()
        {
            Assert.AreEqual(0, _allocationSet.Id);
        }

        [Test]
        public void AllocationHasPersonId()
        {
            Assert.AreEqual(0, _allocationSet.PersonId);
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

        [Test]
        public void AllocationHasWorker()
        {
            Assert.IsNull(_allocationSet.Worker);
        }

        [Test]
        public void AllocationHasTeam()
        {
            Assert.IsNull(_allocationSet.Team);
        }

        #region Audit properties

        [Test]
        public void AllocationHasCreatedAt()
        {
            Assert.IsNull(_allocationSet.CreatedAt);
        }

        [Test]
        public void AllocationHasCreatedBy()
        {
            Assert.IsNull(_allocationSet.CreatedBy);
        }

        [Test]
        public void AllocationHasLastUpdatedAt()
        {
            Assert.IsNull(_allocationSet.LastModifiedAt);
        }

        [Test]
        public void AllocationHasLastUpdatedBy()
        {
            Assert.IsNull(_allocationSet.LastModifiedBy);
        }
        #endregion
    }
}
