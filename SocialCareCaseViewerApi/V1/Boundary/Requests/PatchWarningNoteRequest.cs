using System;
using System.ComponentModel.DataAnnotations;

namespace SocialCareCaseViewerApi.V1.Boundary.Requests
{
    public class PatchWarningNoteRequest
    {
        [Required]
        public long WarningNoteId { get; set; }
        public DateTime? ReviewDate { get; set; }
        public string ReviewedBy { get; set; }
        public DateTime? NextReviewDate { get; set; }
        public string Status { get; set; }
        public DateTime? EndedDate { get; set; }
        public string EndedBy { get; set; }
        public string ReviewNotes { get; set; }
        public string ManagerName { get; set; }
        public DateTime? DiscussedWithManagerDate { get; set; }

        // warning note id - match with warning_notes
        // review date - can also be last review date - both warning_notes & review
        // review by - review table
        // next review date _ warning_notes
        // status - Warning_notes
        // ended date? - warning_note
        // ended by? _ warning_note
        // review notes _ review
        // discussed with manager - review
        // Discussed with manager date - review
    }
}