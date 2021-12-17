
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
            RuleFor(x => x.WorkerId)
                .NotNull()
                .GreaterThan(0)
                .When(x => x.WorkerEmail == null && x.WorkerId != null && x.UpdateType == "ASSIGN-WORKER")
                .WithMessage("Must provide a valid worker id or email");
            RuleFor(x => x.WorkerEmail)
                .NotNull()
                .EmailAddress()
                .When(x => x.WorkerId == null && x.WorkerEmail != null && x.UpdateType == "ASSIGN-WORKER")
                .WithMessage("Must provide a valid worker id or email");
            When(x => x.WorkerEmail == null && x.WorkerId == null && x.UpdateType == "ASSIGN-WORKER", () =>
            {
                RuleFor(x => x.WorkerId).NotNull().WithMessage("Must provide a valid worker id or email");
            });
            When(x => x.WorkerEmail != null && x.WorkerId != null && x.UpdateType == "ASSIGN-WORKER", () =>
            {
                RuleFor(x => x.WorkerId).Null().WithMessage("Do not provide both worker id and worker email address");
            });
        }
    }
}
