using FluentAssertions;
using NUnit.Framework;
using SocialCareCaseViewerApi.Tests.V1.Helpers;
using SocialCareCaseViewerApi.V1.Domain;

namespace SocialCareCaseViewerApi.Tests.V1.Domain
{
    [TestFixture]
    public class AddressDomainTests
    {
        [Test]
        public void AddressDomainIncludesRequiredProperties()
        {
            var address = new AddressDomain
            {
                Address = "Address11",
                Uprn = 123,
                Postcode = "AB1 2BC"
            };

            address.Address.Should().Be("Address11");
            address.Uprn.Should().Be(123);
            address.Postcode.Should().Be("AB1 2BC");
        }

        [Test]
        public void ValidationFailsIfAddressIsNotProvided()
        {
            AddressDomain address = new AddressDomain()
            {
                Uprn = 123,
                Postcode = "AB1"
            };

            var errors = ValidationHelper.ValidateModel(address);

            Assert.AreEqual(1, errors.Count);
        }
    }
}
