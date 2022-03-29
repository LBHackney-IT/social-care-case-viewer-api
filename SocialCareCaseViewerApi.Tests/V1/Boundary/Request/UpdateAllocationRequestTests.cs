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
            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);
            var badAllocationRequests = new List<(UpdateAllocationRequest, string)>
            {
                (TestHelpers.UpdateValidatorAllocationRequest(null, deallocationDate: today, ragRating: null), "Id Required"),
                (TestHelpers.UpdateValidatorAllocationRequest(0, deallocationDate: today, ragRating: null), "Id must be greater than 1"),
                (TestHelpers.UpdateValidatorAllocationRequest(1, deallocationReason: null, deallocationDate: today, ragRating: null), "Deallocation reason required"),
                (TestHelpers.UpdateValidatorAllocationRequest(1, deallocationReason: "", deallocationDate: today, ragRating: null), "Deallocation reason required"),
                (TestHelpers.UpdateValidatorAllocationRequest(1, deallocationDate: today, deallocationScope: null, ragRating: null), "Deallocation Scope required"),
                (TestHelpers.UpdateValidatorAllocationRequest(1, deallocationDate: today, deallocationScope: "foo", ragRating: null), "Deallocation Scope must be either 'team' or 'worker'"),
                (TestHelpers.UpdateValidatorAllocationRequest(1, deallocationDate: null, ragRating: null), "Deallocation Date is required"),
                (TestHelpers.UpdateValidatorAllocationRequest(1, deallocationDate: tomorrow, ragRating: null), "DeallocationDate start date must not be in future"),
                (TestHelpers.UpdateValidatorAllocationRequest(1, deallocationDate: today, ragRating: null, createdBy: null), "Email required"),
                (TestHelpers.UpdateValidatorAllocationRequest(1, deallocationDate: today, ragRating: null, createdBy: "not an email"), "Provide a valid email"),
                (TestHelpers.UpdateValidatorAllocationRequest(1, deallocationDate: null, deallocationReason: null, ragRating: "blue"), "RAG rating must be 'low', 'high', 'medium', 'urgent' or 'none'"),
                (TestHelpers.UpdateValidatorAllocationRequest(1, deallocationDate: today), "Please do not patch RagRating and deallocate at the same time"),
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
        public void ValidDeAllocationRequestReturnsNoErrorsOnValidation()
        {
            var updateAllocationRequest = TestHelpers.UpdateValidatorAllocationRequest(1, deallocationDate: DateTime.Now, ragRating: null);
            var validator = new UpdateAllocationRequestValidator();

            var validationResponse = validator.Validate(updateAllocationRequest);

            validationResponse.IsValid.Should().Be(true);
        }

        [Test]
        public void ValidPatchAllocationRequestReturnsNoErrorsOnValidation()
        {
            var updateAllocationRequest = TestHelpers.UpdateValidatorAllocationRequest(1, deallocationDate: null, deallocationReason: null, ragRating: "urgent");
            var validator = new UpdateAllocationRequestValidator();

            var validationResponse = validator.Validate(updateAllocationRequest);

            validationResponse.IsValid.Should().Be(true);
        }
    }
}
