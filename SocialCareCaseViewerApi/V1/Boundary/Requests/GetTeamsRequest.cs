using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace SocialCareCaseViewerApi.V1.Boundary.Requests
{
    public class GetTeamsRequest
    {
        [FromQuery(Name = "context_flag")]
        [Required]
        [StringLength(1, MinimumLength = 1, ErrorMessage = "The context_flag must be 1 character")]
        [RegularExpression("(?i:^A|B|C)", ErrorMessage = "The context_flag must be 'A', 'B' or 'C' only.")]
        public string ContextFlag { get; set; }
    }
}
