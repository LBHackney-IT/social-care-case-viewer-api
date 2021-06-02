using System;
using System.Collections.Generic;
using SocialCareCaseViewerApi.V1.Infrastructure;

namespace SocialCareCaseViewerApi.V1.Domain
#nullable enable
{
    public class CaseSubmission
    {
        public Guid FormId { get; set; }
        public Domain.Worker CreatedBy { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public List<Person> Residents { get; set; } = null!;
        public List<Worker> Workers { get; set; } = null!;
        public List<(Worker, DateTime)> EditHistory { get; set; } = null!;
        public SubmissionState SubmissionState { get; set; }

        // outer hashset int represents step id for form, inner hashset int represents questionId, answer values stored as string[]
        public Dictionary<string, Dictionary<string, string[]>> FormAnswers { get; set; } = null!;
    }
}

