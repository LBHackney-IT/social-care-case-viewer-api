using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace SocialCareCaseViewerApi.V1.Boundary.Requests
{
    public class GetPersonRequest
    {
        [Required]
        [Range(1, long.MaxValue, ErrorMessage = "Invalid id") ]
        [FromRoute]
        public long Id { get; set; }
    }
}
