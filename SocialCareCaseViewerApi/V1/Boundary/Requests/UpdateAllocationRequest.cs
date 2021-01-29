using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace SocialCareCaseViewerApi.V1.Boundary.Requests
{

    public class UpdateAllocationRequest
    {
        [FromBody]
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Please enter a value bigger than 0")]
        public int Id { get; set; }

        [FromBody]
        [Required(ErrorMessage = "Please provide deallocation reason")]
        public string DeallocationReason { get; set; }

        [FromBody]
        [Required]
        public string AllocationId { get; set; }

        [FromBody]
        [Required]
        public string CreatedBy { get; set; }
    }
}
