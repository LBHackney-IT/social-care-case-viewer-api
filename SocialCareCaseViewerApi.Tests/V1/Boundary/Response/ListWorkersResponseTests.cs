using NUnit.Framework;
using SocialCareCaseViewerApi.V1.Boundary.Response;

namespace SocialCareCaseViewerApi.Tests.V1.Boundary.Response
{
    [TestFixture]
    public class ListWorkersResponseTests
    {
        private ListWorkersResponse _listWorkersResponse;

        [SetUp]
        public void SetUp()
        {
            _listWorkersResponse = new ListWorkersResponse();
        }

        [Test]
        public void ResponseHasWorkersList()
        {
            Assert.IsNull(_listWorkersResponse.Workers);
        }
    }
}
