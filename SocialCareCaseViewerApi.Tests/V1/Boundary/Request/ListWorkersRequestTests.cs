using System.Linq;
using NUnit.Framework;
using SocialCareCaseViewerApi.Tests.V1.Helpers;
using SocialCareCaseViewerApi.V1.Boundary.Requests;

namespace SocialCareCaseViewerApi.Tests.V1.Boundary.Request
{
    [TestFixture]
    public class ListWorkersRequestTests
    {
        private ListWorkersRequest _listWorkersRequest;

        [SetUp]
        public void SetUp()
        {
            _listWorkersRequest = new ListWorkersRequest();
        }

        [Test]
        public void RequestHasId()
        {
            Assert.AreEqual(0, _listWorkersRequest.WorkerId);
        }

        [Test]
        public void RequestHasTeamID()
        {
            Assert.AreEqual(0, _listWorkersRequest.TeamId);
        }

        #region model validation
        [Test]
        public void ValidationFailsWhenTeamIdAndIdAreNotProvided()
        {
            var errors = ValidationHelper.ValidateModel(_listWorkersRequest);

            Assert.AreEqual(errors.Count, 1);
            Assert.IsTrue(errors.Any(x => x.ErrorMessage.Contains("Please provide either worker id or team id")));

        }

        [Test]
        public void ValidationFailsWhenBothMosaicIdAndWorkerIdAreProvided()
        {
            _listWorkersRequest.WorkerId = 50;
            _listWorkersRequest.TeamId = 30;

            var errors = ValidationHelper.ValidateModel(_listWorkersRequest);

            Assert.AreEqual(errors.Count, 1);
            Assert.IsTrue(errors.Any(x => x.ErrorMessage.Contains("Please provide only worker id or team id, but not both")));
        }
        #endregion
    }
}
