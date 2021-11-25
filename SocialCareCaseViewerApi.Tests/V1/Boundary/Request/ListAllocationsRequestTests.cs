using System.Linq;
using NUnit.Framework;
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
        public void ValidationFailsWhenNoAttributesAreProvided()
        {
            var validator = new ListAllocationsRequestValidator();
            var validationResults = validator.Validate(_request);

            Assert.IsFalse(validationResults.IsValid);
            Assert.IsTrue(validationResults.Errors.Any(x => x.ErrorMessage.Contains("Please provide either mosaic_id, worker_id or worker_email")));
        }

        [Test]
        public void ValidationFailsWhenTwoOrMoreAttributesAreProvided()
        {
            _request.MosaicId = 50;
            _request.WorkerId = 30;
            _request.WorkerEmail = "test@example.com";

            var validator = new ListAllocationsRequestValidator();
            var validationResults = validator.Validate(_request);

            Assert.IsFalse(validationResults.IsValid);
            Assert.IsTrue(validationResults.Errors.Any(x => x.ErrorMessage.Contains("Please provide only one of mosaic_id, worker_id or worker_email")));
        }

        [Test]
        public void ValidationFailsWhenTheWorkerEmailProvidedIsNotAnEmailAddress()
        {
            _request.WorkerEmail = "not-an-email";

            var validator = new ListAllocationsRequestValidator();
            var validationResults = validator.Validate(_request);

            Assert.IsFalse(validationResults.IsValid);
            Assert.IsTrue(validationResults.Errors.Any(x => x.ErrorMessage.Contains("Please provide a valid email address for worker_email")));
        }
    }
}
