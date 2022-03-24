using System;
using System.Text.Json.Serialization;
using FluentValidation;

namespace SocialCareCaseViewerApi.V1.Boundary.Requests
{
    public class UpdateAllocationRequest
    {
        [JsonPropertyName("id")]
        public int? Id { get; set; }

        [JsonPropertyName("ragRating")]
        public string? RagRating { get; set; }

        [JsonPropertyName("deallocationReason")]
        public string? DeallocationReason { get; set; }

        [JsonPropertyName("createdBy")]
        public string CreatedBy { get; set; }

        [JsonPropertyName("deallocationDate")]
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
                .Matches("(?i:^low|high|medium|urgent)").WithMessage("RAG rating must be 'low', 'high', 'medium' or 'urgent'");
            When(x => x.RagRating != null
                      && x.DeallocationReason != null
                      && x.DeallocationDate != null, () => { RuleFor(x => x.RagRating).Null().WithMessage("Please do not patch RagRating and deallocate at the same time"); });
        }
    }
}
