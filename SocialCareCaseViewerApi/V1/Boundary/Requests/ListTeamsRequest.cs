using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace SocialCareCaseViewerApi.V1.Boundary.Requests
{
    public class ListTeamsRequest
    {
        [FromQuery(Name = "context")]
        [Required]
        [StringLength(1, MinimumLength = 1, ErrorMessage = "The context must be 1 character")]
        [RegularExpression("(?i:^A|C)", ErrorMessage = "The context must be either 'A' or 'C' only.")]
        public string Context { get; set; }
    }
}
