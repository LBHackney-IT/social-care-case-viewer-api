
using FluentValidation;

#nullable enable
namespace SocialCareCaseViewerApi.V1.Boundary.Requests
{
    public class UpdateMashReferral
    {
        public string WorkerEmail { get; set; } = null!;
        public string UpdateType { get; set; } = null!;
        public string? Decision { get; set; }
        public string? ReferralCategory { get; set; }
        public bool? RequiresUrgentContact { get; set; }
    }

    public class UpdateMashReferralValidator : AbstractValidator<UpdateMashReferral>
    {
        public UpdateMashReferralValidator()
        {
            RuleFor(x => x.Decision)
                .NotNull()
                    .When(x => x.UpdateType.ToUpper() == "SCREENING-DECISION" || x.UpdateType.ToUpper() == "INITIAL-DECISION")
                    .WithMessage("Must provide a decision");
            RuleFor(x => x.Decision)
                .MinimumLength(1)
                    .When(x => x.UpdateType.ToUpper() == "SCREENING-DECISION" || x.UpdateType.ToUpper() == "INITIAL-DECISION")
                    .WithMessage("Must provide a decision");
            RuleFor(x => x.ReferralCategory)
                .NotNull()
                    .When(x => x.UpdateType.ToUpper() == "INITIAL-DECISION")
                    .WithMessage("Must provide a referral category");
            RuleFor(x => x.ReferralCategory)
                .MinimumLength(1)
                    .When(x => x.UpdateType.ToUpper() == "INITIAL-DECISION")
                    .WithMessage("Must provide a referral category");
            RuleFor(x => x.RequiresUrgentContact)
                .NotNull()
                    .When(x => x.UpdateType.ToUpper() == "SCREENING-DECISION" || x.UpdateType.ToUpper() == "INITIAL-DECISION")
                    .WithMessage("Must provide if urgent contact is required");
        }
    }
}
