using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;
using SocialCareCaseViewerApi.V1.Boundary.Response;

namespace SocialCareCaseViewerApi.V1.Infrastructure

#nullable enable
{
    public class CaseSubmission
    {
        [BsonId]
        public Guid SubmissionId { get; set; }

        public string FormId { get; set; } = null!;
        public Worker CreatedBy { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public List<Person> Residents { get; set; } = null!;
        public List<Worker> Workers { get; set; } = null!;
        public List<EditHistory<Worker>> EditHistory { get; set; } = null!;
        public SubmissionState SubmissionState { get; set; }

        // outer hashset int represents step id for form, inner hashset int represents questionId
        // object represents answer as either string, string[] or List<Dictionary<string,string>>
        public Dictionary<string, Dictionary<string, object>> FormAnswers { get; set; } = null!;
    }

    public enum SubmissionState
    {
        InProgress,
        Submitted
    }
}
