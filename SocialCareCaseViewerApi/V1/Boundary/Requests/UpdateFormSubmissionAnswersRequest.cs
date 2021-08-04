using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;

#nullable enable
namespace SocialCareCaseViewerApi.V1.Boundary.Requests
{
    public class UpdateFormSubmissionAnswersRequest
    {
        [FromBody]
        [JsonPropertyName("editedBy")]
        public string EditedBy { get; set; } = null!;

        [FromBody]
        [JsonPropertyName("stepAnswers")]
        public string StepAnswers { get; set; } = null!;

        [FromBody]
        [JsonPropertyName("dateOfEvent")]
        public DateTime? DateOfEvent { get; set; }

        [FromBody]
        [JsonPropertyName("title")]
        public string? Title { get; set; }
    }

    public class UpdateFormSubmissionAnswersValidator : AbstractValidator<UpdateFormSubmissionAnswersRequest>
    {
        public UpdateFormSubmissionAnswersValidator()
        {
            RuleFor(u => u.EditedBy)
                .NotNull().WithMessage("Provide who edited the submission")
                .EmailAddress().WithMessage("Provide a valid email address for who edited the submission");
            RuleFor(u => u.StepAnswers)
                .NotNull().WithMessage("Provide form answers");
            RuleFor(u => u.Title)
                .MinimumLength(1).When(u => u.Title != null).WithMessage("Title must have a length of at least 1");
        }
    }
}
