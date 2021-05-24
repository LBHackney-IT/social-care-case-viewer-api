using Bogus;
using NUnit.Framework;
using SocialCareCaseViewerApi.Tests.V1.Helpers;
using SocialCareCaseViewerApi.V1.Boundary.Requests;

namespace SocialCareCaseViewerApi.Tests.V1.Boundary.Request
{
    [TestFixture]
    public class ListVisitsRequestTests
    {
        private ListVisitsRequest _request;
        private Faker _faker;

        [SetUp]
        public void SetUp()
        {
            _request = new ListVisitsRequest();
            _faker = new Faker();
        }

        [Test]
        public void ValidationFailsIfPersonIdIsNotProvided()
        {
            var errors = ValidationHelper.ValidateModel(_request);

            Assert.AreEqual(1, errors.Count);
        }

        [Test]
        public void ValidationSucceeedsWhenValidPersonIdIsProvided()
        {
            _request.Id = _faker.Random.String();

            var errors = ValidationHelper.ValidateModel(_request);

            Assert.AreEqual(0, errors.Count);
        }
    }
}
