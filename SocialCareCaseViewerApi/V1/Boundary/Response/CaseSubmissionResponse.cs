using System;
using System.Collections.Generic;
using SocialCareCaseViewerApi.V1.Infrastructure;

#nullable enable
namespace SocialCareCaseViewerApi.V1.Boundary.Response
{
    public class CaseSubmissionResponse
    {
        public Guid SubmissionId { get; set; }
        public string FormId { get; set; } = null!;
        public WorkerResponse CreatedBy { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public List<Person> Residents { get; set; } = null!;
        public List<WorkerResponse> Workers { get; set; } = null!;
        public List<EditHistory<WorkerResponse>> EditHistory { get; set; } = null!;
        public SubmissionState SubmissionState { get; set; }

        // outer hashset int represents step id for form, inner hashset int represents questionId
        // object represents answer as either string, string[] or List<Dictionary<string,string>>
        public Dictionary<string, Dictionary<string, object>> FormAnswers { get; set; } = null!;

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
