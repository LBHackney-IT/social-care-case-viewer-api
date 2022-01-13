using FluentValidation;

namespace SocialCareCaseViewerApi.V1.Boundary.Requests
{
    public class UpdateMashResidentRequest
    {
        public long SocialCareId { get; set; }
    }

    public class UpdateMashResidentRequestValidator : AbstractValidator<UpdateMashResidentRequest>
    {
        public UpdateMashResidentRequestValidator()
        {
            RuleFor(x => x.SocialCareId).NotNull().WithMessage("Must provide a social care ID");
        }
    }
}
