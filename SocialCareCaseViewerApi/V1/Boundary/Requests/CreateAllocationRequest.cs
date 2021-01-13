using System.ComponentModel.DataAnnotations;

namespace SocialCareCaseViewerApi.V1.Boundary.Requests
{
    public class CreateAllocationRequest
    {
        [Required]
        [MaxLength(16)]
        public long MosaicId { get; set; }

        [MaxLength(50)]
        public string WorkerEmail { get; set; } = null;

        [MaxLength(62)]
        public string AllocatedWorkerTeam { get; set; }
    }
}
