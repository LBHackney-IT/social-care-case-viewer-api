using System.Text.Json.Serialization;
using FluentValidation;

namespace SocialCareCaseViewerApi.V1.Boundary.Requests

#nullable enable
{
    public class CreatePersonalRelationshipRequest
    {
        [JsonPropertyName("personId")]
        public long PersonId { get; set; }

        [JsonPropertyName("otherPersonId")]
        public long OtherPersonId { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; } = null!;

        [JsonIgnore]
        public long? TypeId { get; set; }

        [JsonPropertyName("isMainCarer")]
        public string? IsMainCarer { get; set; }

        [JsonPropertyName("isInformalCarer")]
        public string? IsInformalCarer { get; set; }

        [JsonPropertyName("details")]
        public string? Details { get; set; }

        [JsonPropertyName("createdBy")]
        public string CreatedBy { get; set; } = null!;
    }

    public class CreatePersonalRelationshipRequestValidator : AbstractValidator<CreatePersonalRelationshipRequest>
    {
        public CreatePersonalRelationshipRequestValidator()
        {
            RuleFor(pr => pr.PersonId)
                .GreaterThanOrEqualTo(1).WithMessage("'personId' must be provided.");
            RuleFor(pr => pr.OtherPersonId)
                .GreaterThanOrEqualTo(1).WithMessage("'otherPersonId' must be provided.");
            RuleFor(pr => pr.Type)
                .NotNull().WithMessage("'type' must be provided.");
            RuleFor(pr => pr.IsMainCarer)
                .Matches("(?i:^Y|N)$").WithMessage("'isMainCarer' must be 'Y' or 'N'.");
            RuleFor(pr => pr.IsInformalCarer)
                .Matches("(?i:^Y|N)$").WithMessage("'isInformalCarer' must be 'Y' or 'N'.");
            RuleFor(pr => pr.Details)
                .MaximumLength(1000).WithMessage("'details' must be less than or equal to 1,000 characters.");
            RuleFor(pr => pr.CreatedBy)
                .NotNull().WithMessage("'createdBy' must be provided.")
                .EmailAddress().WithMessage("'createdBy' must be an email address.");
        }
    }
}
