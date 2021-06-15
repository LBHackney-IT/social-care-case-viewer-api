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
    public class CreateWorkerRequestTests
    {
        private Faker _faker;

        [SetUp]
        public void SetUp()
        {
            _faker = new Faker();
        }

        [Test]
        public void CreateWorkerValidationReturnsErrorsWithInvalidProperties()
        {
            const string longEmail = "thisEmailIsLongerThan62CharactersAndAlsoValid@HereIAmJustCreatingMoreCharacters.com";

            var badCreateWorkerRequests = new List<(CreateWorkerRequest, string)>
            {
                (TestHelpers.CreateWorkerRequest(email: ""), "Email address must be valid"),
                (TestHelpers.CreateWorkerRequest(email: longEmail), "Email address must be no longer than 62 characters"),
                (TestHelpers.CreateWorkerRequest(firstName: ""), "First name must be provided"),
                (TestHelpers.CreateWorkerRequest(firstName: _faker.Random.String2(101)), "First name must be no longer than 100 characters"),
                (TestHelpers.CreateWorkerRequest(lastName: ""), "Last name must be provided"),
                (TestHelpers.CreateWorkerRequest(lastName: _faker.Random.String2(101)), "Last name must be no longer than 100 characters"),
                (TestHelpers.CreateWorkerRequest(contextFlag: ""), $"Context flag must be provided{Environment.NewLine}Context flag must be 'A' or 'C'"),
                (TestHelpers.CreateWorkerRequest(contextFlag: "B"), "Context flag must be 'A' or 'C'"),
                (TestHelpers.CreateWorkerRequest(role: ""), "Role must be provided"),
                (TestHelpers.CreateWorkerRequest(role: _faker.Random.String2(201)), "Role provided is too long (more than 200 characters)"),
                (TestHelpers.CreateWorkerRequest(teamName: ""), "Team must be provided with a name"),
                (TestHelpers.CreateWorkerRequest(teamName: _faker.Random.String2(201)), "Team name must be no more than 200 characters"),
                (TestHelpers.CreateWorkerRequest(teamId: 0), "Team ID must be greater than 0"),
                (TestHelpers.CreateWorkerRequest(createATeam: false), "A team must be provided"),
                (TestHelpers.CreateWorkerRequest(createdByEmail: ""), "Created by email address must be valid"),
                (TestHelpers.CreateWorkerRequest(createdByEmail: longEmail), "Created by email address must be no longer than 62 characters")

            };

            var validator = new CreateWorkerRequestValidator();

            foreach (var (request, expectedErrorMessage) in badCreateWorkerRequests)
            {
                var validationResponse = validator.Validate(request);

                if (validationResponse == null)
                {
                    throw new NullReferenceException();
                }

                validationResponse.IsValid.Should().Be(false);
                validationResponse.ToString().Should().Be(expectedErrorMessage);
            }
        }

        [Test]
        public void ValidCreateWorkerRequestReturnsNoErrorsOnValidation()
        {
            var createWorkerRequest = TestHelpers.CreateWorkerRequest();
            var validator = new CreateWorkerRequestValidator();

            var validationResponse = validator.Validate(createWorkerRequest);

            validationResponse.IsValid.Should().Be(true);
        }
    }
}
