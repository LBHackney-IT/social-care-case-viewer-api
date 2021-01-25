using System;

namespace SocialCareCaseViewerApi.V1.Boundary.Response
{
    public class Allocation
    {
        public int Id { get; set; }
        public long PersonId { get; set; }
        public string AllocatedWorker { get; set; }
        public string WorkerType { get; set; }
        public string AllocatedWorkerTeam { get; set; }
        public DateTime? AllocationStartDate { get; set; }
        public DateTime? AllocationEndDate { get; set; }
        public string CaseStatus { get; set; }
    }
}
