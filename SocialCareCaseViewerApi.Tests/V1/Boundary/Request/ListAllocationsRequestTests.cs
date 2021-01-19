using NUnit.Framework;
using SocialCareCaseViewerApi.Tests.V1.Helpers;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using System.Linq;

namespace SocialCareCaseViewerApi.Tests.V1.Boundary.Request
{
    [TestFixture]
    public class ListAllocationsRequestTests
    {

        private ListAllocationsRequest _request;

        [SetUp]
        public void SetUp()
        {
            _request = new ListAllocationsRequest();
        }

        [Test]
        public void RequestHasMosaicId()
        {
            Assert.AreEqual(0, _request.MosaicId);
        }

        #region Model validation
        [Test]
        public void ValidationFailsIfMosaicIdIsNotBiggerThan0()
        {
            var errors = ValidationHelper.ValidateModel(_request);

            Assert.AreEqual(errors.Count, 1);
            Assert.IsTrue(errors.Any(x => x.ErrorMessage.Contains("Please enter a value bigger than 0")));
        }
        #endregion
    }
}
