using NUnit.Framework;
using SocialCareCaseViewerApi.Tests.V1.Helpers;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using System.Linq;

namespace SocialCareCaseViewerApi.Tests.V1.Boundary.Request
{
    [TestFixture]
    public class UpdateAllocationRequestTests
    {
        private UpdateAllocationRequest _updateAllocationRequest;

        #region Properties
        [SetUp]
        public void SetUp()
        {
            _updateAllocationRequest = new UpdateAllocationRequest();
        }

        [Test]
        public void RequestHasId()
        {
            Assert.AreEqual(0, _updateAllocationRequest.Id);
        }

        [Test]
        public void RequestHasDeallocationReason()
        {
            Assert.AreEqual(null, _updateAllocationRequest.DeallocationReason);
        }
        #endregion

        #region validation
        [Test]
        public void ModelValidationFailsIfIdIsNotBiggerThan0()
        {
            _updateAllocationRequest.DeallocationReason = "Sample reason";

            var errors = ValidationHelper.ValidateModel(_updateAllocationRequest);

            Assert.IsTrue(errors.Count == 1);
        }

        [Test]
        public void ModelValidationReturnsCorrectErrroMessageWhenIdIsNotBiggerThan0()
        {
            var errors = ValidationHelper.ValidateModel(_updateAllocationRequest);
            Assert.IsTrue(errors.Any(x => x.ErrorMessage.Contains("Please enter a value bigger than 0")));
        }

        [Test]
        public void ModelValidationFailsIfDeallocationReasonIsNotProvided()
        {
            _updateAllocationRequest.Id = 1;

            var errors = ValidationHelper.ValidateModel(_updateAllocationRequest);
            Assert.IsTrue(errors.Count == 1);
        }

        [Test]
        public void ModelValidationFailsIfDeallocationReasonIsNullOrEmpty()
        {
            var errors = ValidationHelper.ValidateModel(_updateAllocationRequest);
            Assert.IsTrue(errors.Any(x => x.ErrorMessage.Contains("Please provide deallocation reason")));
        }

        #endregion
    }
}
