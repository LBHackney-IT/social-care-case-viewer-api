using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using FluentValidation;

namespace SocialCareCaseViewerApi.V1.Boundary.Requests

#nullable enable
{
    public class CreateCaseStatusRequest
    {
        [JsonPropertyName("personId")]
        public long PersonId { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; } = null!;

        [JsonPropertyName("answers")]
        public List<CaseStatusRequestAnswers> Answers { get; set; } = new List<CaseStatusRequestAnswers>();

        [JsonPropertyName("startDate")]
        public DateTime StartDate { get; set; }

        [JsonPropertyName("notes")]
        public string? Notes { get; set; } = null!;

        [JsonPropertyName("createdBy")]
        public string CreatedBy { get; set; } = null!;
    }

    public class CaseStatusRequestAnswers
    {
        public string? Option { get; set; } = null!;
        public string? Value { get; set; } = null!;
    }

    public class CreateCaseStatusRequestValidator : AbstractValidator<CreateCaseStatusRequest>
    {
        public CreateCaseStatusRequestValidator()
        {
            List<string> conditions = new List<string>() { "CIN", "CP", "LAC" };

            RuleFor(pr => pr.PersonId)
                .GreaterThanOrEqualTo(1).WithMessage("'personId' must be provided.");

            RuleFor(pr => pr.Type)
                .NotNull().WithMessage("'type' must be provided.")
                .Must(x => conditions.Contains(x))
                .WithMessage("'type' must be CIN, CP or LAC.");

            RuleFor(x => x.StartDate)
                .NotEqual(DateTime.MinValue).WithMessage("'startDate' must be provided.")
                .LessThanOrEqualTo(DateTime.Now).WithMessage("'start_date' must be today or in the past");

            RuleFor(pr => pr.Notes)
                .MaximumLength(1000).WithMessage("'notes' must be less than or equal to 1,000 characters.");

            RuleFor(pr => pr.CreatedBy)
                .NotNull().WithMessage("'createdBy' must be provided.")
                .EmailAddress().WithMessage("'createdBy' must be an email address.");

            RuleFor(pr => pr.Answers)
                .Must(x => x.Count == 1)
                .When(x => x.Type == "CP")
                .WithMessage("CP type must have one answer only");

            RuleFor(pr => pr.Answers)
               .Must(x => x.Count == 2)
               .When(x => x.Type == "LAC")
               .WithMessage("LAC type must have two answers only");

            RuleForEach(pr => pr.Answers)
               .ChildRules(field =>
               {
                   field.RuleFor(t => t.Option).NotEmpty().WithMessage("'option' must not be empty");
                   field.RuleFor(t => t.Value).NotEmpty().WithMessage("'value' must not be empty");
               })
               .When(x => x.Type == "CP" || x.Type == "LAC");
        }
    }
}
