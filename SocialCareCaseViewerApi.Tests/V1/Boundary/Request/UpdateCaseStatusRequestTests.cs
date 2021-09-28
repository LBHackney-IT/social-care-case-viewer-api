using System;
using Bogus;
using FluentAssertions;
using NUnit.Framework;
using SocialCareCaseViewerApi.Tests.V1.Helpers;
using SocialCareCaseViewerApi.V1.Boundary.Requests;

#nullable enable
namespace SocialCareCaseViewerApi.Tests.V1.Boundary.Request
{
    [TestFixture]
    public class UpdateCaseStatusRequestTests
    {
        private static readonly Faker _faker = new Faker();

        private static object[] _updateCaseStatusRequests =
        {
            new object?[] { TestHelpers.CreateUpdateCaseStatusRequest(), true, ""},
            new object?[] { TestHelpers.CreateUpdateCaseStatusRequest(DateTime.Now.AddDays(-1)), false, "'end_date' must be in the future"},
            new object?[] { TestHelpers.CreateUpdateCaseStatusRequest(email: "invalid-email-address"), false, "'edited_by' must be a valid email address"},
            new object?[] { TestHelpers.CreateUpdateCaseStatusRequest(notes: _faker.Random.String2(1001)), false, "'notes' must be less than or equal to 1,000 characters."},
        };

        [TestCaseSource(nameof(_updateCaseStatusRequests))]
        public void UpdateCaseStatusRequestValidation(UpdateCaseStatusRequest request, bool validRequest, string? errorMessage)
        {
            var validator = new UpdateCaseStatusValidator();
            var validationResponse = validator.Validate(request);

            if (validRequest)
            {
                validationResponse.IsValid.Should().BeTrue();
            }
            else
            {
                validationResponse.IsValid.Should().BeFalse();
                validationResponse.ToString().Should().Be(errorMessage);
            }
        }
    }
}
