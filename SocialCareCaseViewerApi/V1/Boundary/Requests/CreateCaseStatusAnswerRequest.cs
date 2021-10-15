using FluentValidation;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

#nullable enable
namespace SocialCareCaseViewerApi.V1.Boundary.Requests
{
    public class CreateCaseStatusAnswerRequest
    {
        [JsonPropertyName("caseStatusId")]
        public long CaseStatusId { get; set; }

        [JsonPropertyName("startDate")]
        public DateTime StartDate { get; set; }

        [JsonPropertyName("answers")]
        public List<CaseStatusRequestAnswers> Answers { get; set; } = new List<CaseStatusRequestAnswers>();

        [JsonPropertyName("createdBy")]
        public string CreatedBy { get; set; } = null!;
    }

    public class CreateCaseStatusAnswerRequestValidator : AbstractValidator<CreateCaseStatusAnswerRequest>
    {
        public CreateCaseStatusAnswerRequestValidator()
        {
            RuleFor(pr => pr.CaseStatusId)
                .GreaterThanOrEqualTo(1).WithMessage("'caseStatusId' must be provided");

            RuleFor(pr => pr.Answers).Must(x => x.Count > 0).WithMessage("'answers' must be provided");

            RuleForEach(pr => pr.Answers)
                .ChildRules(field =>
                {
                    field.RuleFor(t => t.Option).NotEmpty().WithMessage("'option' must not be empty");
                    field.RuleFor(t => t.Value).NotEmpty().WithMessage("'value' must not be empty");
                });

            RuleFor(x => x.StartDate)
                .NotEmpty().WithMessage("'startDate' must be provided")
                .Must(x => x != DateTime.MinValue)
                .WithMessage("'startDate' must have a valid value"); 

            RuleFor(pr => pr.CreatedBy)
                .NotNull().WithMessage("'createdBy' must be provided")
                .EmailAddress().WithMessage("'createdBy' must be an email address");
        }
    }
}
