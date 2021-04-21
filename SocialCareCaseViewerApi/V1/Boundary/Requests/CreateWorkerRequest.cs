using System;
using System.Text.Json.Serialization;
using FluentValidation;

#nullable enable
namespace SocialCareCaseViewerApi.V1.Boundary.Requests
{
    public class CreateWorkerRequest
    {
        [JsonPropertyName("emailAddress")]
        public string EmailAddress { get; set; } = null!;

        [JsonPropertyName("firstName")]
        public string FirstName { get; set; }= null!;

        [JsonPropertyName("lastName")]
        public string LastName { get; set; } = null!;

        [JsonPropertyName("contextFlag")]
        public string ContextFlag { get; set; } = null!;

        [JsonPropertyName("team")]
        public string Team { get; set; } = null!;

        [JsonPropertyName("role")]
        public string Role { get; set; } = null!;

        [JsonPropertyName("dateStart")]
        public DateTime DateStart { get; set; }
    }

    public class CreateWorkerRequestValidator : AbstractValidator<CreateWorkerRequest>
    {
        // Rules are based on database/schema.sql DBO.SCCV_WORKER
        public CreateWorkerRequestValidator()
        {
            RuleFor(w => w.EmailAddress)
                .NotNull().WithMessage("Email address must be provided")
                .MinimumLength(1).WithMessage("Email address must be provided")
                .MaximumLength(62).WithMessage("Email address must be no longer than 62 characters")
                .EmailAddress().WithMessage("Email address must be valid");
            RuleFor(w => w.FirstName)
                .NotNull().WithMessage("First name must be provided")
                .MinimumLength(1).WithMessage("First name must be provided")
                .MaximumLength(100).WithMessage("First name must be no longer than 100 characters");
            RuleFor(w => w.LastName)
                .NotNull().WithMessage("Last name must be provided")
                .MinimumLength(1).WithMessage("Last name must be provided")
                .MaximumLength(100).WithMessage("Last name must be no longer than 100 characters");
            RuleFor(w => w.ContextFlag)
                .NotNull().WithMessage("Context flag must be provided")
                .MinimumLength(1).WithMessage("Context flag must be provided")
                .MaximumLength(100).WithMessage("Context flag must be no longer than 100 characters");
            RuleFor(w => w.Team)
                .NotNull().WithMessage("Team must be provided")
                .MinimumLength(1).WithMessage("Team must be provided")
                .MaximumLength(200).WithMessage("Team provided is too long (more than 200 characters)");
            RuleFor(w => w.Role)
                .NotNull().WithMessage("Role must be provided")
                .MinimumLength(1).WithMessage("Role must be provided")
                .MaximumLength(200).WithMessage("Role provided is too long (more than 200 characters)");
            RuleFor(w => w.DateStart)
                .NotNull().WithMessage("Start date must be provided");
        }
    }

}
