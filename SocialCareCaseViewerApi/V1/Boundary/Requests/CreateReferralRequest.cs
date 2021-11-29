using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using FluentValidation;

#nullable enable
namespace SocialCareCaseViewerApi.V1.Boundary.Requests
{
    public class CreateReferralRequest
    {
        public string Referrer { get; set; } = null!;
        public string RequestedSupport { get; set; } = null!;
        public string ReferralUri { get; set; } = null!;
        public List<string> Clients { get; set; } = null!;
    }

    public class CreateReferralRequestValidator : AbstractValidator<CreateReferralRequest>
    {
        public CreateReferralRequestValidator()
        {
            RuleFor(m => m.Referrer).NotNull().MinimumLength(1).WithMessage("Referrer must have at least one character");
            RuleFor(m => m.RequestedSupport).NotNull().MinimumLength(1).WithMessage("Requested support must have at least one character");
            RuleFor(m => m.ReferralUri).NotNull().MinimumLength(1).WithMessage("Referral document url must have at least one character");
            RuleFor(m => m.Clients).Must(NotBeEmpty).WithMessage("List of referred clients can not contain empty strings");
        }

        private static bool NotBeEmpty(List<string> client)
        {
            return client.Any() && !client.Contains("");
        }
    }
}
