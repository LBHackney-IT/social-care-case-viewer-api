using SocialCareCaseViewerApi.V1.Domain;

namespace SocialCareCaseViewerApi.V1.Boundary.Response
{
    public class Allocation
    {
        public long PersonId { get; set; }
        public string AllocatedWorker { get; set; }
        public string WorkerType { get; set; }
        public string AllocatedWorkerTeam { get; set; }
        public string AllocationStartDate { get; set; }
        public string AllocationEndDate { get; set; }
        public string CaseStatus { get; set; }
    }
}
