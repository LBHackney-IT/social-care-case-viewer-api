using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using SocialCareCaseViewerApi.Tests.V1.Helpers;
using SocialCareCaseViewerApi.V1.Boundary.Requests;

namespace SocialCareCaseViewerApi.Tests.V1.Boundary.Request
{
    public class CreateCaseSubmissionRequestTests
    {
        [Test]
        public void CreateCaseSubmissionRequestValidationReturnsErrorsWithInvalidProperties()
        {
            var badCreateCaseSubmissionRequest = new List<(CreateCaseSubmissionRequest, string)>
            {
                (TestHelpers.CreateCaseSubmissionRequest(createdBy: "invalid email"), "Provide a valid email address for who created the submission"),
            };

            var validator = new CreateCaseSubmissionRequestValidator();

            foreach (var (request, expectedErrorMessage) in badCreateCaseSubmissionRequest)
            {
                var validationResponse = validator.Validate(request);

                validationResponse.Should().NotBeNull();
                validationResponse.IsValid.Should().Be(false);
                validationResponse.ToString().Should().Be(expectedErrorMessage);
            }
        }

        [Test]
        public void ValidCreateCaseSubmissionRequestReturnsNoErrorsOnValidation()
        {
            var createTeamRequest = TestHelpers.CreateCaseSubmissionRequest();
            var validator = new CreateCaseSubmissionRequestValidator();

            var validationResponse = validator.Validate(createTeamRequest);

            validationResponse.Should().NotBeNull();
            validationResponse.IsValid.Should().Be(true);
        }
    }
}
