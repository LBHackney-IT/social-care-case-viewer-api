using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace SocialCareCaseViewerApi.V1.Boundary.Requests
{
    public class ListTeamsRequest
    {
        [FromQuery(Name = "context_flag")]
        [Required]
        [StringLength(1, MinimumLength = 1, ErrorMessage = "The context_flag must be 1 character")]
        [RegularExpression("(?i:^A|C)", ErrorMessage = "The context_flag must be either 'A' or 'C' only.")]
        public string ContextFlag { get; set; }
    }
}
