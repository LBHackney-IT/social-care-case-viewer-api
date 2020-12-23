using FluentAssertions;
using NUnit.Framework;
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
    }
}