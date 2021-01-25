using System.Linq;
using AutoFixture;
using NUnit.Framework;
using SocialCareCaseViewerApi.Tests.V1.Helpers;
using SocialCareCaseViewerApi.V1.Boundary.Requests;

namespace SocialCareCaseViewerApi.Tests.V1.Boundary.Request
{
    [TestFixture]
    public class CreateAllocationRequestTests
    {

        private CreateAllocationRequest _createAllocationRequest;
        private Fixture _fixture;

        [SetUp]
        public void SetUp()
        {
            _createAllocationRequest = new CreateAllocationRequest();
            _fixture = new Fixture();
        }

        [Test]
        public void RequestHasMosaicId()
        {
            Assert.AreEqual(0, _createAllocationRequest.MosaicId);
        }

        [Test]
        public void RequestHasAllocatedWorkerId()
        {
            Assert.AreEqual(0, _createAllocationRequest.AllocatedWorkerId);
        }

        [Test]
        public void RequestHasAllocatedBy()
        {
            Assert.IsNull(_createAllocationRequest.AllocatedBy);
        }

        #region Model validation
        [Test]
        public void ValidationFailsIfMosaicIdIsNotBiggerThan0()
        {
            CreateAllocationRequest request = new CreateAllocationRequest() { AllocatedBy = _fixture.Create<string>(), AllocatedWorkerId = _fixture.Create<int>() };

            var errors = ValidationHelper.ValidateModel(request);

            Assert.AreEqual(errors.Count, 1);
            Assert.IsTrue(errors.Any(x => x.ErrorMessage.Contains("Please enter a value bigger than 0")));
        }

        [Test]
        public void ValidationFailsIfAllocatedWorkerIdIsNotBiggerThan0()
        {
            CreateAllocationRequest request = new CreateAllocationRequest() { AllocatedBy = _fixture.Create<string>(), MosaicId = _fixture.Create<long>() };

            var errors = ValidationHelper.ValidateModel(request);

            Assert.AreEqual(errors.Count, 1);
            Assert.IsTrue(errors.Any(x => x.ErrorMessage.Contains("Please enter a value bigger than 0")));
        }

        [Test]
        public void ValidationFailsIfAllocatedByIsNotProvided()
        {
            CreateAllocationRequest request = new CreateAllocationRequest() { MosaicId = _fixture.Create<long>(), AllocatedWorkerId = _fixture.Create<int>() };

            var errors = ValidationHelper.ValidateModel(request);

            Assert.AreEqual(errors.Count, 1);
            Assert.IsTrue(errors.Any(x => x.ErrorMessage.Contains("AllocatedBy field is required")));
        }
        #endregion
    }
}
