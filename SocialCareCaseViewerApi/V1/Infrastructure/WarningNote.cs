using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SocialCareCaseViewerApi.V1.Infrastructure
{
    [Table("sccv_warning_note", Schema = "dbo")]
    public class WarningNote : IAuditEntity
    {
        [Column("id")]
        [Key]
        public long Id { get; set; }

        [Column("person_id")]
        [Required]
        public long PersonId { get; set; }

        [Column("start_date")]
        public DateTime? StartDate { get; set; }

        [Column("review_date")]
        public DateTime? ReviewDate { get; set; }

        [Column("end_date")]
        public DateTime? EndDate { get; set; }

        [Column("individual_notified")]
        public bool DisclosedWithIndividual { get; set; }

        [Column("notification_details")]
        [MaxLength(1000)]
        public string DisclosedDetails { get; set; }

        [Column("review_details")]
        [MaxLength(1000)]
        public string Notes { get; set; }

        [Column("next_review_date")]
        public DateTime? NextReviewDate { get; set; }

        [Column("note_type")]
        [MaxLength(50)]
        public string NoteType { get; set; }

        [Column("status")]
        [MaxLength(50)]
        public string Status { get; set; }

        [Column("date_informed")]
        public DateTime? DisclosedDate { get; set; }

        [Column("how_informed")]
        [MaxLength(50)]
        public string DisclosedHow { get; set; }

        [Column("warning_narrative")]
        [MaxLength(1000)]
        public string WarningNarrative { get; set; }

        [Column("managers_name")]
        [MaxLength(100)]
        public string ManagerName { get; set; }

        [Column("date_manager_informed")]
        public DateTime? DiscussedWithManagerDate { get; set; }

        //nav props
        public Person Person { get; set; }

        //audit props
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

        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}
