using SocialCareCaseViewerApi.V1.Boundary.Response;
using SocialCareCaseViewerApi.V1.Domain;
using NUnit.Framework;
using SocialCareCaseViewerApi.V1.Infrastructure;
using System;
using SocialCareCaseViewerApi.V1.Factories;
using FluentAssertions;

namespace SocialCareCaseViewerApi.Tests.V1.Factories
{
    public class ResponseFactoryTests
    {
        [Test]
        public void CanMapResidentAndAddressFromDomainToResponse()
        {
            var testDateOfBirth = DateTime.Now;
            var domain = new Person
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

            var domainAddress = new AddressDomain
            {
                Address = "Address11"
            };

            var expectedResponse = new AddNewResidentResponse
            {
                PersonId = 123,
                Title = "Mx",
                FirstName = "Ciascom",
                LastName = "Tesselate",
                Gender = "x",
                Nationality = "British",
                NhsNumber = 456,
                DateOfBirth = testDateOfBirth,
                AgeGroup = "b",
                Address = new AddressDomain
                {
                    Address = "Address11"
                }
            };

            domain.ToResponse(domainAddress).Should().BeEquivalentTo(expectedResponse);
        }
    }
}