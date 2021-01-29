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
            Assert.AreEqual(null, _createAllocationRequest.AllocatedBy);
        }

        [Test]
        public void RequestHasAllocationId()
        {
            Assert.AreEqual(null, _createAllocationRequest.AllocationId);
        }

        [Test]
        public void RequestHasCreatedBy()
        {
            Assert.AreEqual(null, _createAllocationRequest.CreatedBy);
        }

        #region Model validation
        [Test]
        public void ValidationFailsIfMosaicIdIsNotBiggerThan0()
        {
            CreateAllocationRequest request = new CreateAllocationRequest()
            {
                AllocatedBy = _fixture.Create<string>(),
                AllocatedWorkerId = _fixture.Create<int>(),
                AllocationId = _fixture.Create<string>(),
                CreatedBy = _fixture.Create<string>()
            };

            var errors = ValidationHelper.ValidateModel(request);

            Assert.AreEqual(errors.Count, 1);
            Assert.IsTrue(errors.Any(x => x.ErrorMessage.Contains("Please enter a value bigger than 0")));
        }

        [Test]
        public void ValidationFailsIfAllocatedWorkerIdIsNotBiggerThan0()
        {
            CreateAllocationRequest request = new CreateAllocationRequest()
            {
                AllocatedBy = _fixture.Create<string>(),
                MosaicId = _fixture.Create<long>(),
                AllocationId = _fixture.Create<string>(),
                CreatedBy = _fixture.Create<string>()
            };

            var errors = ValidationHelper.ValidateModel(request);

            Assert.AreEqual(errors.Count, 1);
            Assert.IsTrue(errors.Any(x => x.ErrorMessage.Contains("Please enter a value bigger than 0")));
        }

        [Test]
        public void ValidationFailsIfAllocatedByIsNotProvided()
        {
            CreateAllocationRequest request = new CreateAllocationRequest()
            {
                MosaicId = _fixture.Create<long>(),
                AllocatedWorkerId = _fixture.Create<int>(),
                AllocationId = _fixture.Create<string>(),
                CreatedBy = _fixture.Create<string>()
            };

            var errors = ValidationHelper.ValidateModel(request);

            Assert.AreEqual(errors.Count, 1);
            Assert.IsTrue(errors.Any(x => x.ErrorMessage.Contains("AllocatedBy field is required")));
        }

        [Test]
        public void ValidationFailsIfAllocationIdIsNotProvided()
        {
            CreateAllocationRequest request = new CreateAllocationRequest()
            {
                AllocatedBy = _fixture.Create<string>(),
                MosaicId = _fixture.Create<long>(),
                AllocatedWorkerId = _fixture.Create<int>(),
                CreatedBy = _fixture.Create<string>()
            };

            var errors = ValidationHelper.ValidateModel(request);

            Assert.AreEqual(errors.Count, 1);
            Assert.IsTrue(errors.Any(x => x.ErrorMessage.Contains("AllocationId field is required")));
        }

        [Test]
        public void ValidationFailsIfCreatedByIsNotProvided()
        {
            CreateAllocationRequest request = new CreateAllocationRequest()
            {
                AllocatedBy = _fixture.Create<string>(),
                MosaicId = _fixture.Create<long>(),
                AllocatedWorkerId = _fixture.Create<int>(),
                AllocationId = _fixture.Create<string>()
            };

            var errors = ValidationHelper.ValidateModel(request);

            Assert.AreEqual(errors.Count, 1);
            Assert.IsTrue(errors.Any(x => x.ErrorMessage.Contains("CreatedBy field is required")));
        }
        #endregion
    }
}
