using System;
using System.Collections.Generic;
using SocialCareCaseViewerApi.V1.Infrastructure;

#nullable enable
namespace SocialCareCaseViewerApi.V1.Boundary.Response
{
    public class CaseSubmissionResponse
    {
        public Guid SubmissionId { get; set; }
        public int FormId { get; set; }
        public WorkerResponse CreatedBy { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public List<Person> Residents { get; set; } = null!;
        public List<WorkerResponse> Workers { get; set; } = null!;
        public List<EditHistory<WorkerResponse>> EditHistory { get; set; } = null!;
        public SubmissionState SubmissionState { get; set; }

        // outer hashset key represents step id for form, inner hashset key represents questionId, answer values stored as string[]
        public Dictionary<string, Dictionary<string, string[]>> FormAnswers { get; set; } = null!;
    }

    public class EditHistory<T> where T : class
    {
        public T Worker { get; set; } = null!;
        public DateTime EditTime { get; set; }
    }

}
