using System;
using FluentValidation;

namespace SocialCareCaseViewerApi.V1.Boundary.Requests
{
    public class UpdateAllocationRequest
    {
        public int Id { get; set; }

        public string DeallocationReason { get; set; }

        public string CreatedBy { get; set; }

        public DateTime DeallocationDate { get; set; }
    }

    public class UpdateAllocationRequestValidator : AbstractValidator<UpdateAllocationRequest>
    {
        public UpdateAllocationRequestValidator()
        {
            RuleFor(x => x.Id)
                .NotNull().WithMessage("Id Required")
                .InclusiveBetween(1, int.MaxValue).WithMessage("Id must be grater than 1");
            RuleFor(x => x.DeallocationReason)
                .MinimumLength(1).WithMessage("Deallocation reason required");
            RuleFor(x => x.CreatedBy)
                .NotNull().WithMessage("Email required")
                .EmailAddress().WithMessage("Provide a valid email");
            RuleFor(x => x.DeallocationDate)
                .LessThan(DateTime.Now).WithMessage("Deallocation date must be in the past");
        }
    }
}
