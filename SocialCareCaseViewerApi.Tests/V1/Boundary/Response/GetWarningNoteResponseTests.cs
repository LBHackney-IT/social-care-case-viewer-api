using FluentAssertions;
using NUnit.Framework;
using SocialCareCaseViewerApi.V1.Boundary.Response;

namespace SocialCareCaseViewerApi.Tests.V1.Boundary.Response
{
    [TestFixture]
    public class GetWarningNoteResponseTests
    {
        private GetWarningNoteResponse _classUnderTest;

        [SetUp]
        public void SetUp()
        {
            _classUnderTest = new GetWarningNoteResponse();
        }

        [Test]
        public void ResponseHasListOfCaseNotes()
        {
            _classUnderTest.WarningNotes.Should().BeNull();
        }
    }
}
