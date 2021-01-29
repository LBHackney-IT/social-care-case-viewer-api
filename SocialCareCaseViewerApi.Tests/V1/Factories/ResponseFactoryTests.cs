using System;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using SocialCareCaseViewerApi.V1.Boundary.Response;
using SocialCareCaseViewerApi.V1.Factories;
using SocialCareCaseViewerApi.V1.Infrastructure;
using Address = SocialCareCaseViewerApi.V1.Infrastructure.Address;

namespace SocialCareCaseViewerApi.Tests.V1.Factories
{
    public class ResponseFactoryTests
    {
        [Test]
        public void CanMapResidentAndAddressFromDomainToResponse()
        {
            var testDateOfBirth = DateTime.Now;

            Person person = new Person
            {
                Id = 123,
                Title = "Mx",
                FirstName = "Ciascom",
                LastName = "Tesselate",
                Gender = "x",
                Nationality = "British",
                NhsNumber = 456,
                DateOfBirth = testDateOfBirth,
                AgeContext = "b"
            };

            Address newAddress = new Address() { AddressId = 345 };

            List<PersonOtherName> names = new List<PersonOtherName>
            {
                new PersonOtherName() { Id = 1 },
                new PersonOtherName() { Id = 2 }
            };

            List<PhoneNumber> phoneNumbers = new List<PhoneNumber>()
            {
                new PhoneNumber() { Id = 1 },
                new PhoneNumber() { Id = 2 },
            };

            var expectedResponse = new AddNewResidentResponse
            {
                PersonId = 123,
                AddressId = newAddress.AddressId,
                OtherNameIds = new List<int>() { 1, 2 },
                PhoneNumberIds = new List<int> { 1, 2 }

            };

            person.ToResponse(newAddress.AddressId, names, phoneNumbers).Should().BeEquivalentTo(expectedResponse);
        }
    }
}
