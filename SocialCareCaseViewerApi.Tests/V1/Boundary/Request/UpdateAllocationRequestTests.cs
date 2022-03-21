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
                (TestHelpers.UpdateValidatorAllocationRequest(null, deallocationDate: DateTime.Now, ragRating: null), "Id Required"),
                // (TestHelpers.UpdateValidatorAllocationRequest(0, deallocationDate: DateTime.Now, ragRating: null), "Id must be greater than 1"),
                // (TestHelpers.UpdateValidatorAllocationRequest(1, deallocationDate: DateTime.Now), "Please do not patch RagRating and deallocate at the same time"),
                // (TestHelpers.UpdateValidatorAllocationRequest(0, deallocationDate: DateTime.Now, ragRating: null, createdBy: null), "Id must be greater than 1"),


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

        // [Test]
        // public void ValidCreateAllocationRequestReturnsNoErrorsOnValidation()
        // {
        //     var createAllocationRequest = TestHelpers.CreateValidatorAllocationRequest(allocationStartDate: DateTime.Now);
        //     var validator = new CreateAllocationRequestValidator();
        //
        //     var validationResponse = validator.Validate(createAllocationRequest);
        //
        //     validationResponse.IsValid.Should().Be(true);
        // }
    }
}
