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

        [Column("end_date")]
        public DateTime? EndDate { get; set; }

         [Column("last_review_date")]
        public DateTime? LastReviewDate { get; set; }

         [Column("next_review_date")]
        public DateTime? NextReviewDate { get; set; }

        [Column("individual_notified")]
        public Boolean IndividualNotified { get; set; }

        [Column("notification_details")]
        public string NotificationDetails { get; set; }

        [Column("review_details")]
        public string ReviewDetails { get; set; }

        [Column("note_type")]
        public string NoteType { get; set; }

        [Column("status")]
        public string Status { get; set; }

        [Column("date_informed")]
        public DateTime? DateInformed { get; set; }

        [Column("how_informed")]
        public string HowInformed { get; set; }

        [Column("warning_narrative")]
        public string WarningNarrative { get; set; }

        [Column("managers_name")]
        public string ManagersName { get; set; }

        [Column("date_manager_informed")]
        public DateTime? DateManagerInformed { get; set; }

        //nav props
        public Person Person { get; set; }

        //audit props
        [Column("sccv_created_at")]
        public DateTime? CreatedAt { get; set; }

        [Column("sccv_created_by")]
        public string CreatedBy { get; set; }

        [Column("sccv_last_modified_at")]
        public DateTime? LastModifiedAt { get; set; }

        [Column("sccv_last_modified_by")]
        public string LastModifiedBy { get; set; }

        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}
