using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using FluentValidation;

namespace SocialCareCaseViewerApi.V1.Boundary.Requests
#nullable enable
{
    public class QueryCaseSubmissionsRequest
    {
        [JsonPropertyName("formId")]
        public string? FormId { get; set; }

        [JsonPropertyName("submissionStates")]
        public IEnumerable<string>? SubmissionStates { get; set; }

        [JsonPropertyName("createdAfter")]
        public DateTime? CreatedAfter { get; set; }

        [JsonPropertyName("createdBefore")]
        public DateTime? CreatedBefore { get; set; }

        [JsonPropertyName("includeFormAnswers")]
        public bool IncludeFormAnswers { get; set; } = false;

        [JsonPropertyName("includeEditHistory")]
        public bool IncludeEditHistory { get; set; } = false;

        [JsonPropertyName("page")]
        public int Page { get; set; } = 1;

        [JsonPropertyName("size")]
        public int Size { get; set; } = 100;

        [JsonPropertyName("ageContext")]
        public string? AgeContext { get; set; }
    }

    public class QueryCaseSubmissionsValidator : AbstractValidator<QueryCaseSubmissionsRequest>
    {
        public QueryCaseSubmissionsValidator()
        {
            RuleFor(query => query)
                .Must(query => !string.IsNullOrEmpty(query.FormId) ||
                               query.SubmissionStates != null ||
                               query.CreatedAfter != null ||
                               query.CreatedBefore != null ||
                               query.AgeContext != null)
                .WithMessage("Must provide at least one query parameter");
        }
    }
}
