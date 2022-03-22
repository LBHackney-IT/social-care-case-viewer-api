using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using FluentValidation;

#nullable enable
namespace SocialCareCaseViewerApi.V1.Boundary.Requests
{
    public class CreateWorkerRequest
    {
        [JsonPropertyName("email")]
        public string EmailAddress { get; set; } = null!;

        [JsonPropertyName("firstName")]
        public string FirstName { get; set; } = null!;

        [JsonPropertyName("lastName")]
        public string LastName { get; set; } = null!;

        [JsonPropertyName("createdBy")]
        public string CreatedBy { get; set; } = null!;

        [JsonPropertyName("contextFlag")]
        public string ContextFlag { get; set; } = null!;

        [JsonPropertyName("teams")]
        public List<WorkerTeamRequest> Teams { get; set; } = null!;

        [JsonPropertyName("role")]
        public string Role { get; set; } = null!;

        [JsonPropertyName("dateStart")]
        public DateTime DateStart { get; set; }
    }

    public class WorkerTeamRequest
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = null!;
    }

    public class CreateWorkerRequestValidator : AbstractValidator<CreateWorkerRequest>
    {
        // Rules are based on database/schema.sql DBO.SCCV_WORKER
        public CreateWorkerRequestValidator()
        {
            RuleFor(w => w.EmailAddress)
                .NotNull().WithMessage("Email address must be provided")
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
                .MaximumLength(1).WithMessage("Context flag must be no longer than 1 character")
                .Matches("(?i:^A|C)").WithMessage("Context flag must be 'A' or 'C'");
            RuleFor(w => w.Teams.Count).GreaterThan(0).WithMessage("A team must be provided");
            RuleFor(w => w.Teams.Count).LessThan(2).WithMessage("Please provide only one team");
            RuleForEach(w => w.Teams)
                .ChildRules(team =>
                {
                    team.RuleFor(t => t.Id).NotNull().WithMessage("Teams must be provided with an ID");
                    team.RuleFor(t => t.Id).GreaterThan(0).WithMessage("Team ID must be greater than 0");
                    team.RuleFor(t => t.Name).NotNull().WithMessage("Team must be provided with a name");
                    team.RuleFor(t => t.Name).MinimumLength(1).WithMessage("Team must be provided with a name");
                    team.RuleFor(t => t.Name).MaximumLength(200).WithMessage("Team name must be no more than 200 characters");
                });
            RuleFor(w => w.Role)
                .NotNull().WithMessage("Role must be provided")
                .MinimumLength(1).WithMessage("Role must be provided")
                .MaximumLength(200).WithMessage("Role provided is too long (more than 200 characters)");
            RuleFor(w => w.DateStart)
                .NotNull().WithMessage("Start date must be provided");
            RuleFor(w => w.CreatedBy)
                .NotNull().WithMessage("Created by email address must be provided")
                .MaximumLength(62).WithMessage("Created by email address must be no longer than 62 characters")
                .EmailAddress().WithMessage("Created by email address must be valid");
        }
    }

}
