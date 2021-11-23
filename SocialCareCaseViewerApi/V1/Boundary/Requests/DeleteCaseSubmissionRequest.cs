using FluentValidation;
using System.Text.Json.Serialization;

namespace SocialCareCaseViewerApi.V1.Boundary.Requests
{
    public class DeleteCaseSubmissionRequest
    {
        [JsonPropertyName("deletedBy")]
        public string DeletedBy { get; set; }

        [JsonPropertyName("deleteReason")]
        public string DeleteReason { get; set; }

        [JsonPropertyName("deleteRequestedBy")]
        public string DeleteRequestedBy { get; set; }
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
