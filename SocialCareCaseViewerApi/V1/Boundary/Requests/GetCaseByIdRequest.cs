using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace SocialCareCaseViewerApi.V1.Boundary.Requests
{
    public class GetCaseByIdRequest
    {
        [FromRoute(Name = "record_id")]
        [Required]
        [StringLength(24, MinimumLength = 24, ErrorMessage = "The record_id must be 24 characters")]
        public string RecordId { get; set; }
    }
}
