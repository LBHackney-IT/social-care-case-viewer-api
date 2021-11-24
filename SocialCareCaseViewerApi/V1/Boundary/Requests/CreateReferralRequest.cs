using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using FluentValidation;

namespace SocialCareCaseViewerApi.V1.Boundary.Requests
{
    public class CreateReferralRequest
    {
        [Required]
        public string Referrer { get; set; }

        [Required]
        public string RequestedSupport { get; set; }

        [Required]
        public string ReferralUri { get; set; }

        [Required]
        public List<string> Clients { get; set; }
    }

    public class CreateReferralRequestValidator : AbstractValidator<CreateReferralRequest>
    {
        public CreateReferralRequestValidator()
        {
            RuleFor(m => m.Referrer).NotNull().MinimumLength(1).WithMessage("Referrer must have at least one character");
            RuleFor(m => m.RequestedSupport).NotNull().MinimumLength(1).WithMessage("Requested support must have at least one character");
            RuleFor(m => m.ReferralUri).NotNull().MinimumLength(1).WithMessage("Referral document url must have at least one character");
            RuleFor(m => m.Clients).Must(NotBeEmptyOrNull).WithMessage("List of referred clients can not contain empty strings");
        }

        private static bool NotBeEmptyOrNull(List<string> client)
        {
            if (client == null) return false;
            return client.Any() && !client.Contains("");
        }
    }
}
