using System.Linq;
using NUnit.Framework;
using SocialCareCaseViewerApi.Tests.V1.Helpers;
using SocialCareCaseViewerApi.V1.Boundary.Requests;

namespace SocialCareCaseViewerApi.Tests.V1.Boundary.Request
{
    [TestFixture]
    public class GetWorkersRequestTests
    {
        private GetWorkersRequest _getWorkersRequest;

        [SetUp]
        public void SetUp()
        {
            _getWorkersRequest = new GetWorkersRequest();
        }

        [Test]
        public void RequestHasId()
        {
            Assert.AreEqual(0, _getWorkersRequest.WorkerId);
        }

        [Test]
        public void RequestHasTeamID()
        {
            Assert.AreEqual(0, _getWorkersRequest.TeamId);
        }
    }
}
