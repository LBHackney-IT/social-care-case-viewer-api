using System;
using System.Collections.Generic;
using FluentValidation;
using Newtonsoft.Json;

#nullable enable
namespace SocialCareCaseViewerApi.V1.Boundary.Requests
{
    public class UpdateCaseStatusRequest
    {
        [JsonProperty("person_id")]
        public long PersonId { get; set; }

        [JsonProperty("start_date")]
        public DateTime? StartDate { get; set; }

        [JsonProperty("end_date")]
        public DateTime? EndDate { get; set; }

        [JsonProperty("edited_by")]
        public string EditedBy { get; set; } = null!;

        [JsonProperty("notes")]
        public string? Notes { get; set; }

        [JsonProperty("values")]
        public List<CaseStatusValue>? Values { get; set; }
    }

    public class CaseStatusValue
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }
    }

    public class UpdateCaseStatusValidator : AbstractValidator<UpdateCaseStatusRequest>
    {
        public UpdateCaseStatusValidator()
        {
            RuleFor(x => x.StartDate)
                .LessThan(DateTime.Now).When(x => x.StartDate != null).WithMessage("'start_date' must be in the past");
            RuleFor(x => x.EndDate)
                .GreaterThan(DateTime.Now).When(x => x.EndDate != null).WithMessage("'end_date' must be in the future");
            RuleFor(x => x.EditedBy)
                .NotNull().WithMessage("'edited_by' must be provided")
                .EmailAddress().WithMessage("'edited_by' must be a valid email address");
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
