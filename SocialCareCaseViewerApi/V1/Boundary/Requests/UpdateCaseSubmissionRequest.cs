using System.Text.Json.Serialization;
using FluentValidation;

#nullable enable
namespace SocialCareCaseViewerApi.V1.Boundary.Requests
{
    public class UpdateCaseSubmissionRequest
    {
        [JsonPropertyName("createdBy")]
        public string CreatedBy { get; set; } = null!;

        [JsonPropertyName("submissionState")]
        public string? SubmissionState { get; set; }
    }

    public class UpdateCaseSubmissionRequestValidator : AbstractValidator<UpdateCaseSubmissionRequest>
    {
        public UpdateCaseSubmissionRequestValidator()
        {
            RuleFor(s => s.CreatedBy)
                .NotNull().WithMessage("Provide who is finishing the submission")
                .EmailAddress().WithMessage("Provide a valid email address for who is finishing the submission");
        }
    }
}
