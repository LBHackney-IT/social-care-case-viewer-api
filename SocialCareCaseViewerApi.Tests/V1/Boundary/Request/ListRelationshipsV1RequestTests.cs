using Bogus;
using FluentAssertions;
using NUnit.Framework;
using SocialCareCaseViewerApi.Tests.V1.Helpers;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using System.Linq;

namespace SocialCareCaseViewerApi.Tests.V1.Boundary.Request
{
    [TestFixture]
    public class ListRelationshipsV1RequestTests
    {
        private Faker _faker;

        [SetUp]
        public void SetUp()
        {
            _faker = new Faker();
        }

        [Test]
        public void ValidationPassesWhenValidPersonIdIsProvided()
        {
            var request = new ListRelationshipsV1Request() { PersonId = _faker.Random.Long(1, long.MaxValue) };

            var errors = ValidationHelper.ValidateModel(request);

            errors.Count.Should().Be(0);
        }

        [Test]
        public void ValidationFailsWhenPersonIdIsNotProvided()
        {
            var request = new ListRelationshipsV1Request();

            var errors = ValidationHelper.ValidateModel(request);

            errors.Count.Should().Be(1);
            errors.Any(x => x.ErrorMessage.Contains("Please enter a valid personId")).Should().BeTrue();
        }
    }
}
