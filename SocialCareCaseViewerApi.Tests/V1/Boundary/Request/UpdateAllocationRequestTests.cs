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
        public void RequestHasCreatedBy()
        {
            Assert.AreEqual(null, _updateAllocationRequest.CreatedBy);
        }

        private static UpdateAllocationRequest GetValidUpdateAllocationRequest()
        {
            return new UpdateAllocationRequest()
            {
                CreatedBy = "test@.domain.",
                DeallocationReason = "My reason",
                Id = 1
            };
        }
        #endregion

        #region validation
        [Test]
        public void ModelValidationFailsIfIdIsNotBiggerThan0()
        {
            UpdateAllocationRequest _request = GetValidUpdateAllocationRequest();
            _request.Id = 0;

            var errors = ValidationHelper.ValidateModel(_request);

            Assert.IsTrue(errors.Count == 1);
        }

        [Test]
        public void ModelValidationReturnsCorrectErrorMessageWhenIdIsNotBiggerThan0()
        {
            UpdateAllocationRequest _request = GetValidUpdateAllocationRequest();
            _request.Id = 0;

            var errors = ValidationHelper.ValidateModel(_request);
            Assert.IsTrue(errors.Any(x => x.ErrorMessage.Contains("Please enter a value bigger than 0")));
        }

        [Test]
        public void ModelValidationFailsIfDeallocationReasonIsNotProvided()
        {
            UpdateAllocationRequest _request = GetValidUpdateAllocationRequest();
            _request.DeallocationReason = null;

            var errors = ValidationHelper.ValidateModel(_request);
            Assert.IsTrue(errors.Count == 1);
        }

        [Test]
        public void ModelValidationReturnsCorrectErrorMessageIfDeallocationReasonIsNullOrEmpty()
        {
            UpdateAllocationRequest _request = GetValidUpdateAllocationRequest();
            _request.DeallocationReason = null;

            var errors = ValidationHelper.ValidateModel(_request);
            Assert.IsTrue(errors.Any(x => x.ErrorMessage.Contains("Please provide deallocation reason")));
        }


        [Test]
        public void ModelValidationReturnsCorrectErrorMessageIfIdIsNotProvided()
        {
            UpdateAllocationRequest _request = GetValidUpdateAllocationRequest();
            _request.Id = 0;

            var errors = ValidationHelper.ValidateModel(_request);
            Assert.IsTrue(errors.Any(x => x.ErrorMessage.Contains("Please enter a value bigger than 0")));
        }

        [Test]
        public void ModelValidationFailsIfCreatedByIsNotProvided()
        {
            UpdateAllocationRequest _request = GetValidUpdateAllocationRequest();
            _request.CreatedBy = null;

            var errors = ValidationHelper.ValidateModel(_request);
            Assert.IsTrue(errors.Count == 1);
        }

        [Test]
        public void ModelValidationReturnsCorrectErrorMessageIfCreatedByIsNotProvided()
        {
            UpdateAllocationRequest _request = GetValidUpdateAllocationRequest();
            _request.CreatedBy = null;

            var errors = ValidationHelper.ValidateModel(_request);
            Assert.IsTrue(errors.Any(x => x.ErrorMessage.Contains("CreatedBy field is required")));
        }

        [Test]
        public void ModelValidationReturnsCorrectErrorMessageIfCreatedByIsNotValidEmailAddress()
        {
            UpdateAllocationRequest _request = GetValidUpdateAllocationRequest();
            _request.CreatedBy = "invalidEmailAddress";

            var errors = ValidationHelper.ValidateModel(_request);
            Assert.IsTrue(errors.Any(x => x.ErrorMessage.Contains("The CreatedBy field is not a valid e-mail address.")));
        }
        #endregion
    }
}
