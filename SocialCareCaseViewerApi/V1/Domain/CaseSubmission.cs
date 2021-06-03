using System;
using System.Collections.Generic;
using SocialCareCaseViewerApi.V1.Boundary.Response;
using SocialCareCaseViewerApi.V1.Infrastructure;

namespace SocialCareCaseViewerApi.V1.Domain
#nullable enable
{
    public class CaseSubmission
    {
        public Guid SubmissionId { get; set; }
        public int FormId { get; set; }
        public Domain.Worker CreatedBy { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public List<Person> Residents { get; set; } = null!;
        public List<Worker> Workers { get; set; } = null!;
        public List<EditHistory<Worker>> EditHistory { get; set; } = null!;
        public SubmissionState SubmissionState { get; set; }

        // outer hashset key represents step id for form, inner hashset key represents questionId, answer values stored as string[]
        public Dictionary<string, Dictionary<string, string[]>> FormAnswers { get; set; } = null!;
    }
}

