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
            var badAllocationRequests = new List<(UpdateAllocationRequest, string)>
            {
                (TestHelpers.UpdateValidatorAllocationRequest(0), "Id must be grater than 1"),
                (TestHelpers.UpdateValidatorAllocationRequest(workerId: 0), "Worker Id must be grater than 1"),
                (TestHelpers.UpdateValidatorAllocationRequest(deallocationReason: ""), "Deallocation reason required"),
                (TestHelpers.UpdateValidatorAllocationRequest(createdBy: null), "Email required"),
                (TestHelpers.UpdateValidatorAllocationRequest(createdBy: "not_an_email"), "Provide a valid email"),
                (TestHelpers.UpdateValidatorAllocationRequest(deallocationDate: DateTime.Now.AddDays(1)), "Deallocation date must be in the past"),
            };

            var validator = new UpdateAllocationRequestValidator();

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
            var updateAllocationRequest = TestHelpers.UpdateValidatorAllocationRequest(allocationId: 1);
            var validator = new UpdateAllocationRequestValidator();

            var validationResponse = validator.Validate(updateAllocationRequest);

            validationResponse.IsValid.Should().Be(true);
        }
    }
}
