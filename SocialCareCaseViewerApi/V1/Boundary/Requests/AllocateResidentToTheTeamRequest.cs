using System;
using System.Text.Json.Serialization;
using FluentValidation;

namespace SocialCareCaseViewerApi.V1.Boundary.Requests
{
    public class AllocateResidentToTheTeamRequest
    {
        [JsonPropertyName("allocatedTeamId")]
        public int AllocatedTeamId { get; set; }

        [JsonPropertyName("createdBy")]
        public string CreatedBy { get; set; }

        [JsonPropertyName("summary")]
        public string Summary { get; set; }

        [JsonPropertyName("carePackage")]
        public string CarePackage { get; set; }

        [JsonPropertyName("ragRating")]
        public string RagRating { get; set; }

        [JsonPropertyName("allocationDate")]
        public DateTime AllocationDate { get; set; }
    }

    public class AllocateResidentToTheTeamRequestValidator : AbstractValidator<AllocateResidentToTheTeamRequest>
    {
        public AllocateResidentToTheTeamRequestValidator()
        {
            RuleFor(x => x.AllocatedTeamId)
                .NotNull().WithMessage("Team Id Required")
                .InclusiveBetween(1, int.MaxValue).WithMessage($"Team Id must be grater than 1");
            RuleFor(x => x.CreatedBy)
                .NotNull().WithMessage("Email Required")
                .EmailAddress().WithMessage("Enter a valid email address");
            RuleFor(x => x.AllocationDate)
                .NotNull().WithMessage("Allocation start date required");
        }
    }
}
