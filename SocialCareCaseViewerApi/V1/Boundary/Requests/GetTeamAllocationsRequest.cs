using FluentValidation;
using Microsoft.AspNetCore.Mvc;

#nullable enable
namespace SocialCareCaseViewerApi.V1.Boundary.Requests
{
    public class GetTeamAllocationsRequest
    {
        [FromQuery(Name = "view")]
        public string View { get; set; } = null!;
    }

    public class GetTeamAllocationsRequestValidator : AbstractValidator<GetTeamAllocationsRequest>
    {
        public GetTeamAllocationsRequestValidator()
        {
            RuleFor(w => w.View)
                .NotNull().WithMessage("Type of view must be provided")
                .Matches("(?i:^allocated|unallocated)").WithMessage("View must be 'allocated' or 'unallocated'");

        }
    }
}
