using FluentValidation;
using Microsoft.AspNetCore.Mvc;

#nullable enable
namespace SocialCareCaseViewerApi.V1.Boundary.Requests
{
    public class GetTeamsRequest
    {
        [FromQuery(Name = "id")]
        public int? Id { get; set; }

        [FromQuery(Name = "name")]
        public string? Name { get; set; }

        [FromQuery(Name = "context_flag")]
        public string? ContextFlag { get; set; }
    }

    public class GetTeamsRequestValidator : AbstractValidator<GetTeamsRequest>
    {
        public GetTeamsRequestValidator()
        {
            RuleFor(t => t).Must(t => t.Name != null || t.ContextFlag != null || t.Id != null)
                .WithMessage("Must provide either a team ID, team name or context flag");
            RuleFor(t => t.Name)
                .MinimumLength(1).WithMessage("Team name must be at least 1 character")
                .MaximumLength(200).WithMessage("Team name has a maximum length of 200 characters");
            RuleFor(t => t.ContextFlag)
                .MaximumLength(1).WithMessage("Context flag must be 1 character in length")
                .Matches("(?i:^A|C)").WithMessage("Context flag must be 'A' or 'C'");
        }
    }
}
