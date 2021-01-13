using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

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

    }
}
