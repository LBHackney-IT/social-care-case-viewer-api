using System;
using System.Text.Json.Serialization;
using FluentValidation;

namespace SocialCareCaseViewerApi.V1.Boundary.Requests
{
    public class UpdateAllocationRequest
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("deallocationDate")]
        public DateTime? DeallocationDate { get; set; }

        [JsonPropertyName("deallocationReason")]
        public string? DeallocationReason { get; set; }

        [JsonPropertyName("allocatedWorkerId")]
        public int? AllocatedWorkerId { get; set; }

        [JsonPropertyName("createdBy")]
        public string CreatedBy { get; set; }

        [JsonPropertyName("allocationStartDate")]
        public DateTime? AllocationStartDate { get; set; }

        [JsonPropertyName("ragRating")]
        public string? RagRating { get; set; }

        [JsonPropertyName("summary")]
        public string? Summary { get; set; }

        [JsonPropertyName("carePackage")]
        public string? CarePackage { get; set; }
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
