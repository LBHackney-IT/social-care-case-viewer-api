using System;
using System.ComponentModel.DataAnnotations;
using FluentValidation;

namespace SocialCareCaseViewerApi.V1.Boundary.Requests
{
    public class PatchWarningNoteRequest
    {
        public long WarningNoteId { get; set; }
        public DateTime? ReviewDate { get; set; }
        public string ReviewedBy { get; set; }
        public DateTime? NextReviewDate { get; set; }
        public string Status { get; set; }
        public DateTime? EndedDate { get; set; }
        public string EndedBy { get; set; }
        public string ReviewNotes { get; set; }
        public string ManagerName { get; set; }
        public DateTime? DiscussedWithManagerDate { get; set; }
        public bool? DisclosedWithIndividual { get; set; }
    }

    public class PatchWarningNoteRequestValidator : AbstractValidator<PatchWarningNoteRequest>
    {
        public PatchWarningNoteRequestValidator()
        {
            RuleFor(x => x.WarningNoteId)
                .NotNull().WithMessage("Warning Note Id required")
                .InclusiveBetween(1, long.MaxValue).WithMessage("Warning Note Id must be greater than 1");
            RuleFor(x => x.ReviewDate)
                .NotNull().WithMessage("Review date required")
                .LessThan(DateTime.Now).WithMessage("Review date must be in the past");
            RuleFor(x => x.ReviewedBy)
                .NotNull().WithMessage("Reviewer email required")
                .EmailAddress().WithMessage("Provide a valid email address");
            RuleFor(x => x.NextReviewDate)
                .GreaterThan(DateTime.Now).WithMessage("Next review date must be in the future");
            RuleFor(x => x.Status)
                .NotNull().WithMessage("Status must be provided")
                .Must(x => x.Equals("open") || x.Equals("closed")).WithMessage("Provide a valid status");
            RuleFor(x => x.EndedDate)
                .LessThan(DateTime.Today.AddDays(1)).WithMessage("Ended date must be in the past");
            RuleFor(x => x.EndedBy)
                .EmailAddress().WithMessage("Provide a valid email address");
            RuleFor(x => x.ReviewNotes)
                .NotNull().WithMessage("Review notes required")
                .MinimumLength(1).WithMessage("Review notes required")
                .MaximumLength(1000).WithMessage("Review notes should be 1000 characters or less");
            RuleFor(x => x.ManagerName)
                .MaximumLength(100).WithMessage("Manager name must be less than 100 characters");
            RuleFor(x => x.DiscussedWithManagerDate)
                .LessThan(DateTime.Now).WithMessage("Discussed with manager date must be in the past");
        }
    }
}
