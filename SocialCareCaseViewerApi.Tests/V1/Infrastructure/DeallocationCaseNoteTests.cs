using NUnit.Framework;
using SocialCareCaseViewerApi.V1.Infrastructure;

namespace SocialCareCaseViewerApi.Tests.V1.Infrastructure
{
    [TestFixture]
    public class DeallocationCaseNoteTests
    {
        private DeallocationCaseNote _deallocationCaseNote;

        [SetUp]
        public void SetUp()
        {
            _deallocationCaseNote = new DeallocationCaseNote();
        }

        [Test]
        public void DeallocationCaseNoteHasDeallocationReason()
        {
            Assert.AreEqual(null, _deallocationCaseNote.DeallocationReason);
        }

        [Test]
        public void CaseNoteBaseHasAllocationId()
        {
            Assert.AreEqual(null, _deallocationCaseNote.AllocationId);
        }

        [Test]
        public void CaseNoteBaseHasCreatedBy()
        {
            Assert.AreEqual(null, _deallocationCaseNote.CreatedBy);
        }
    }
}
