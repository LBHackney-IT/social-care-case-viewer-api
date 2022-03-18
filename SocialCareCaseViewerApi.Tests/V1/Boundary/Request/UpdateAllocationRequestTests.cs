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
    public class UpdateAllocationRequestTests
    {
        [Test]
        public void UpdateAllocationValidationReturnsErrorsWithInvalidProperties()
        {
            var badAllocationRequests = new List<(CreateAllocationRequest, string)>
            {
                (TestHelpers.CreateValidatorAllocationRequest(0, allocationStartDate: DateTime.Now), "Mosaic Id must be grater than 1"),
                (TestHelpers.CreateValidatorAllocationRequest(workerId: 0, allocationStartDate: DateTime.Now), "Worker Id must be grater than 1"),
                (TestHelpers.CreateValidatorAllocationRequest(teamId: 0, allocationStartDate: DateTime.Now), "Team Id must be grater than 1")
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
