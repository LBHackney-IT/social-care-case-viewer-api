using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using FluentValidation;
using SocialCareCaseViewerApi.V1.Domain;

namespace SocialCareCaseViewerApi.V1.Boundary.Requests

#nullable enable
{
    public class CreateCaseStatusRequest
    {
        [JsonPropertyName("personId")]
        public long PersonId { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; } = null!;

        [JsonPropertyName("fields")]
        public List<CaseStatusRequestField> Fields { get; set; } = new List<CaseStatusRequestField>();

        [JsonPropertyName("startDate")]
        public DateTime StartDate { get; set; }

        [JsonPropertyName("endDate")]
        public DateTime? EndDate { get; set; }

        [JsonPropertyName("notes")]
        public string Notes { get; set; } = null!;

        [JsonPropertyName("createdBy")]
        public string CreatedBy { get; set; } = null!;
    }

    public class CaseStatusRequestField
    {
        public String? Name { get; set; } = null!;
        public String? Selected { get; set; } = null!;
    }

    public class CreateCaseStatusRequestValidator : AbstractValidator<CreateCaseStatusRequest>
    {
        public CreateCaseStatusRequestValidator()
        {
            RuleFor(pr => pr.PersonId)
                .GreaterThanOrEqualTo(1).WithMessage("'personId' must be provided.");
            RuleFor(pr => pr.Type)
                .NotNull().WithMessage("'type' must be provided.");

            RuleForEach(pr => pr.Fields)
                .ChildRules(field =>
                {
                    field.RuleFor(t => t.Name).NotNull().WithMessage("Field must have a name");
                    field.RuleFor(t => t.Selected).NotNull().WithMessage("Field selected value must not be empty");
                });

            RuleFor(x => x.StartDate)
                .LessThan(DateTime.Now).WithMessage("'start_date' must be in the past");

            RuleFor(pr => pr.Notes)
                .MaximumLength(1000).WithMessage("'notes' must be less than or equal to 1,000 characters.");

            RuleFor(pr => pr.CreatedBy)
                .NotNull().WithMessage("'createdBy' must be provided.")
                .EmailAddress().WithMessage("'createdBy' must be an email address.");
        }
    }
}
