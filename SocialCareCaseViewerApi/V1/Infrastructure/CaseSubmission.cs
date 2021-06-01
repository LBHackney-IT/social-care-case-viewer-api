using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;

namespace SocialCareCaseViewerApi.V1.Infrastructure

#nullable enable
{
    public class CaseSubmission
    {
        [BsonId]
        public Guid FormId { get; set; }

        public Worker CreatedBy { get; set; } = null!;

        public DateTime CreatedAt { get; set; }

        public List<Person> Residents { get; set; } = null!;

        public List<Worker> Workers { get; set; } = null!;

        public List<(Worker, DateTime)> EditHistory { get; set; } = null!;

        public DateTime? IsEditable { get; set; }

        public DateTime? IsSubmitted { get; set; }

        // outer hashset int represents step id for form, inner hashset int represents questionId, answer value stored as string
        public Dictionary<int, Dictionary<int, string>> FormAnswers { get; set; } = null!;
    }
}
