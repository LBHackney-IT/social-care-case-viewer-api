using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace SocialCareCaseViewerApi.V1.Boundary.Requests
{
    public class ListRelationshipsRequest
    {
        [Required]
        [Range(1, long.MaxValue, ErrorMessage = "Please enter a valid personId")]
        [FromRoute]
        public long PersonId { get; set; }
    }
}
