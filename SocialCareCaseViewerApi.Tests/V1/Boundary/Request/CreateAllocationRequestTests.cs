using System.Linq;
using AutoFixture;
using Bogus;
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
        private Faker _faker;

        [SetUp]
        public void SetUp()
        {
            _createAllocationRequest = new CreateAllocationRequest();
            _fixture = new Fixture();
            _faker = new Faker();
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
        public void RequestHasCreatedBy()
        {
            Assert.AreEqual(null, _createAllocationRequest.CreatedBy);
        }

        [Test]
        public void RequestHasAllocatedTeamId()
        {
            Assert.AreEqual(0, _createAllocationRequest.AllocatedTeamId);
        }

        private CreateAllocationRequest GetValidCreateAllocationRequest()
        {
            return new CreateAllocationRequest()
            {
                MosaicId = _fixture.Create<int>(),
                AllocatedWorkerId = _fixture.Create<int>(),
                AllocatedTeamId = _fixture.Create<int>(),
                CreatedBy = _faker.Internet.Email(),
            };
        }

        #region Model validation
        [Test]
        public void ValidationFailsIfMosaicIdIsNotBiggerThan0()
        {
            CreateAllocationRequest request = GetValidCreateAllocationRequest();
            request.MosaicId = 0;

            var errors = ValidationHelper.ValidateModel(request);

            Assert.AreEqual(errors.Count, 1);
            Assert.IsTrue(errors.Any(x => x.ErrorMessage.Contains("Please enter a value bigger than 0")));
        }

        [Test]
        public void ValidationFailsIfAllocatedWorkerIdIsNotBiggerThan0()
        {
            CreateAllocationRequest request = GetValidCreateAllocationRequest();
            request.AllocatedWorkerId = 0;

            var errors = ValidationHelper.ValidateModel(request);

            Assert.AreEqual(errors.Count, 1);
            Assert.IsTrue(errors.Any(x => x.ErrorMessage.Contains("Please enter a value bigger than 0")));
        }

        [Test]
        public void ValidationFailsIfAllocatedTeamIdIsNotBiggerThan0()
        {
            CreateAllocationRequest request = GetValidCreateAllocationRequest();
            request.AllocatedTeamId = 0;

            var errors = ValidationHelper.ValidateModel(request);

            Assert.AreEqual(errors.Count, 1);
            Assert.IsTrue(errors.Any(x => x.ErrorMessage.Contains("Please enter a value bigger than 0")));
        }

        [Test]
        public void ValidationFailsIfCreatedByIsNotProvided()
        {
            CreateAllocationRequest request = GetValidCreateAllocationRequest();
            request.CreatedBy = null;
           
            var errors = ValidationHelper.ValidateModel(request);

            Assert.AreEqual(errors.Count, 1);
            Assert.IsTrue(errors.Any(x => x.ErrorMessage.Contains("CreatedBy field is required")));
        }
        #endregion
    }
}
