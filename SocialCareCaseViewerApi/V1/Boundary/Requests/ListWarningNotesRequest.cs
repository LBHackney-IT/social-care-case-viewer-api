using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace SocialCareCaseViewerApi.V1.Boundary.Requests
{
    public class ListWarningNotesRequest
    {
        [ListWarningNotesRequestValidator]
        [FromQuery(Name = "person_id")]
        public long PersonId { get; set; }
    }

    public class ListWarningNotesRequestValidator : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var request = (ListWarningNotesRequest) validationContext.ObjectInstance;
            var personId = request.PersonId;

            if (personId == 0)
            {
                return new ValidationResult($"Please provide person_id");
            }

            return ValidationResult.Success;
        }
    }
}