using System;
using System.Collections.Generic;
using SocialCareCaseViewerApi.V1.Boundary.Response;
using SocialCareCaseViewerApi.V1.Infrastructure;

namespace SocialCareCaseViewerApi.V1.Domain
#nullable enable
{
    public class CaseSubmission
    {
        public string SubmissionId { get; set; } = null!;
        public string FormId { get; set; } = null!;
        public Worker CreatedBy { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public DateTime? DateOfEvent { get; set; }
        public Worker? SubmittedBy { get; set; }
        public DateTime? SubmittedAt { get; set; }
        public Worker? ApprovedBy { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public Worker? PanelApprovedBy { get; set; }
        public DateTime? PanelApprovedAt { get; set; }
        public string? RejectionReason { get; set; }
        public List<Person> Residents { get; set; } = null!;
        public List<Worker> Workers { get; set; } = null!;
        public List<EditHistory<Worker>>? EditHistory { get; set; } = null!;
        public string SubmissionState { get; set; } = null!;
        public string? Title { get; set; }
        public DateTime? LastEdited { get; set; }
        public int? CompletedSteps { get; set; }

        // outer hashset string represents step id for form
        // value represents JSON string of question ids (as stringified ints) to answers, answers in the format
        // either string, string[] or List<Dictionary<string,string>>
        public Dictionary<string, string>? FormAnswers { get; set; } = null!;
    }
}

