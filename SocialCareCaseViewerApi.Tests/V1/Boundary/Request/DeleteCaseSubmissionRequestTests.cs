using FluentAssertions;
using NUnit.Framework;
using SocialCareCaseViewerApi.Tests.V1.Helpers;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using System.Collections.Generic;
using System.Linq;

namespace SocialCareCaseViewerApi.Tests.V1.Boundary.Request
{
    [TestFixture]
    public class DeleteCaseSubmissionRequestTests
    {
        [Test]
        public void DeleteCaseSubmissionRequestValidationReturnsErrorsWithInvalidProperties()
        {
            var badDeleteCaseSubmissionRequest = new List<(DeleteCaseSubmissionRequest, string)>
            {
                (TestHelpers.DeleteCaseSubmissionRequest(deletedBy: "invalid email"), "Provide a valid email address for who is deleting the submission"),
                (TestHelpers.DeleteCaseSubmissionRequest(deleteRequestedBy: " "), "Provide valid delete requested by name"),
                (TestHelpers.DeleteCaseSubmissionRequest(deletedReason: " "), "Provide valid delete reason"),
            };

            var validator = new DeleteCaseSubmissionRequestValidator();

            foreach (var (request, expectedErrorMessage) in badDeleteCaseSubmissionRequest)
            {
                var validationResponse = validator.Validate(request);

                validationResponse.IsValid.Should().Be(false);
                validationResponse.ToString().Should().Be(expectedErrorMessage);
            }
        }

        [Test]
        public void DeleteCaseSubmissionRequestValidationReturnsErrorWithMessageWhenDeletedByEmailIsNull()
        {
            var updateRequestWithoutDeletedByEmail = TestHelpers.DeleteCaseSubmissionRequest();
            updateRequestWithoutDeletedByEmail.DeletedBy = null;

            var validator = new DeleteCaseSubmissionRequestValidator();

            var validationResponse = validator.Validate(updateRequestWithoutDeletedByEmail);

            validationResponse.IsValid.Should().Be(false);

            validationResponse.Errors.Any(x => x.ErrorMessage.Contains("Provide who is deleting the submission")).Should().BeTrue();
        }

        [Test]
        public void ValidUpdateCaseSubmissionReturnsNoErrorsOnValidation()
        {
            var deleteCaseSubmissionRequest = TestHelpers.DeleteCaseSubmissionRequest();
            var validator = new DeleteCaseSubmissionRequestValidator();

            var validationResponse = validator.Validate(deleteCaseSubmissionRequest);

            validationResponse.IsValid.Should().Be(true);
        }
    }
}
