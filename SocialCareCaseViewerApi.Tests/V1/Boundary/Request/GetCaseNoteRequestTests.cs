using NUnit.Framework;
using SocialCareCaseViewerApi.Tests.V1.Helpers;
using SocialCareCaseViewerApi.V1.Boundary.Requests;

namespace SocialCareCaseViewerApi.Tests.V1.Boundary.Request
{
    [TestFixture]
    public class GetCaseNoteRequestTests
    {
        private GetCaseNotesRequest _request;

        [SetUp]
        public void SetUp()
        {
            _request = new GetCaseNotesRequest();
        }

        [Test]
        public void RequestHasId()
        {
            Assert.IsNull(_request.Id);
        }

        #region Model validation
        [Test]
        public void ValidationFailsIfIdIsNotProvided()
        {
            var errors = ValidationHelper.ValidateModel(_request);

            Assert.AreEqual(1, errors.Count);
        }

        [Test]
        public void ValidationSucceeedsWhenValidPersonIdIsProvided()
        {
            _request.Id = "Id123";

            var errors = ValidationHelper.ValidateModel(_request);

            Assert.AreEqual(0, errors.Count);
        }
        #endregion
    }
}
