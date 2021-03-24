using FluentAssertions;
using NUnit.Framework;
using SocialCareCaseViewerApi.V1.Boundary.Response;

namespace SocialCareCaseViewerApi.Tests.V1.Boundary.Response
{
    [TestFixture]
    public class ListWarningNotesResponseTests
    {
        private ListWarningNotesResponse _classUnderTest;

        [SetUp]
        public void SetUp()
        {
            _classUnderTest = new ListWarningNotesResponse();
        }

        [Test]
        public void ResponseHasListOfCaseNotes()
        {
            _classUnderTest.WarningNotes.Should().BeNull();
        }
    }
}