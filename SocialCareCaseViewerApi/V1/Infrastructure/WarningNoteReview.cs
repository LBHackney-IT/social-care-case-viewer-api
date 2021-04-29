using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SocialCareCaseViewerApi.V1.Infrastructure
{
    [Table("sccv_warning_note_review", Schema = "dbo")]
    public class WarningNoteReview : IAuditEntity
    {
        [Column("id")]
        [Key]
        public long Id { get; set; }

        [Column("warning_note_id")]
        [Required]
        public long WarningNoteId { get; set; }

        [Column("review_date")]
        public DateTime? ReviewDate { get; set; }

        [Column("individual_notified")]
        public bool DisclosedWithIndividual { get; set; }

        [Column("notes")]
        [MaxLength(1000)]
        public string Notes { get; set; }

        [Column("managers_name")]
        [MaxLength(100)]
        public string ManagerName { get; set; }

        [Column("date_manager_informed")]
        public DateTime? DiscussedWithManagerDate { get; set; }

        // audit props
        [Column("sccv_created_at")]
        public DateTime? CreatedAt { get; set; }

        [Column("sccv_created_by")]
        [MaxLength(300)]
        public string CreatedBy { get; set; }

        [Column("sccv_last_modified_at")]
        public DateTime? LastModifiedAt { get; set; }

        [Column("sccv_last_modified_by")]
        [MaxLength(300)]
        public string LastModifiedBy { get; set; }
    }
}
