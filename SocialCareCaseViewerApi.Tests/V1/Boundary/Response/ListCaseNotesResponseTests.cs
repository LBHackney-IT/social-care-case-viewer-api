using NUnit.Framework;
using SocialCareCaseViewerApi.V1.Boundary.Response;

namespace SocialCareCaseViewerApi.Tests.V1.Boundary.Response
{
    [TestFixture]
    public class ListCaseNotesResponseTests
    {
        private ListCaseNotesResponse _response;

        [SetUp]
        public void SetUp()
        {
            _response = new ListCaseNotesResponse();
        }

        [Test]
        public void ResponseHasListOfCaseNotes()
        {
            Assert.IsNull(_response.CaseNotes);
        }
    }
}
