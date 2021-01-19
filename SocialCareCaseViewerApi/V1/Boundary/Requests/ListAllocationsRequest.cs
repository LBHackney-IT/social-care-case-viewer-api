using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace SocialCareCaseViewerApi.V1.Boundary.Requests
{
    public class ListAllocationsRequest
    {
        [FromQuery(Name = "mosaic_id")]
        [Required]
        [Range(1, long.MaxValue, ErrorMessage = "Please enter a value bigger than 0")]
        public long MosaicId { get; set; }
    }
}
