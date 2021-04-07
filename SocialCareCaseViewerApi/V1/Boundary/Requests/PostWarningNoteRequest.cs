using System;
using System.ComponentModel.DataAnnotations;

namespace SocialCareCaseViewerApi.V1.Boundary.Requests
{
    public class PostWarningNoteRequest
    {
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Please enter a value bigger than 0")]
        public long PersonId { get; set; }

        [Required]
        public DateTime? StartDate { get; set; }

        [Required]
        public DateTime? ReviewDate { get; set; }

        public DateTime? EndDate { get; set; }

        [Required]
        public Boolean DisclosedWithIndividual { get; set; }

        [StringLength(1000, ErrorMessage = "Character limit of 1000 exceeded")]
        public string DisclosedDetails { get; set; }

        [Required]
        [StringLength(1000, ErrorMessage = "Character limit of 1000 exceeded")]
        public string Notes { get; set; }

        [StringLength(50, ErrorMessage = "Character limit of 50 exceeded")]
        public string NoteType { get; set; }

        [StringLength(50, ErrorMessage = "Character limit of 50 exceeded")]
        public string Status { get; set; }

        public DateTime? DisclosedDate { get; set; }

        [StringLength(50, ErrorMessage = "Character limit of 50 exceeded")]
        public string DisclosedHow { get; set; }

        [StringLength(1000, ErrorMessage = "Character limit of 1000 exceeded")]
        public string WarningNarrative { get; set; }

        [StringLength(100, ErrorMessage = "Character limit of 100 exceeded")]
        public string ManagerName { get; set; }
        public DateTime? DiscussedWithManagerDate { get; set; }

        [Required]
        [EmailAddress]
        public string CreatedBy { get; set; }
    }
}
