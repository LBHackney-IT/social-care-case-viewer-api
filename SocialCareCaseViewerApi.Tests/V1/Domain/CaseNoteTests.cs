using NUnit.Framework;
using SocialCareCaseViewerApi.V1.Domain;
using System;

namespace SocialCareCaseViewerApi.Tests.V1.Domain
{
    [TestFixture]
    public class CaseNoteTests
    {
        private CaseNote _caseNote;

        [SetUp]
        public void SetUp()
        {
            _caseNote = new CaseNote();
        }

        [Test]
        public void CaseNoteHasMosaicId()
        {
            Assert.IsNull(_caseNote.MosaicId);
        }

        [Test]
        public void CaseNoteHasCaseNoteTitle()
        {
            Assert.IsNull(_caseNote.CaseNoteTitle);
        }

        [Test]
        public void CaseNoteHasContent()
        {
            Assert.IsNull(_caseNote.CaseNoteContent);
        }

        [Test]
        public void CaseNoteHasCaseNoteId()
        {
            Assert.IsNull(_caseNote.CaseNoteId);
        }

        [Test]
        public void CaseNoteHasCreatedOn()
        {
            Assert.AreEqual(DateTime.MinValue, _caseNote.CreatedOn);
        }

        [Test]
        public void CaseNoteHasCreatedByEmail()
        {
            Assert.IsNull(_caseNote.CreatedByEmail);
        }

        [Test]
        public void CaseNoteHasCreatedByName()
        {
            Assert.IsNull(_caseNote.CreatedByName);
        }

        [Test]
        public void CaseNoteHasNoteType()
        {
            Assert.IsNull(_caseNote.NoteType);
        }

    }
}
