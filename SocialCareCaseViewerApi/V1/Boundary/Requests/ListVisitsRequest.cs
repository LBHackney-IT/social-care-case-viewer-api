using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace SocialCareCaseViewerApi.V1.Boundary.Requests
{
    public class ListVisitsRequest
    {
        [FromRoute]
        [Required]
        public string Id { get; set; }
    }
}
