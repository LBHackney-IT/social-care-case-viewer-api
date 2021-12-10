using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using SocialCareCaseViewerApi.Tests.V1.Helpers;
using SocialCareCaseViewerApi.V1.Boundary.Requests;

namespace SocialCareCaseViewerApi.Tests.V1.Boundary.Request
{
    [TestFixture]
    public class CreateReferralRequestTests
    {
        [Test]
        public void CreateReferralRequestValidationReturnsErrorsWhenAnyPropertyIsEmpty()
        {
            var badRequests = new List<(CreateReferralRequest, string)>()
            {
                (TestHelpers.CreateNewMashReferralRequest(""), "Referrer must have at least one character"),
                (TestHelpers.CreateNewMashReferralRequest(requestedSupport: ""), "Requested support must have at least one character"),
                (TestHelpers.CreateNewMashReferralRequest(referralUri: ""), "Referral document url must have at least one character"),
            };

            var validator = new CreateReferralRequestValidator();
            foreach (var (request, expectedErrorMessage) in badRequests)
            {
                var validationResponse = validator.Validate(request);

                validationResponse.IsValid.Should().Be(false);
                validationResponse.ToString().Should().Be(expectedErrorMessage);
            }
        }

        [Test]
        public void ValidCreateReferralRequestValidationReturnsNoErrorsOnValidation()
        {
            var createReferralRequest = TestHelpers.CreateNewMashReferralRequest();
            var validator = new CreateReferralRequestValidator();

            var validationResponse = validator.Validate(createReferralRequest);

            validationResponse.IsValid.Should().Be(true);
        }
    }
}
