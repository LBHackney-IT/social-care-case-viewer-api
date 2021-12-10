
using FluentValidation;

#nullable enable
namespace SocialCareCaseViewerApi.V1.Boundary.Requests
{
    public class UpdateMashReferral
    {
        public string? WorkerEmail { get; set; }
        public int? WorkerId { get; set; }
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
                .When(x => x.UpdateType == "INITIAL-DECISION" || x.UpdateType == "SCREENING-DECISION" || x.UpdateType == "FINAL-DECISION")
                    .WithMessage("Must provide a decision");
            RuleFor(x => x.Decision)
                .MinimumLength(1)
                    .When(x => x.UpdateType == "INITIAL-DECISION" || x.UpdateType == "SCREENING-DECISION" || x.UpdateType == "FINAL-DECISION")
                .WithMessage("Must provide a decision");
            RuleFor(x => x.ReferralCategory)
                .NotNull()
                    .When(x => x.UpdateType == "INITIAL-DECISION" || x.UpdateType == "FINAL-DECISION")
                    .WithMessage("Must provide a referral category");
            RuleFor(x => x.ReferralCategory)
                .MinimumLength(1)
                    .When(x => x.UpdateType == "INITIAL-DECISION" || x.UpdateType == "FINAL-DECISION")
                    .WithMessage("Must provide a referral category");
            RuleFor(x => x.RequiresUrgentContact)
                .NotNull()
                .When(x => x.UpdateType == "CONTACT-DECISION" || x.UpdateType == "INITIAL-DECISION" ||
                           x.UpdateType == "SCREENING-DECISION" || x.UpdateType == "FINAL-DECISION")
                .WithMessage("Must provide if urgent contact is required");
            When(x => x.WorkerEmail != null, () =>
            {
                RuleFor(x => x.WorkerEmail).EmailAddress();
                RuleFor(x => x.WorkerId).Null().WithMessage("Do not provided both worker id and worker email address");
            });
            When(x => x.WorkerId != null, () =>
            {
                RuleFor(x => x.WorkerId).GreaterThan(0);
                RuleFor(x => x.WorkerEmail).Null().WithMessage("Do not provided both worker id and worker email address");
            });
        }
    }
}
