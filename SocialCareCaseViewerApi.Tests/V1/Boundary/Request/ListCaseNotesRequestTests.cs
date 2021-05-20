using Bogus;
using NUnit.Framework;
using SocialCareCaseViewerApi.Tests.V1.Helpers;
using SocialCareCaseViewerApi.V1.Boundary.Requests;

namespace SocialCareCaseViewerApi.Tests.V1.Boundary.Request
{
    [TestFixture]
    public class ListCaseNotesRequestTests
    {
        private ListCaseNotesRequest _request;

        [SetUp]
        public void SetUp()
        {
            _request = new ListCaseNotesRequest();
        }

        [Test]
        public void ValidationFailsIfPersonIdIsNotProvided()
        {
            var errors = ValidationHelper.ValidateModel(_request);

            Assert.AreEqual(1, errors.Count);
        }

        [Test]
        public void ValidationSucceedsWhenValidPersonIdIsProvided()
        {
            _request.Id = "Id123";

            var errors = ValidationHelper.ValidateModel(_request);

            Assert.AreEqual(0, errors.Count);
        }
    }
}
