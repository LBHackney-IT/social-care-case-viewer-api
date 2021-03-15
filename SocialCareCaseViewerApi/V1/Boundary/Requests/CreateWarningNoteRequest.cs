using System;
using System.ComponentModel.DataAnnotations;

namespace SocialCareCaseViewerApi.V1.Boundary.Requests
{
    public class CreateWarningNoteRequest
    {
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Please enter a value bigger than 0")]
        public long PersonId { get; set; }

        [Required]
        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        [Required]
        public Boolean IndividualNotified { get; set; }

        [StringLength(1000, ErrorMessage = "Character limit of 1000 exceeded")]
        public string NotificationDetails { get; set; }

        [StringLength(1000, ErrorMessage = "Character limit of 1000 exceeded")]
        public string ReviewDetails { get; set; }

        [StringLength(50, ErrorMessage = "Character limit of 50 exceeded")]
        public string NoteType { get; set; }

        [StringLength(50, ErrorMessage = "Character limit of 50 exceeded")]
        public string Status { get; set; }

        public DateTime? DateInformed { get; set; }

        [StringLength(50, ErrorMessage = "Character limit of 50 exceeded")]
        public string HowInformed { get; set; }

        [StringLength(1000, ErrorMessage = "Character limit of 1000 exceeded")]
        public string WarningNarrative { get; set; }

        [StringLength(100, ErrorMessage = "Character limit of 100 exceeded")]
        public string ManagersName { get; set; }
        public DateTime? DateManagerInformed { get; set; }
    }
}
