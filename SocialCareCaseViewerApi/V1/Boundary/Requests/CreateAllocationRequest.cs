using System.ComponentModel.DataAnnotations;

namespace SocialCareCaseViewerApi.V1.Boundary.Requests
{
    public class CreateAllocationRequest
    {
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Please enter a value bigger than 0")]
        public long MosaicId { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Please enter a value bigger than 0")]
        public int AllocatedWorkerId { get; set; }

        [Required]
        public string AllocatedBy { get; set; }

        [Required]
        public string CreatedBy { get; set; }
    }
}
