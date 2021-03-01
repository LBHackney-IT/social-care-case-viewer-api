using NUnit.Framework;
using SocialCareCaseViewerApi.V1.Infrastructure;

namespace SocialCareCaseViewerApi.Tests.V1.Infrastructure
{
    [TestFixture]
    public class AllocationCaseNoteTests
    {
        private AllocationCaseNote _allocationCaseNote;

        [SetUp]
        public void SetUp()
        {
            _allocationCaseNote = new AllocationCaseNote();
        }

        [Test]
        public void DeallocationCaseNoteHasDeallocationReason()
        {
            Assert.AreEqual(null, _allocationCaseNote.Note);
        }

        [Test]
        public void CaseNoteBaseHasAllocationId()
        {
            Assert.AreEqual(null, _allocationCaseNote.AllocationId);
        }

        [Test]
        public void CaseNoteBaseHasCreatedBy()
        {
            Assert.AreEqual(null, _allocationCaseNote.CreatedBy);
        }
    }
}
