using System.Collections.Generic;
using FluentValidation;
using SocialCareCaseViewerApi.V1.Infrastructure;

namespace SocialCareCaseViewerApi.V1.Boundary.Requests
#nullable enable
{
    public class QueryCaseSubmissionsRequest
    {
        public string? FormId { get; set; }

        public IEnumerable<string>? SubmissionStates { get; set; }
    }

    public class QueryCaseSubmissionsValidator : AbstractValidator<QueryCaseSubmissionsRequest>
    {
        public QueryCaseSubmissionsValidator()
        {
            RuleFor(query => query)
                .Must(query => !string.IsNullOrEmpty(query.FormId) || query.SubmissionStates != null)
                .WithMessage("Must provide at least one query parameter");
        }
    }
}
