using System;
using System.Collections.Generic;
using SocialCareCaseViewerApi.V1.Infrastructure;
using Worker = SocialCareCaseViewerApi.V1.Domain.Worker;

#nullable enable
namespace SocialCareCaseViewerApi.V1.Boundary.Response
{
    public class CaseSubmissionResponse
    {
        public string SubmissionId { get; set; } = null!;
        public string FormId { get; set; } = null!;
        public WorkerResponse CreatedBy { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public DateTime? DateOfEvent { get; set; }
        public WorkerResponse? SubmittedBy { get; set; }
        public DateTime? SubmittedAt { get; set; }
        public WorkerResponse? ApprovedBy { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public WorkerResponse? PanelApprovedBy { get; set; }
        public DateTime? PanelApprovedAt { get; set; }
        public string? RejectionReason { get; set; }
        public List<Person> Residents { get; set; } = null!;
        public List<WorkerResponse> Workers { get; set; } = null!;
        public List<EditHistory<WorkerResponse>> EditHistory { get; set; } = null!;
        public string SubmissionState { get; set; } = null!;
        public string? Title { get; set; }

        // outer hashset string represents step id for form
        // value represents JSON string of question ids (as stringified ints) to answers, answers in the format
        // either string, string[] or List<Dictionary<string,string>>
        public Dictionary<string, string> FormAnswers { get; set; } = null!;

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override bool Equals(object? obj)
        {
            return true;
        }
    }

    public class EditHistory<T> where T : class
    {
        public T Worker { get; set; } = null!;
        public DateTime EditTime { get; set; }
    }

}
