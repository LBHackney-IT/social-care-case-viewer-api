using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json;
using SocialCareCaseViewerApi.V1.Boundary.Response;

namespace SocialCareCaseViewerApi.V1.Infrastructure

#nullable enable
{
    public class CaseSubmission
    {
        [JsonProperty("_id")]
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId SubmissionId { get; set; }
        public string FormId { get; set; } = null!;
        public Worker CreatedBy { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public Worker? SubmittedBy { get; set; }
        public DateTime? SubmittedAt { get; set; }
        public Worker? ApprovedBy { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public string? RejectionReason { get; set; }
        public List<Person> Residents { get; set; } = null!;
        public List<Worker> Workers { get; set; } = null!;
        public List<EditHistory<Worker>> EditHistory { get; set; } = null!;
        public SubmissionState SubmissionState { get; set; }

        public List<string>? Tags { get; set; }

        // outer hashset string represents step id for form
        // value represents JSON string of question ids (as stringified ints) to answers, answers in the format
        // either string, string[] or List<Dictionary<string,string>>
        public Dictionary<string, string> FormAnswers { get; set; } = null!;
    }

    public enum SubmissionState
    {
        InProgress,
        Submitted,
        Approved,
        Discarded,
        PanelApproved
    }
}
