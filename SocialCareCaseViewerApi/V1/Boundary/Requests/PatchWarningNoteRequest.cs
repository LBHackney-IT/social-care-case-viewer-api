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
    }
}
