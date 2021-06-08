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
        public Dictionary<string, object>
        StepAnswers
        { get; set; } = null!;
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
        }
    }
}
