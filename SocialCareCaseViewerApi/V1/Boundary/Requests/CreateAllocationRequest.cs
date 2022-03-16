using System;
using System.Text.Json.Serialization;
using FluentValidation;

namespace SocialCareCaseViewerApi.V1.Boundary.Requests
{
    public class CreateAllocationRequest
    {
        [JsonPropertyName("mosaicId")]
        public long MosaicId { get; set; }

        [JsonPropertyName("allocatedWorkerId")]
        public int AllocatedWorkerId { get; set; }

        [JsonPropertyName("allocatedTeamId")]
        public int AllocatedTeamId { get; set; }

        [JsonPropertyName("createdBy")]
        public string CreatedBy { get; set; }

        [JsonPropertyName("allocationStartDate")]
        public DateTime AllocationStartDate { get; set; }

        [JsonPropertyName("ragRating")]
        public string RagRating { get; set; }

        [JsonPropertyName("summary")]
        public string Summary { get; set; }

        [JsonPropertyName("carePackage")]
        public string CarePackage { get; set; }
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
