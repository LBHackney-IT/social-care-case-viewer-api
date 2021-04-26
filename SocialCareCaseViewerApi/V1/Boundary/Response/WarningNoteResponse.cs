using System.Collections.Generic;

namespace SocialCareCaseViewerApi.V1.Boundary.Response
{
    public class WarningNoteResponse
    {
        public long Id { get; set; }
        public long PersonId { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public bool DisclosedWithIndividual { get; set; }
        public string DisclosedDetails { get; set; }
        public string Notes { get; set; }
        public string NextReviewDate { get; set; }
        public string NoteType { get; set; }
        public string Status { get; set; }
        public string DisclosedDate { get; set; }
        public string DisclosedHow { get; set; }
        public string WarningNarrative { get; set; }
        public string ManagerName { get; set; }
        public string DiscussedWithManagerDate { get; set; }
        public List<WarningNoteReviewResponse> WarningNoteReviews { get; set; }
    }

    public class WarningNoteReviewResponse
    {
        public long Id { get; set; }
        public long WarningNoteId { get; set; }
        public string ReviewDate { get; set; }
        public bool DisclosedWithIndividual { get; set; }
        public string Notes { get; set; }
        public string ManagerName { get; set; }
        public string DiscussedWithManagerDate { get; set; }
        public string CreatedAt { get; set; }
        public string CreatedBy { get; set; }
        public string LastModifiedAt { get; set; }
        public string LastModifiedBy { get; set; }
    }
}
