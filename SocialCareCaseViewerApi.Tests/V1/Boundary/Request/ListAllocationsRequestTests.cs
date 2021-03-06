using System.Linq;
using NUnit.Framework;
using SocialCareCaseViewerApi.Tests.V1.Helpers;
using SocialCareCaseViewerApi.V1.Boundary.Requests;

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
        public void ValidationFailsWhenMosaicIdAndWorkerIdAreNotProvided()
        {
            var errors = ValidationHelper.ValidateModel(_request);

            Assert.AreEqual(errors.Count, 1);
            Assert.IsTrue(errors.Any(x => x.ErrorMessage.Contains("Please provide either mosaic_id or worker_id")));
        }

        [Test]
        public void ValidationFailsWhenBothMosaicIdAndWorkerIdAreProvided()
        {
            _request.MosaicId = 50;
            _request.WorkerId = 30;

            var errors = ValidationHelper.ValidateModel(_request);

            Assert.AreEqual(errors.Count, 1);
            Assert.IsTrue(errors.Any(x => x.ErrorMessage.Contains("Please provide only mosaic_id or worker_id, but not both")));
        }
    }
}
