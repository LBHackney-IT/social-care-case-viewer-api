using System.Text.Json.Serialization;
using FluentValidation;

namespace SocialCareCaseViewerApi.V1.Boundary.Requests

#nullable enable
{
    public class CreateCaseSubmissionRequest
    {
        [JsonPropertyName("formId")]
        public string FormId { get; set; } = null!;

        [JsonPropertyName("socialCareId")]
        public int ResidentId { get; set; }

        [JsonPropertyName("createdBy")]
        public string CreatedBy { get; set; } = null!;
    }

    public class CreateCaseSubmissionRequestValidator : AbstractValidator<CreateCaseSubmissionRequest>
    {
        public CreateCaseSubmissionRequestValidator()
        {
            RuleFor(s => s.FormId)
                .NotNull().WithMessage("Provide a form ID");
            RuleFor(s => s.ResidentId)
                .NotNull().WithMessage("Provide an ID for the resident");
            RuleFor(s => s.CreatedBy)
                .NotNull().WithMessage("Provide who created the submission")
                .EmailAddress().WithMessage("Provide a valid email address for who created the submission");
        }
    }
}
