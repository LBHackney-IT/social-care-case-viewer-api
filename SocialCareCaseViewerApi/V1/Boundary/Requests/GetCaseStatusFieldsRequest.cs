using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace SocialCareCaseViewerApi.V1.Boundary.Requests
{
    public class GetCaseStatusFieldsRequest
    {
        [Required(ErrorMessage = "Must specify a case status type.")]
        [FromRoute]
        public string Type { get; set; }
    }
}
