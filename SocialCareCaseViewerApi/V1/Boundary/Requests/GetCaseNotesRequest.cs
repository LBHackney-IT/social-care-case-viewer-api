using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace SocialCareCaseViewerApi.V1.Boundary.Requests
{
    public class GetCaseNotesRequest
    {
        [FromRoute]
        [Required]
        public string Id { get; set; }
    }
}
