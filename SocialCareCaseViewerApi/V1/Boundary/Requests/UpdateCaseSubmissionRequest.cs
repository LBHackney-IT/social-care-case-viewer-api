using System.Collections.Generic;
using System.Text.Json.Serialization;
using FluentValidation;

#nullable enable
namespace SocialCareCaseViewerApi.V1.Boundary.Requests
{
    public class UpdateCaseSubmissionRequest
    {
        [JsonPropertyName("editedBy")]
        public string EditedBy { get; set; } = null!;

        [JsonPropertyName("submissionState")]
        public string? SubmissionState { get; set; }

        [JsonPropertyName("residents")]
        public List<long>? Residents { get; set; }

        [JsonPropertyName("rejectionReason")]
        public string? RejectionReason { get; set; }
    }

    public class UpdateCaseSubmissionRequestValidator : AbstractValidator<UpdateCaseSubmissionRequest>
    {
        public UpdateCaseSubmissionRequestValidator()
        {
            RuleFor(s => s.EditedBy)
                .NotNull().WithMessage("Provide who is updating the submission")
                .EmailAddress().WithMessage("Provide a valid email address for who is updating the submission");
            When(s => s.Residents != null, () =>
            {
                RuleFor(s => s.Residents!.Count).GreaterThan(0)
                    .WithMessage("Provide residents for who this submission applies too");
            });

        }
    }
}
