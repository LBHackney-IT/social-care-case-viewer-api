using System;
using System.Collections.Generic;
using SocialCareCaseViewerApi.V1.Infrastructure;

#nullable enable
namespace SocialCareCaseViewerApi.V1.Boundary.Response
{
    public class CaseSubmissionResponse
    {
        public string? SubmissionId { get; set; }
        public string FormId { get; set; } = null!;
        public WorkerResponse CreatedBy { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public List<Person> Residents { get; set; } = null!;
        public List<WorkerResponse> Workers { get; set; } = null!;
        public List<EditHistory<WorkerResponse>> EditHistory { get; set; } = null!;
        public SubmissionState SubmissionState { get; set; }

        // outer hashset string represents step id for form, inner hashset int represents questionId
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
