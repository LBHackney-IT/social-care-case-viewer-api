using System.Text.Json.Serialization;
using FluentValidation;

#nullable enable
namespace SocialCareCaseViewerApi.V1.Boundary.Requests
{
    public class FinishCaseSubmissionRequest
    {
        [JsonPropertyName("createdBy")]
        public string CreatedBy { get; set; } = null!;
    }

    public class FinishCaseSubmissionRequestValidator : AbstractValidator<FinishCaseSubmissionRequest>
    {
        public FinishCaseSubmissionRequestValidator()
        {
            RuleFor(s => s.CreatedBy)
                .NotNull().WithMessage("Provide who is finishing the submission")
                .EmailAddress().WithMessage("Provide a valid email address for who is finishing the submission");
        }
    }
}
