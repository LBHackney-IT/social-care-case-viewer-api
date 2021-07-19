using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace SocialCareCaseViewerApi.V1.Boundary.Requests
{
    public class GetCaseNotesRequest
    {
        [FromRoute]
        [Required]
        public string Id { get; set; }

        public string UserId { get; set; }

        public bool AuditingEnabled { get; set; }

        public string ResidentId { get; set; }
    }
}
