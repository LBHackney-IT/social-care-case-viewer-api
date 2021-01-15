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
    }
}
