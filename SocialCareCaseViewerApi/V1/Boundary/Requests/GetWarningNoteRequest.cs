using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace SocialCareCaseViewerApi.V1.Boundary.Requests
{
    public class GetWarningNoteRequest
    {
        [GetWarningNoteRequestValidator]
        [FromQuery(Name = "person_id")]
        public long PersonId { get; set; }
    }

    public class GetWarningNoteRequestValidator : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var request = (GetWarningNoteRequest) validationContext.ObjectInstance;
            var personId = request.PersonId;

            if (personId == 0)
            {
                return new ValidationResult($"Please provide person_id");
            }

            return ValidationResult.Success;
        }
    }
}
