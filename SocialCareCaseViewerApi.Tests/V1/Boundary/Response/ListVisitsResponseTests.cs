using NUnit.Framework;
using SocialCareCaseViewerApi.V1.Boundary.Response;

namespace SocialCareCaseViewerApi.Tests.V1.Boundary.Response
{
    public class ListVisitsResponseTests
    {
        private ListVisitsResponse _response;

        [SetUp]
        public void SetUp()
        {
            _response = new ListVisitsResponse();
        }

        [Test]
        public void ResponseHasListOfCaseNotes()
        {
            Assert.IsNull(_response.Visits);
        }
    }
}
