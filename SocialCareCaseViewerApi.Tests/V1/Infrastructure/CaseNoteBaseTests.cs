using NUnit.Framework;
using SocialCareCaseViewerApi.V1.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SocialCareCaseViewerApi.Tests.V1.Infrastructure
{
    [TestFixture]
    public class CaseNoteBaseTests
    {
        private CaseNoteBase _caseNoteBase;

        [SetUp]
        public void SetUp()
        {
            _caseNoteBase = new CaseNoteBase();
        }

        [Test]
        public void CaseNoteBaseHasCaseFirstName()
        {
            Assert.AreEqual(null, _caseNoteBase.FirstName);
        }

        [Test]
        public void CaseNoteBaseHasLastName()
        {
            Assert.AreEqual(null, _caseNoteBase.LastName);
        }

        [Test]
        public void CaseNoteBaseHasTimestamp()
        {
            Assert.AreEqual(new DateTime(), _caseNoteBase.Timestamp);
        }

        [Test]
        public void CaseNoteBaseHasMosaicId()
        {
            Assert.AreEqual(null, _caseNoteBase.MosaicId);
        }

        [Test]
        public void CaseNoteBaseHasWorkerEmail()
        {
            Assert.AreEqual(null, _caseNoteBase.WorkerEmail);
        }

        [Test]
        public void CaseNoteBaseHasFormNameOverall()
        {
            Assert.CatchAsync(null, _caseNoteBase.FormNameOverall);
        }
    }
}
