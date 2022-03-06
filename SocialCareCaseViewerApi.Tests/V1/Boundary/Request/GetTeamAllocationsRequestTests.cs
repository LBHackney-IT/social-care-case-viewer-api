using FluentAssertions;
using NUnit.Framework;
using SocialCareCaseViewerApi.Tests.V1.Helpers;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using System.Collections.Generic;

namespace SocialCareCaseViewerApi.Tests.V1.Boundary.Request
{
    [TestFixture]
    public class GetTeamAllocationsRequestTests
    {
        [Test]
        public void GetTeamAllocationsRequestValidationWithMissingOrInvalidViewReturnsError()
        {
            var invalidGetTeamAllocationRequests = new List<(GetTeamAllocationsRequest, string)>()
            {
                (TestHelpers.CreateGetTeamAllocationsRequest(view: ""), "View must be 'allocated' or 'unallocated'"),
                (TestHelpers.CreateGetTeamAllocationsRequest(view: "invalid"), "View must be 'allocated' or 'unallocated'"),
                (TestHelpers.CreateGetTeamAllocationsRequest(view: null), "Type of view must be provided"),
            };

            var validator = new GetTeamAllocationsRequestValidator();

            foreach (var (request, expectedErrorMessage) in invalidGetTeamAllocationRequests)
            {
                var validationResponse = validator.Validate(request);

                validationResponse.IsValid.Should().Be(false);
                validationResponse.ToString().Should().Be(expectedErrorMessage);
            }
        }

        [Test]
        public void ValidGetTeamRequestReturnsNoErrorsOnValidation()
        {
            var correctGetTeamAllocationRequests = new List<GetTeamAllocationsRequest>()
            {
                (TestHelpers.CreateGetTeamAllocationsRequest(view: "allocated")),
                (TestHelpers.CreateGetTeamAllocationsRequest(view: "unallocated")),
            };

            var validator = new GetTeamAllocationsRequestValidator();

            foreach (var request in correctGetTeamAllocationRequests)
            {
                var validationResponse = validator.Validate(request);

                validationResponse.IsValid.Should().Be(true);
            }
        }
    }
}
