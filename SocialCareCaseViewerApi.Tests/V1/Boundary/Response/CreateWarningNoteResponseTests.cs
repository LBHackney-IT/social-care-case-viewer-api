using FluentAssertions;
using NUnit.Framework;
using SocialCareCaseViewerApi.V1.Boundary.Response;

namespace SocialCareCaseViewerApi.Tests.V1.Boundary.Response
{
    [TestFixture]
    public class CreateWarningNoteResponseTests
    {
        private CreateWarningNoteResponse _classUnderTest;

        [SetUp]
        public void SetUp()
        {
            _classUnderTest = new CreateWarningNoteResponse();
        }

        [Test]
        public void ResponseHasWarningNoteId()
        {
            _classUnderTest.WarningNoteId.Should().Be(0);
        }

        [Test]
        public void ResponseHasCaseNoteId()
        {
            _classUnderTest.CaseNoteId.Should().Be(null);
        }
    }
}