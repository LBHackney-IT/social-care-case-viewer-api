using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace SocialCareCaseViewerApi.V1.Boundary.Requests
{
    public class ListCaseNotesRequest
    {
        [FromRoute]
        [Required]
        public string Id { get; set; }
    }
}
