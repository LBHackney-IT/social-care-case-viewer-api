using System.Linq;
using NUnit.Framework;
using SocialCareCaseViewerApi.Tests.V1.Helpers;
using SocialCareCaseViewerApi.V1.Boundary.Requests;

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

        [Test]
        public void RequestHasAllocationId()
        {
            Assert.AreEqual(null, _updateAllocationRequest.AllocationId);
        }

        [Test]
        public void RequestHasCreatedBy()
        {
            Assert.AreEqual(null, _updateAllocationRequest.CreatedBy);
        }
        #endregion

        #region validation
        [Test]
        public void ModelValidationFailsIfIdIsNotBiggerThan0()
        {
            _updateAllocationRequest.DeallocationReason = "Sample reason";
            _updateAllocationRequest.AllocationId = "Sample Id";
            _updateAllocationRequest.CreatedBy = "Sample Creator";

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
            _updateAllocationRequest.AllocationId = "Sample Id";
            _updateAllocationRequest.CreatedBy = "Sample Creator";


            var errors = ValidationHelper.ValidateModel(_updateAllocationRequest);
            Assert.IsTrue(errors.Count == 1);
        }

        [Test]
        public void ModelValidationFailsIfDeallocationReasonIsNullOrEmpty()
        {
            var errors = ValidationHelper.ValidateModel(_updateAllocationRequest);
            Assert.IsTrue(errors.Any(x => x.ErrorMessage.Contains("Please provide deallocation reason")));
        }

        [Test]
        public void ModelValidationFailsIfAllocationIdIsNotProvided()
        {
            _updateAllocationRequest.Id = 1;
            _updateAllocationRequest.DeallocationReason = "Sample reason";
            _updateAllocationRequest.CreatedBy = "Sample Creator";


            var errors = ValidationHelper.ValidateModel(_updateAllocationRequest);
            Assert.IsTrue(errors.Count == 1);
        }

        [Test]
        public void ModelValidationReturnsCorrectErrorMessageIfAllocationIdIsNotProvided()
        {
            var errors = ValidationHelper.ValidateModel(_updateAllocationRequest);
            Assert.IsTrue(errors.Any(x => x.ErrorMessage.Contains("AllocationId field is required")));
        }

        [Test]
        public void ModelValidationFailsIfCreatedByIsNotProvided()
        {
            _updateAllocationRequest.Id = 1;
            _updateAllocationRequest.DeallocationReason = "Sample reason";
            _updateAllocationRequest.AllocationId = "Sample Id";


            var errors = ValidationHelper.ValidateModel(_updateAllocationRequest);
            Assert.IsTrue(errors.Count == 1);
        }

        [Test]
        public void ModelValidationReturnsCorrectErrorMessageIfCreatedByIsNotProvided()
        {
            var errors = ValidationHelper.ValidateModel(_updateAllocationRequest);
            Assert.IsTrue(errors.Any(x => x.ErrorMessage.Contains("CreatedBy field is required")));
        }
        #endregion
    }
}
