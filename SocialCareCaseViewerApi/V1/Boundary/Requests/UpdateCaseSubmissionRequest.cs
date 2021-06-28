using System.Text.Json.Serialization;
using FluentValidation;

#nullable enable
namespace SocialCareCaseViewerApi.V1.Boundary.Requests
{
    public class UpdateCaseSubmissionRequest
    {
        [JsonPropertyName("updatedBy")]
        public string UpdatedBy { get; set; } = null!;

        [JsonPropertyName("submissionState")]
        public string? SubmissionState { get; set; }
    }

    public class UpdateCaseSubmissionRequestValidator : AbstractValidator<UpdateCaseSubmissionRequest>
    {
        public UpdateCaseSubmissionRequestValidator()
        {
            RuleFor(s => s.UpdatedBy)
                .NotNull().WithMessage("Provide who is finishing the submission")
                .EmailAddress().WithMessage("Provide a valid email address for who is finishing the submission");
        }
    }
}
