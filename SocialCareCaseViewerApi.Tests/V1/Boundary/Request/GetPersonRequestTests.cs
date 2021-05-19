using System.Linq;
using Bogus;
using FluentAssertions;
using NUnit.Framework;
using SocialCareCaseViewerApi.Tests.V1.Helpers;
using SocialCareCaseViewerApi.V1.Boundary.Requests;

namespace SocialCareCaseViewerApi.Tests.V1.Boundary.Request
{
    [TestFixture]
    public class GetPersonRequestTests
    {
        private GetPersonRequest _request;
        private Faker _faker;

        [SetUp]
        public void SetUp()
        {
            _request = new GetPersonRequest();
            _faker = new Faker();
        }

        [Test]
        public void ValidationFailsIfIdIsNotProvided()
        {
            var errors = ValidationHelper.ValidateModel(_request);

            errors.Count.Should().Be(1);
            errors.First().ErrorMessage.Should().Be("Invalid id");
        }

        [Test]
        public void ValidationFailsIfIdIsNegative()
        {
            var errors = ValidationHelper.ValidateModel(_request);

            _request.Id = _faker.Random.Long(long.MinValue, 0);

            errors.Count.Should().Be(1);
            errors.First().ErrorMessage.Should().Be("Invalid id");
        }

        [Test]
        public void ValidationFailsIfIdIsZero()
        {
            var errors = ValidationHelper.ValidateModel(_request);

            _request.Id = 0;

            errors.Count.Should().Be(1);
            errors.First().ErrorMessage.Should().Be("Invalid id");
        }

        [Test]
        public void ValidationSucceesdIfValidIdIsProvided()
        {
            _request.Id = _faker.Random.Long(1, long.MaxValue);

            var errors = ValidationHelper.ValidateModel(_request);

            errors.Count.Should().Be(0);
        }
    }
}
