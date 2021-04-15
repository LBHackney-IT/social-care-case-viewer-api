using System;
using System.ComponentModel.DataAnnotations;
using FluentValidation;

namespace SocialCareCaseViewerApi.V1.Boundary.Requests
{
    public class CreateAllocationRequest
    {
        public long MosaicId { get; set; }

        public long AllocatedWorkerId { get; set; }

        public long AllocatedTeamId { get; set; }

        public string CreatedBy { get; set; }

        public DateTime AllocationStartDate { get; set; }
    }

    public class CreateAllocationRequestValidator : AbstractValidator<CreateAllocationRequest>
    {
        public CreateAllocationRequestValidator()
        {
            RuleFor(x => x.MosaicId)
                .NotNull().WithMessage("Mosaic Id Required")
                .InclusiveBetween(1, int.MaxValue).WithMessage($"Mosaic Id must be grater than 1");
            RuleFor(x => x.AllocatedWorkerId)
                .NotNull().WithMessage("Worker Id Required")
                .InclusiveBetween(1, int.MaxValue).WithMessage($"Worker Id must be grater than 1");
            RuleFor(x => x.AllocatedTeamId)
                .NotNull().WithMessage("Team Id Required")
                .InclusiveBetween(1, int.MaxValue).WithMessage($"Team Id must be grater than 1");
            RuleFor(x => x.CreatedBy)
                .NotNull().WithMessage("Email Required")
                .EmailAddress().WithMessage("Enter a valid email address");
            RuleFor(x => x.AllocationStartDate)
                .NotNull().WithMessage("Allocation start date required");
        }
    }
}
