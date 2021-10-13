using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using FluentValidation;

#nullable enable
namespace SocialCareCaseViewerApi.V1.Boundary.Requests
{
    public class UpdateCaseStatusRequest
    {
        [JsonPropertyName("caseStatusId")]
        public long CaseStatusId { get; set; }

        [JsonPropertyName("startDate")]
        public DateTime? StartDate { get; set; }

        [JsonPropertyName("endDate")]
        public DateTime? EndDate { get; set; }

        [JsonPropertyName("editedBy")]
        public string EditedBy { get; set; } = null!;

        [JsonPropertyName("notes")]
        public string? Notes { get; set; }

        [JsonPropertyName("answers")]
        public List<CaseStatusValue>? Answers { get; set; }
    }

    public class CaseStatusValue
    {
        [JsonPropertyName("option")]
        public string Option { get; set; } = null!;

        [JsonPropertyName("value")]
        public string Value { get; set; } = null!;
    }

    public class UpdateCaseStatusValidator : AbstractValidator<UpdateCaseStatusRequest>
    {
        public UpdateCaseStatusValidator()
        {
            RuleFor(x => x.EditedBy)
                .NotNull().WithMessage("'editedBy' must be provided")
                .EmailAddress().WithMessage("'editedBy' must be a valid email address");
            RuleFor(x => x.Notes)
                .MaximumLength(1000).When(x => x.Notes != null).WithMessage("'notes' must be less than or equal to 1,000 characters.");
            RuleForEach(x => x.Answers)
                .ChildRules(value =>
                {
                    value.RuleFor(x => x.Option).NotNull().WithMessage("Answer must have value for option");
                    value.RuleFor(x => x.Value).NotNull().WithMessage("Answer must have value");
                });
        }
    }
}
