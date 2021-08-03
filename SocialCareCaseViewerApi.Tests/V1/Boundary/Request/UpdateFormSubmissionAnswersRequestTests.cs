using System;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using SocialCareCaseViewerApi.Tests.V1.Helpers;
using SocialCareCaseViewerApi.V1.Boundary.Requests;

namespace SocialCareCaseViewerApi.Tests.V1.Boundary.Request
{
    [TestFixture]
    public class UpdateFormSubmissionAnswersRequestTests
    {
        [Test]
        public void UpdateFormSubmissionAnswersRequestReturnsIsNotValidForInvalidRequest()
        {
            var badRequests = new List<(UpdateFormSubmissionAnswersRequest, string)>
            {
                (TestHelpers.CreateUpdateFormSubmissionAnswersRequest(editedBy: "invalid_email"), "Provide a valid email address for who edited the submission"),
                (TestHelpers.CreateUpdateFormSubmissionAnswersRequest(title: ""), "Title must have a length of at least 1")
            };

            var validator = new UpdateFormSubmissionAnswersValidator();

            foreach (var (request, errorMessage) in badRequests)
            {
                var validationResponse = validator.Validate(request);

                if (validationResponse == null)
                {
                    throw new NullReferenceException();
                }

                validationResponse.IsValid.Should().Be(false);
                validationResponse.ToString().Should().Be(errorMessage);
            }

        }

        [Test]
        public void UpdateWorkerReturnsIsValidForValidRequest()
        {
            var request = TestHelpers.CreateUpdateFormSubmissionAnswersRequest();
            var validator = new UpdateFormSubmissionAnswersValidator();

            var validationResponse = validator.Validate(request);

            validationResponse.IsValid.Should().Be(true);
        }
    }
}
