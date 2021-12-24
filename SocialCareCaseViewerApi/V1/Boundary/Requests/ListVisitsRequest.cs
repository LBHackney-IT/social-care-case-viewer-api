using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace SocialCareCaseViewerApi.V1.Boundary.Requests
{
    public class ListVisitsRequest
    {
        [FromRoute]
        [Required]
        [Range(1, long.MaxValue)]
        public long Id { get; set; }
    }
}
