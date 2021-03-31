using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace SocialCareCaseViewerApi.V1.Boundary.Requests
{
    public class GetWorkersRequest
    {
        [GetWorkersRequestValidator]
        [FromQuery(Name = "team_id")]
        public int TeamId { get; set; }

        [FromQuery(Name = "id")]
        public int WorkerId { get; set; }

        [FromQuery(Name = "email")]
        public string Email { get; set; }
    }

    public class GetWorkersRequestValidator : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var request = (GetWorkersRequest) validationContext.ObjectInstance;
            var workerId = request.WorkerId;
            var teamId = request.TeamId;
            var email = request.Email;

            if (workerId == 0 && teamId == 0 && string.IsNullOrEmpty(email))
            {
                return new ValidationResult($"Please provide either worker id, worker email or team id.");
            }

            return ValidationResult.Success;
        }
    }
}
