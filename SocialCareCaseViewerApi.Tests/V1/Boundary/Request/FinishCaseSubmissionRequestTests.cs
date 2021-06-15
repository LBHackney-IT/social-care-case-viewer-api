using System.Collections.Generic;
using NUnit.Framework;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.Tests.V1.Helpers;
using FluentAssertions;

namespace SocialCareCaseViewerApi.Tests.V1.Boundary.Request
{
    public class FinishCaseSubmissionRequestTests
    {
        [Test]
        public void FinishCaseSubmissionRequestValidationReturnsErrorsWithInvalidProperties()
        {
            var badFinishCaseSubmissionRequest = new List<(FinishCaseSubmissionRequest, string)>
            {
                (TestHelpers.FinishCaseSubmissionRequest(createdBy: "invalid email"), "Provide a valid email address for who is finishing the submission")
            };

            var validator = new FinishCaseSubmissionRequestValidator();

            foreach (var (request, expectedErrorMessage) in badFinishCaseSubmissionRequest)
            {
                var validationResponse = validator.Validate(request);

                validationResponse.IsValid.Should().Be(false);
                validationResponse.ToString().Should().Be(expectedErrorMessage);
            }
        }

        [Test]
        public void ValidFinishCaseSubmissionReturnsNoErrorsOnValidation()
        {
            var createTeamRequest = TestHelpers.FinishCaseSubmissionRequest();
            var validator = new FinishCaseSubmissionRequestValidator();

            var validationResponse = validator.Validate(createTeamRequest);

            validationResponse.IsValid.Should().Be(true);
        }
    }
}
