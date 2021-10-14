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

            RuleFor(pr => pr.Answers).Must(x => x.Count > 0).WithMessage("Answers must be provided"); 

            RuleForEach(pr => pr.Answers)
                .ChildRules(field =>
                {
                    field.RuleFor(t => t.Option).NotEmpty().WithMessage("Option must not be empty");
                    field.RuleFor(t => t.Value).NotEmpty().WithMessage("Value must not be empty");
                });

            RuleFor(x => x.StartDate)
                .LessThan(DateTime.Now).WithMessage("'start_date' must be in the past");

            RuleFor(pr => pr.CreatedBy)
                .NotNull().WithMessage("'createdBy' must be provided")
                .EmailAddress().WithMessage("'createdBy' must be an email address");
        }
    }
}
