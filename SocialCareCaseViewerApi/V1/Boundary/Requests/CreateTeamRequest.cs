using System.Text.Json.Serialization;
using FluentValidation;

namespace SocialCareCaseViewerApi.V1.Boundary.Requests

#nullable enable
{
    public class CreateTeamRequest
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = null!;

        [JsonPropertyName("context")]
        public string Context { get; set; } = null!;
    }

    public class CreateTeamRequestValidator : AbstractValidator<CreateTeamRequest>
    {
        public CreateTeamRequestValidator()
        {
            RuleFor(t => t.Name)
                .NotNull().WithMessage("Team name must be provided")
                .MinimumLength(1).WithMessage("Team name must be provided")
                .MaximumLength(200).WithMessage("Team name has a maximum length of 200 characters");
            RuleFor(w => w.Context)
                .NotNull().WithMessage("Context flag must be provided")
                .MinimumLength(1).WithMessage("Context flag must be provided")
                .MaximumLength(1).WithMessage("Context flag must be no longer than 1 character")
                .Matches("(?i:^A|C)").WithMessage("Context flag must be 'A' or 'C'");
        }
    }
}
