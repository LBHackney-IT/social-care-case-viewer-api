using System;
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
    public class GetTeamsRequestTests
    {
        private Faker _faker;

        [SetUp]
        public void SetUp()
        {
            _faker = new Faker();
        }

        [Test]
        public void GetTeamsRequestValidationWithInvalidPropertiesReturnsError()
        {
            var badGetTeamsRequests = new List<(GetTeamsRequest, string)>()
            {
                (TestHelpers.CreateGetTeamsRequest(true), "Must provide either a team ID, team name or context flag"),
                (TestHelpers.CreateGetTeamsRequest(contextFlag: "d"), "Context flag must be 'A' or 'C'"),
                (TestHelpers.CreateGetTeamsRequest(contextFlag: ""), "Context flag must be 'A' or 'C'"),
                (TestHelpers.CreateGetTeamsRequest(contextFlag: "aa"), "Context flag must be 1 character in length"),
                (TestHelpers.CreateGetTeamsRequest(name: ""), "Team name must be at least 1 character"),
                (TestHelpers.CreateGetTeamsRequest(name: _faker.Random.String2(201)), "Team name has a maximum length of 200 characters")
            };

            var validator = new GetTeamsRequestValidator();

            foreach (var (request, expectedErrorMessage) in badGetTeamsRequests)
            {
                var validationResponse = validator.Validate(request);

                if (validationResponse == null)
                {
                    throw new NullReferenceException();
                }

                validationResponse.Should().NotBeNull();
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

            if (validationResponse == null)
            {
                throw new NullReferenceException();
            }

            validationResponse.Should().NotBeNull();
            validationResponse.IsValid.Should().Be(true);
        }
    }
}
