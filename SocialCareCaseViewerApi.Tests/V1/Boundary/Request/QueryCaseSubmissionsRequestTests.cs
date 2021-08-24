using System;
using System.Collections.Generic;
using FluentAssertions;
using NUnit.Framework;
using SocialCareCaseViewerApi.Tests.V1.Helpers;
using SocialCareCaseViewerApi.V1.Boundary.Requests;

#nullable enable
namespace SocialCareCaseViewerApi.Tests.V1.Boundary.Request
{
    [TestFixture]
    public class QueryCaseSubmissionsRequestTests
    {
        private static object[] _queryCaseSubmissionRequests =
        {
            new object?[] { TestHelpers.CreateQueryCaseSubmissions(), false, "Must provide at least one query parameter"},
            new object?[] { TestHelpers.CreateQueryCaseSubmissions(formId: "form-id"), true, null},
            new object?[] { TestHelpers.CreateQueryCaseSubmissions(submissionStates: new List<string>{"submission-state"}), true, null},
            new object?[] { TestHelpers.CreateQueryCaseSubmissions(createdBefore: DateTime.Now), true, null},
            new object?[] { TestHelpers.CreateQueryCaseSubmissions(createdAfter: DateTime.Now), true, null},
            new object?[] { TestHelpers.CreateQueryCaseSubmissions(workerEmail: "example@hackney.gov.uk"), true, null},
            new object?[] { TestHelpers.CreateQueryCaseSubmissions(personID: 2), true, null},
            new object?[] { TestHelpers.CreateQueryCaseSubmissions(workerEmail: "example@hackney.gov.uk"), true, null}
        };

        [TestCaseSource(nameof(_queryCaseSubmissionRequests))]
        public void UpdateCaseSubmissionRequestValidation(QueryCaseSubmissionsRequest request, bool validRequest, string? errorMessage)
        {
            var validator = new QueryCaseSubmissionsValidator();
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
