using System;
using System.Collections.Generic;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

namespace SocialCareCaseViewerApi.V1.Boundary.Requests
#nullable enable
{
    public class QueryCaseSubmissionsRequest
    {
        [FromQuery(Name = "formId")]
        public string? FormId { get; set; }

        [FromQuery(Name = "submissionStates")]
        public IEnumerable<string>? SubmissionStates { get; set; }

        [FromQuery(Name = "createdAfter")]
        public DateTime? CreatedAfter { get; set; }

        [FromQuery(Name = "createdBefore")]
        public DateTime? CreatedBefore { get; set; }

        [FromQuery(Name = "includeFormAnswers")]
        public bool IncludeFormAnswers { get; set; } = false;

        [FromQuery(Name = "includeEditHistory")]
        public bool IncludeEditHistory { get; set; } = false;

        [FromQuery(Name = "page")]
        public int Page { get; set; } = 1;

        [FromQuery(Name = "size")]
        public int Size { get; set; } = 100;

        [FromQuery(Name = "ageContext")]
        public string? AgeContext { get; set; }

        [FromQuery(Name = "workerEmail")]
        public string? WorkerEmail { get; set; }

        [FromQuery(Name = "personId")]
        public long? PersonID { get; set; }

        [FromQuery(Name = "pruneUnfinished")]
        public bool PruneUnfinished { get; set; } = false;

        [FromQuery(Name = "includeDeletedRecords")]
        public bool IncludeDeletedRecords { get; set; } = false;

        [FromQuery(Name = "includeDeletedRecordsCount")]
        public bool IncludeDeletedRecordsCount { get; set; } = false;
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
                               query.AgeContext != null ||
                               query.WorkerEmail != null ||
                               query.PersonID != null)
                .WithMessage("Must provide at least one query parameter");
        }
    }
}
