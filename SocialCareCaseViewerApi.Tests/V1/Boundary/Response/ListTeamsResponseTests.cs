using NUnit.Framework;
using SocialCareCaseViewerApi.V1.Boundary.Response;

namespace SocialCareCaseViewerApi.Tests.V1.Boundary.Response
{
    [TestFixture]
    public class ListTeamsResponseTests
    {
        private ListTeamsResponse _listTeamsResponse;

        [SetUp]
        public void SetUp()
        {
            _listTeamsResponse = new ListTeamsResponse();
        }

        public void RequestHasContext()
        {
            Assert.IsNull(_listTeamsResponse.Teams);
        }

    }
}
