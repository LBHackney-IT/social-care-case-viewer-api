using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using FluentValidation;

#nullable enable
namespace SocialCareCaseViewerApi.V1.Boundary.Requests
{
    public class UpdateWorkerRequest
    {
        [JsonPropertyName("workerId")]
        public int WorkerId { get; set; }

        [JsonPropertyName("createdBy")]
        public string ModifiedBy { get; set; } = null!;

        [JsonPropertyName("firstName")]
        public string FirstName { get; set; } = null!;

        [JsonPropertyName("lastName")]
        public string LastName { get; set; } = null!;

        [JsonPropertyName("contextFlag")]
        public string ContextFlag { get; set; } = null!;

        [JsonPropertyName("teams")]
        public List<WorkerTeamRequest> Teams { get; set; } = null!;

        [JsonPropertyName("role")]
        public string? Role { get; set; }

        [JsonPropertyName("dateStart")]
        public DateTime? DateStart { get; set; }
    }

    public class UpdateWorkerRequestValidator : AbstractValidator<UpdateWorkerRequest>
    {
        public UpdateWorkerRequestValidator()
        {
            RuleFor(w => w.WorkerId)
                .NotNull().WithMessage("Worker Id must be provided")
                .GreaterThan(0).WithMessage("Worker id must be greater than 0")
                .LessThan(int.MaxValue).WithMessage($"Worker id must be less than {int.MaxValue}");
            RuleFor(w => w.ModifiedBy)
                .NotNull().WithMessage("Created by email address must be provided")
                .MaximumLength(62).WithMessage("Created by email address must be no longer than 62 characters")
                .EmailAddress().WithMessage("Created by email address must be valid");
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
                .MaximumLength(200).WithMessage("Role provided is too long (more than 200 characters)");
            RuleFor(w => w.DateStart)
                .NotNull().WithMessage("Start date must be provided")
                .LessThan(DateTime.Now).WithMessage("Date cannot be set in the future");
        }
    }
}
