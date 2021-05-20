using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

#nullable enable
namespace SocialCareCaseViewerApi.V1.Boundary.Requests
{
    public class GetCaseByIdRequest
    {
        [FromRoute]
        [Required]
        [StringLength(24, MinimumLength = 24, ErrorMessage = "The id must be 24 characters")]
        public string Id { get; set; } = null!;
    }
}
