using FluentValidation;
using Microsoft.AspNetCore.Mvc;

#nullable enable
namespace SocialCareCaseViewerApi.V1.Boundary.Requests
{
    public class GetTeamsRequest
    {
        [FromQuery(Name = "context_flag")]
        public string ContextFlag { get; set; } = null!;
    }

    public class GetTeamsRequestValidator : AbstractValidator<GetTeamsRequest>
    {
        public GetTeamsRequestValidator()
        {
            RuleFor(t => t.ContextFlag)
                .MaximumLength(1).WithMessage("Context flag must be 1 character in length")
                .Matches("(?i:^A|C)").WithMessage("Context flag must be 'A' or 'C'");
        }
    }
}
