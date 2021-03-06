using System;
using System.Collections.Generic;
using SocialCareCaseViewerApi.V1.Infrastructure;

namespace SocialCareCaseViewerApi.V1.Domain
{
    public class WarningNote
    {
        public long Id { get; set; }
        public long PersonId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool DisclosedWithIndividual { get; set; }
        public string DisclosedDetails { get; set; }
        public string Notes { get; set; }
        public DateTime? ReviewDate { get; set; }
        public DateTime? NextReviewDate { get; set; }
        public string NoteType { get; set; }
        public string Status { get; set; }
        public DateTime? DisclosedDate { get; set; }
        public string DisclosedHow { get; set; }
        public string WarningNarrative { get; set; }
        public string ManagerName { get; set; }
        public DateTime? DiscussedWithManagerDate { get; set; }
        public string CreatedBy { get; set; }
        public List<WarningNoteReview> WarningNoteReviews { get; set; }
    }
}
