using System;

namespace SocialCareCaseViewerApi.V1.Boundary.Response
{
    public class Allocation
    {
        public int Id { get; set; }
        public long PersonId { get; set; }
        public string PersonName { get; set; }
        public string PersonAddress { get; set; }
        public DateTime? PersonDateOfBirth { get; set; }
        public string? AllocatedWorker { get; set; }
        public string WorkerType { get; set; }
        public string? AllocatedWorkerTeam { get; set; }
        public int? AllocatedWorkerTeamId { get; set; }
        public DateTime? AllocationStartDate { get; set; }
        public DateTime? TeamAllocationStartDate { get; set; }
        public DateTime? AllocationEndDate { get; set; }
        public string CaseStatus { get; set; }
        public string RagRating { get; set; }
        public DateTime? PersonReviewDate { get; set; }
    }
}
