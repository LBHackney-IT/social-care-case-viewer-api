using System;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using SocialCareCaseViewerApi.Tests.V1.Helpers;
using SocialCareCaseViewerApi.V1.Boundary.Requests;

namespace SocialCareCaseViewerApi.Tests.V1.Boundary.Request
{
    [TestFixture]
    public class GetTeamsRequestTests
    {
        [Test]
        public void GetTeamsRequestValidationWithInvalidPropertiesReturnsError()
        {
            var badGetTeamsRequests = new List<(GetTeamsRequest, string)>()
            {
                (TestHelpers.CreateGetTeamsRequest(contextFlag: "d"), "Context flag must be 'A' or 'C'"),
                (TestHelpers.CreateGetTeamsRequest(contextFlag: ""), "Context flag must be 'A' or 'C'"),
                (TestHelpers.CreateGetTeamsRequest(contextFlag: "aa"), "Context flag must be 1 character in length"),
            };

            var validator = new GetTeamsRequestValidator();

            foreach (var (request, expectedErrorMessage) in badGetTeamsRequests)
            {
                var validationResponse = validator.Validate(request);

                validationResponse.IsValid.Should().Be(false);
                validationResponse.ToString().Should().Be(expectedErrorMessage);
            }
        }

        [Test]
        public void ValidGetTeamRequestReturnsNoErrorsOnValidation()
        {
            var createTeamRequest = TestHelpers.CreateGetTeamsRequest();
            var validator = new GetTeamsRequestValidator();

            var validationResponse = validator.Validate(createTeamRequest);

            validationResponse.IsValid.Should().Be(true);
        }
    }
}
