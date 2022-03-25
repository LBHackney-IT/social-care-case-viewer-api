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
    public class UpdateWorkerRequestTests
    {
        private Faker _faker;

        [SetUp]
        public void SetUp()
        {
            _faker = new Faker();
        }

        [Test]
        public void UpdateWorkerReturnsIsNotValidForInvalidRequest()
        {
            const string longEmail = "thisEmailIsLongerThan62CharactersAndAlsoValid@HereIAmJustCreatingMoreCharacters.com";

            var teams = new List<WorkerTeamRequest>() {
                new WorkerTeamRequest() { Id = 1, Name = "Team 1" },
                new WorkerTeamRequest() { Id = 2, Name = "Team 2" }
            };

            var badRequests = new List<(UpdateWorkerRequest, string)>
            {
                (TestHelpers.CreateUpdateWorkersRequest(workerId: 0), "Worker id must be greater than 0"),
                (TestHelpers.CreateUpdateWorkersRequest(modifiedBy: ""), "Created by email address must be valid"),
                (TestHelpers.CreateUpdateWorkersRequest(modifiedBy: longEmail), "Created by email address must be no longer than 62 characters"),
                (TestHelpers.CreateUpdateWorkersRequest(firstName: ""), "First name must be provided"),
                (TestHelpers.CreateUpdateWorkersRequest(firstName: _faker.Random.String2(101)), "First name must be no longer than 100 characters"),
                (TestHelpers.CreateUpdateWorkersRequest(lastName: ""), "Last name must be provided"),
                (TestHelpers.CreateUpdateWorkersRequest(lastName: _faker.Random.String2(101)), "Last name must be no longer than 100 characters"),
                (TestHelpers.CreateUpdateWorkersRequest(contextFlag: ""), $"Context flag must be provided{Environment.NewLine}Context flag must be 'A' or 'C'"),
                (TestHelpers.CreateUpdateWorkersRequest(contextFlag: _faker.Random.String2(2, "AC")), "Context flag must be no longer than 1 character"),
                (TestHelpers.CreateUpdateWorkersRequest(contextFlag: "B"), "Context flag must be 'A' or 'C'"),
                (TestHelpers.CreateUpdateWorkersRequest(role: _faker.Random.String2(201)), "Role provided is too long (more than 200 characters)"),
                (TestHelpers.CreateUpdateWorkersRequest(dateStart: DateTime.Now.AddMinutes(1)), "Date cannot be set in the future"),
                (TestHelpers.CreateUpdateWorkersRequest(teamName: _faker.Random.String2(201)), "Team name must be no more than 200 characters"),
                (TestHelpers.CreateUpdateWorkersRequest(teamId: 0), "Team ID must be greater than 0"),
                (TestHelpers.CreateUpdateWorkersRequest(createATeam: false, providedTeams: teams), "Please provide only one team")
            };

            var validator = new UpdateWorkerRequestValidator();

            foreach (var (request, errorMessage) in badRequests)
            {
                var validationResponse = validator.Validate(request);

                validationResponse.IsValid.Should().Be(false);
                validationResponse.ToString().Should().Be(errorMessage);
            }

        }

        [Test]
        public void UpdateWorkerReturnsIsValidForValidRequest()
        {
            var request = TestHelpers.CreateUpdateWorkersRequest();
            var validator = new UpdateWorkerRequestValidator();

            var validationResponse = validator.Validate(request);

            validationResponse.IsValid.Should().Be(true);
        }
    }
}
