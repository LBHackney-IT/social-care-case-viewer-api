using System;
using FluentValidation;

namespace SocialCareCaseViewerApi.V1.Boundary.Requests
{
    public class UpdateAllocationRequest
    {
        public int? Id { get; set; }

        public string? RagRating { get; set; }

        public string? DeallocationReason { get; set; }

        public string CreatedBy { get; set; }

        public DateTime? DeallocationDate { get; set; }
    }

    public class UpdateAllocationRequestValidator : AbstractValidator<UpdateAllocationRequest>
    {
        public UpdateAllocationRequestValidator()
        {
            RuleFor(x => x.Id)
                .NotNull().WithMessage("Id Required")
                .InclusiveBetween(1, int.MaxValue).WithMessage("Id must be greater than 1");
            RuleFor(x => x.DeallocationReason)
                .NotNull().WithMessage("Deallocation reason required")
                .When(x => x.RagRating == null && x.DeallocationDate != null)
                .MinimumLength(1).WithMessage("Deallocation reason required");
            RuleFor(x => x.DeallocationDate)
                .NotNull().WithMessage("Deallocation Date is required")
                .When(x => x.RagRating == null && x.DeallocationReason != null)
                .LessThanOrEqualTo(DateTime.Now).WithMessage("DeallocationDate start date must not be in future"); ;
            RuleFor(x => x.CreatedBy)
                .NotNull().WithMessage("Email required")
                .EmailAddress().WithMessage("Provide a valid email");
            RuleFor(x => x.RagRating)
                .NotNull().WithMessage("Please provide either RagRating or Deallocation Details")
                .When(x => x.DeallocationReason == null && x.DeallocationDate == null)
                .Matches("(?i:^green|red|amber|purple)").WithMessage("RAG rating must be 'green', 'red', 'amber' or 'purple'");
            When(x => x.RagRating != null
                      && x.DeallocationReason != null
                      && x.DeallocationDate != null, () => { RuleFor(x => x.RagRating).Null().WithMessage("Please do not patch RagRating and deallocate at the same time"); });
        }
    }
}
