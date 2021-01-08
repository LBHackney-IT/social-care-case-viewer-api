using System.ComponentModel.DataAnnotations;

namespace SocialCareCaseViewerApi.V1.Boundary.Requests
{
    public class AddNewAllocationRequest
    {
        public long MosaicId { get; set; }
        [Required]

        public string WorkerEmail { get; set; } = null;

        public string AllocatedWorkerTeam { get; set; }
    }
}