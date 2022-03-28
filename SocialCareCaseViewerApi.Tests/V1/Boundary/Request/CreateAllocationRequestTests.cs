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
            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);
            var badAllocationRequests = new List<(CreateAllocationRequest, string)>
            {
                (TestHelpers.CreateValidatorAllocationRequest(null, allocationStartDate: today), "Mosaic Id Required"),
                (TestHelpers.CreateValidatorAllocationRequest(0, allocationStartDate: today), "Mosaic Id must be greater than 1"),
                (TestHelpers.CreateValidatorAllocationRequest(workerId: 0, allocationStartDate: today), "Worker Id must be greater than 1"),
                (TestHelpers.CreateValidatorAllocationRequest(teamId: null, allocationStartDate: today), "Team Id Required"),
                (TestHelpers.CreateValidatorAllocationRequest(teamId: 0, allocationStartDate: today), "Team Id must be greater than 1"),
                (TestHelpers.CreateValidatorAllocationRequest(ragRating: "Blue", allocationStartDate: today), "RAG rating must be 'low', 'high', 'medium', 'urgent' or 'none'"),
                (TestHelpers.CreateValidatorAllocationRequest(createdBy: null, allocationStartDate: today), "Email Required"),
                (TestHelpers.CreateValidatorAllocationRequest(createdBy: "not_an_email", allocationStartDate: DateTime.Now), "Enter a valid email address"),
                (TestHelpers.CreateValidatorAllocationRequest(allocationStartDate: today.AddDays(1)), "Allocation start date must not be in future"),
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
        public void ValidCreateAllocationRequestReturnsNoErrorsOnValidation()
        {
            var createAllocationRequest = TestHelpers.CreateValidatorAllocationRequest(allocationStartDate: DateTime.Now);
            var validator = new CreateAllocationRequestValidator();

            var validationResponse = validator.Validate(createAllocationRequest);

            validationResponse.IsValid.Should().Be(true);
        }
    }
}
