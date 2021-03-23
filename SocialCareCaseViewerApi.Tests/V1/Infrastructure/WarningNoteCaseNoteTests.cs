using FluentAssertions;
using NUnit.Framework;
using SocialCareCaseViewerApi.V1.Infrastructure;

namespace SocialCareCaseViewerApi.Tests.V1.Infrastructure
{
    [TestFixture]
    public class WarningNoteCaseNoteTests
    {
        private WarningNoteCaseNote _classUnderTest;

        [SetUp]
        public void SetUp()
        {
            _classUnderTest = new WarningNoteCaseNote();
        }

        [Test]
        public void DeallocationCaseNoteHasDeallocationReason()
        {
            _classUnderTest.Note.Should().BeNull();
        }

        [Test]
        public void CaseNoteBaseHasAllocationId()
        {
            _classUnderTest.WarningNoteId.Should().BeNull();
        }
    }
}
