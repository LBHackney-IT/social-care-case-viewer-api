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
        public void RequestHasTeamID()
        {
            Assert.AreEqual(0, _listWorkersRequest.TeamId);
        }

        #region model validation
        [Test]
        public void ModelValidationFailsIfTeamIdIsNotBiggerThan0() //TODO: move message check to separate tests
        {
            ListWorkersRequest request = new ListWorkersRequest();
            var errors = ValidationHelper.ValidateModel(request);
            Assert.IsTrue(errors.Any(x => x.ErrorMessage.Contains("Please enter a value bigger than 0")));
            Assert.IsTrue(errors.Count == 1);
        }
        #endregion
    }
}
