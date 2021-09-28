using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using FluentValidation;

#nullable enable
namespace SocialCareCaseViewerApi.V1.Boundary.Requests
{
    public class UpdateCaseStatusRequest
    {
        [JsonPropertyName("personId")]
        public long PersonId { get; set; }

        [JsonPropertyName("endDate")]
        public DateTime? EndDate { get; set; }

        [JsonPropertyName("editedBy")]
        public string EditedBy { get; set; } = null!;

        [JsonPropertyName("notes")]
        public string? Notes { get; set; }

        [JsonPropertyName("values")]
        public List<CaseStatusValue>? Values { get; set; }
    }

    public class CaseStatusValue
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = null!;

        [JsonPropertyName("value")]
        public string Value { get; set; } = null!;
    }

    public class UpdateCaseStatusValidator : AbstractValidator<UpdateCaseStatusRequest>
    {
        public UpdateCaseStatusValidator()
        {
            RuleFor(x => x.EndDate)
                .GreaterThan(DateTime.Now).When(x => x.EndDate != null).WithMessage("'endDate' must be in the future");
            RuleFor(x => x.EditedBy)
                .NotNull().WithMessage("'editedBy' must be provided")
                .EmailAddress().WithMessage("'editedBy' must be a valid email address");
            RuleFor(x => x.Notes)
                .MaximumLength(1000).When(x => x.Notes != null).WithMessage("'notes' must be less than or equal to 1,000 characters.");
            RuleForEach(x => x.Values)
                .ChildRules(value =>
                {
                    value.RuleFor(x => x.Name).NotNull().WithMessage("Field must have a name");
                    value.RuleFor(x => x.Value).NotNull().WithMessage("Field selected value must not be empty");
                });
        }
    }
}
