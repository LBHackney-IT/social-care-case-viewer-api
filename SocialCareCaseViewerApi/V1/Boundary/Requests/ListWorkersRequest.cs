using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace SocialCareCaseViewerApi.V1.Boundary.Requests
{
    public class ListWorkersRequest
    {
        [ListWorkersRequestValidator]
        [FromQuery(Name = "team_id")]
        // [Range(1, int.MaxValue, ErrorMessage = "Please enter a value bigger than 0")]
        public int TeamId { get; set; }

        [FromQuery(Name = "id")]
        // [Range(1, int.MaxValue, ErrorMessage = "Please enter a value bigger than 0")]
        public int WorkerId { get; set; }
    }

    public class ListWorkersRequestValidator : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var request = (ListWorkersRequest) validationContext.ObjectInstance;
            var workerId = request.WorkerId;
            var teamId = request.TeamId;

            if (workerId == 0 && teamId == 0)
            {
                return new ValidationResult($"Please provide either worker id or team id");
            }
            else if (workerId != 0 && teamId != 0)
            {
                return new ValidationResult($"Please provide only worker id or team id, but not both");
            }

            return ValidationResult.Success;
        }
    }
}
