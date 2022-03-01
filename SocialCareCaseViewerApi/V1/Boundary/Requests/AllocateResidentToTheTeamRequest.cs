using System;
using System.Text.Json.Serialization;
using FluentValidation;

namespace SocialCareCaseViewerApi.V1.Boundary.Requests
{
    public class AllocateResidentToTheTeamRequest
    {
        [JsonPropertyName("personId")]
        public long PersonId { get; set; }

        [JsonPropertyName("allocatedTeamId")]
        public int AllocatedTeamId { get; set; }

        [JsonPropertyName("createdBy")]
        public int CreatedBy { get; set; }

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
            RuleFor(x => x.PersonId)
                .GreaterThan(0).WithMessage("Resident Id must be grater than 0");
            RuleFor(x => x.AllocatedTeamId)
                .GreaterThan(0).WithMessage("Team Id must be grater than 0");
        }
    }
}
