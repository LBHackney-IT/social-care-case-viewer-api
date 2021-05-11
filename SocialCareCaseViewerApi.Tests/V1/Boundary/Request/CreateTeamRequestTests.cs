using System;
using System.Collections.Generic;
using Bogus;
using FluentAssertions;
using NUnit.Framework;
using SocialCareCaseViewerApi.Tests.V1.Helpers;
using SocialCareCaseViewerApi.V1.Boundary.Requests;

namespace SocialCareCaseViewerApi.Tests.V1.Boundary.Request
{
    [TestFixture]
    public class CreateTeamRequestTests
    {
        private Faker _faker;

        [SetUp]
        public void SetUp()
        {
            _faker = new Faker();
        }

        [Test]
        public void CreateTeamValidationReturnsErrorsWithInvalidProperties()
        {
            var badCreateTeamRequests = new List<(CreateTeamRequest, string)>
            {
                (TestHelpers.CreateTeamRequest(name: ""), "Team name must be provided"),
                (TestHelpers.CreateTeamRequest(name: _faker.Random.String2(201)), "Team name has a maximum length of 200 characters"),
                (TestHelpers.CreateTeamRequest(context: ""), "Context flag must be provided\nContext flag must be 'A' or 'C'"),
                (TestHelpers.CreateTeamRequest(context: "B"), "Context flag must be 'A' or 'C'"),
            };

            var validator = new CreateTeamRequestValidator();

            foreach (var (request, expectedErrorMessage) in badCreateTeamRequests)
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
        public void ValidCreateTeamRequestReturnsNoErrorsOnValidation()
        {
            var createTeamRequest = TestHelpers.CreateTeamRequest();
            var validator = new CreateTeamRequestValidator();

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
