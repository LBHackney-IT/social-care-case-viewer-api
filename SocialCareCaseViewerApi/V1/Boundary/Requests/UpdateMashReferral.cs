
using FluentValidation;

#nullable enable
namespace SocialCareCaseViewerApi.V1.Boundary.Requests
{
    public class UpdateMashReferral
    {
        public string UpdateType { get; set; }
        public string? Decision { get; set; }
        public bool? RequiresUrgentContact { get; set; }
    }

    public class UpdateMashReferralValidator : AbstractValidator<UpdateMashReferral>
    {
        public UpdateMashReferralValidator()
        {
            RuleFor(x => x.Decision)
                .NotNull().When(x => x.UpdateType.ToUpper() == "SCREENING-DECISION");
            RuleFor(x => x.Decision)
                .MinimumLength(1).When(x => x.UpdateType.ToUpper() == "SCREENING-DECISION");
            RuleFor(x => x.RequiresUrgentContact)
                .NotNull().When(x => x.UpdateType.ToUpper() == "SCREENING-DECISION");
        }
    }
}
