using System.Collections.Generic;
using System.Linq;
using Bogus;
using FluentAssertions;
using NUnit.Framework;
using SocialCareCaseViewerApi.Tests.V1.Helpers;
using SocialCareCaseViewerApi.V1.Boundary.Requests;

namespace SocialCareCaseViewerApi.Tests.V1.Boundary.Request
{
    [TestFixture]
    public class CreateReferralRequestTests
    {
        private Faker _faker;

        [SetUp]
        public void SetUp()
        {
            _faker = new Faker();
        }

        [Test]
        public void CreateReferralRequestValidationReturnsErrorsWhenAnyPropertyIsNull()
        {
            var badRequests = new List<CreateReferralRequest>
            {
                new CreateReferralRequest { Referrer = null, ReferralUri = "notNull", RequestedSupport = "notNull", Clients = new List<string>{"notNull"} },
                new CreateReferralRequest { Referrer = "notNull", ReferralUri = null, RequestedSupport = "notNull", Clients = new List<string>{"notNull"} },
                new CreateReferralRequest { Referrer = "notNull", ReferralUri = "notNull", RequestedSupport = null, Clients = new List<string>{"notNull"} },
                new CreateReferralRequest { Referrer = "notNull", ReferralUri = "notNull", RequestedSupport = "notNull", Clients = null }
            };

            var validator = new CreateReferralRequestValidator();

            foreach (var validationResponse in badRequests.Select(request => validator.Validate(request)))
            {
                validationResponse.IsValid.Should().Be(false);
            }
        }

        [Test]
        public void CreateReferralRequestValidationReturnsErrorsWhenAnyPropertyIsEmpty()
        {
            var badRequests = new List<(CreateReferralRequest, string)>()
            {
                (TestHelpers.CreateNewMashReferralRequest(""), "Referrer must have at least one character"),
                (TestHelpers.CreateNewMashReferralRequest(requestedSupport: ""), "Requested support must have at least one character"),
                (TestHelpers.CreateNewMashReferralRequest(referralUri: ""), "Referral document url must have at least one character"),
                (TestHelpers.CreateNewMashReferralRequest(clients: new List<string> {string.Empty}), "List of referred clients can not contain empty strings")
            };

            var validator = new CreateReferralRequestValidator();
            foreach (var (request, expectedErrorMessage) in badRequests)
            {
                var validationResponse = validator.Validate(request);

                validationResponse.IsValid.Should().Be(false);
                validationResponse.ToString().Should().Be(expectedErrorMessage);
            }
        }
    }
}
