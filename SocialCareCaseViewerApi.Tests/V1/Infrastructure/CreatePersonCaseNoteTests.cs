using NUnit.Framework;
using SocialCareCaseViewerApi.V1.Infrastructure;

namespace SocialCareCaseViewerApi.Tests.V1.Infrastructure
{
    [TestFixture]
    public class CreatePersonCaseNoteTests
    {
        private CreatePersonCaseNote _note;

        [SetUp]
        public void SetUp()
        {
            _note = new CreatePersonCaseNote();
        }

        [Test]
        public void NoteHasCreatedBy()
        {
            Assert.IsNull(_note.CreatedBy);
        }
    }
}
