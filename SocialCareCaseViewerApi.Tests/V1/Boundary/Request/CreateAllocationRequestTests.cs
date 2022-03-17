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
    public class CreateAllocationRequestTests
    {
        [Test]
        public void CreateAllocationValidationReturnsErrorsWithInvalidProperties()
        {
            var badAllocationRequests = new List<(CreateAllocationRequest, string)>
            {
                (TestHelpers.CreateValidatorAllocationRequest(null, allocationStartDate: DateTime.Now), "Mosaic Id Required"),
                (TestHelpers.CreateValidatorAllocationRequest(0, allocationStartDate: DateTime.Now), "Mosaic Id must be grater than 1"),
                (TestHelpers.CreateValidatorAllocationRequest(workerId: 0, allocationStartDate: DateTime.Now), "Worker Id must be grater than 1"),
                (TestHelpers.CreateValidatorAllocationRequest(teamId: null, allocationStartDate: DateTime.Now), "Team Id Required"),
                (TestHelpers.CreateValidatorAllocationRequest(teamId: 0, allocationStartDate: DateTime.Now), "Team Id must be grater than 1"),
                (TestHelpers.CreateValidatorAllocationRequest(ragRating: null, allocationStartDate: DateTime.Now), "RagRating is Required"),
                (TestHelpers.CreateValidatorAllocationRequest(createdBy: null, allocationStartDate: DateTime.Now), "Email Required"),
                (TestHelpers.CreateValidatorAllocationRequest(createdBy: "not_an_email", allocationStartDate: DateTime.Now), "Enter a valid email address"),
                (TestHelpers.CreateValidatorAllocationRequest(), "Allocation start date required"),
            };

            var validator = new CreateAllocationRequestValidator();

            foreach (var (request, expectedErrorMessage) in badAllocationRequests)
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
        public void ValidCreateTeamRequestReturnsNoErrorsOnValidation()
        {
            var createAllocationRequest = TestHelpers.CreateValidatorAllocationRequest(allocationStartDate: DateTime.Now);
            var validator = new CreateAllocationRequestValidator();

            var validationResponse = validator.Validate(createAllocationRequest);

            validationResponse.IsValid.Should().Be(true);
        }
    }
}
