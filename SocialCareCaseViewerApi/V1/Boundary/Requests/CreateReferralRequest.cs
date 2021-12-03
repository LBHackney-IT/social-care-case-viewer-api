using System.Collections.Generic;
using System;
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
        public List<MashResidentRequest> MashResidents { get; set; } = null!;
    }

    public class CreateReferralRequestValidator : AbstractValidator<CreateReferralRequest>
    {
        public CreateReferralRequestValidator()
        {
            RuleFor(m => m.Referrer).NotNull().MinimumLength(1).WithMessage("Referrer must have at least one character");
            RuleFor(m => m.RequestedSupport).NotNull().MinimumLength(1).WithMessage("Requested support must have at least one character");
            RuleFor(m => m.ReferralUri).NotNull().MinimumLength(1).WithMessage("Referral document url must have at least one character");
            RuleFor(m => m.MashResidents.Count).GreaterThan(0).WithMessage("List of referred clients can not contain empty strings");
        }

    }

    public class MashResidentRequest
    {
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public DateTime? DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public string? Ethnicity { get; set; }
        public string? FirstLanguage { get; set; }
        public string? School { get; set; }
        public string? Address { get; set; }
        public string? Postcode { get; set; }
    }
}
