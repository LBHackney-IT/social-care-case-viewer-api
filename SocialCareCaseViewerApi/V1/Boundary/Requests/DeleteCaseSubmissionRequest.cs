using FluentValidation;
using System.Text.Json.Serialization;

#nullable enable
namespace SocialCareCaseViewerApi.V1.Boundary.Requests
{
    public class DeleteCaseSubmissionRequest
    {
        [JsonPropertyName("deletedBy")]
        public string DeletedBy { get; set; } = null!;

        [JsonPropertyName("deleteReason")]
        public string DeleteReason { get; set; } = null!;

        [JsonPropertyName("deleteRequestedBy")]
        public string DeleteRequestedBy { get; set; } = null!;
    }

    public class DeleteCaseSubmissionRequestValidator : AbstractValidator<DeleteCaseSubmissionRequest>
    {
        public DeleteCaseSubmissionRequestValidator()
        {
            RuleFor(s => s.DeletedBy)
                .NotNull().WithMessage("Provide who is deleting the submission")
                .EmailAddress().WithMessage("Provide a valid email address for who is deleting the submission");

            RuleFor(s => s.DeleteRequestedBy).NotEmpty().WithMessage("Provide valid delete requested by name");
            RuleFor(s => s.DeleteReason).NotEmpty().WithMessage("Provide valid delete reason");
        }
    }
}
