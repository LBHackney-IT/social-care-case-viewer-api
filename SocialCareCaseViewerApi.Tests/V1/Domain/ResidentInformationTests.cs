using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using SocialCareCaseViewerApi.V1.Domain;

namespace SocialCareCaseViewerApi.Tests.V1.Domain
{
    [TestFixture]
    public class ResidentInformationTests
    {
        [Test]
        public void ResidentInformationIncludesRequiredProperties()
        {
            var address = new Address
            {
                AddressLine1 = "Address Line 1",
                AddressLine2 = "Address Line 2",
                AddressLine3 = "Address Line 3",
                PostCode = "AB1 2BC",
            };

            var residentInformation = new ResidentInformation
            {
                PersonId = "Id",
                Title = "Mx",
                FirstName = "First",
                LastName = "Last",
                DateOfBirth = "1980-10-02",
                Gender = "x",
                Nationality = "British",
                AddressList = new List<Address> { address },
                NhsNumber = "123",
            };

            residentInformation.PersonId.Should().Be("Id");
            residentInformation.Title.Should().Be("Mx");
            residentInformation.FirstName.Should().Be("First");
            residentInformation.LastName.Should().Be("Last");
            residentInformation.DateOfBirth.Should().Be("1980-10-02");
            residentInformation.Gender.Should().Be("x");
            residentInformation.Nationality.Should().Be("British");
            residentInformation.AddressList.Should().Contain(address);
            residentInformation.NhsNumber.Should().Be("123");
        }

    }
}