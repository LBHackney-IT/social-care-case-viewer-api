using System.Collections.Generic;
using NUnit.Framework;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.Tests.V1.Helpers;
using FluentAssertions;

namespace SocialCareCaseViewerApi.Tests.V1.Boundary.Request
{
    public class UpdateCaseSubmissionRequestTests
    {
        [Test]
        public void UpdateCaseSubmissionRequestValidationReturnsErrorsWithInvalidProperties()
        {
            var badUpdateCaseSubmissionRequest = new List<(UpdateCaseSubmissionRequest, string)>
            {
                (TestHelpers.UpdateCaseSubmissionRequest(updatedBy: "invalid email"), "Provide a valid email address for who is updating the submission")
            };

            var validator = new UpdateCaseSubmissionRequestValidator();

            foreach (var (request, expectedErrorMessage) in badUpdateCaseSubmissionRequest)
            {
                var validationResponse = validator.Validate(request);

                validationResponse.IsValid.Should().Be(false);
                validationResponse.ToString().Should().Be(expectedErrorMessage);
            }
        }

        [Test]
        public void ValidUpdateCaseSubmissionReturnsNoErrorsOnValidation()
        {
            var createTeamRequest = TestHelpers.UpdateCaseSubmissionRequest();
            var validator = new UpdateCaseSubmissionRequestValidator();

            var validationResponse = validator.Validate(createTeamRequest);

            validationResponse.IsValid.Should().Be(true);
        }
    }
}
