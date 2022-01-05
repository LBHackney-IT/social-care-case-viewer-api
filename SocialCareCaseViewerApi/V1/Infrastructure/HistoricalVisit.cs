using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

#nullable enable
namespace SocialCareCaseViewerApi.V1.Infrastructure
{
    [Table("dm_visits", Schema = "dbo")]
    public class HistoricalVisit
    {
        [Column("visit_id")]
        [MaxLength(9)]
        [Required]
        [Key]
        public long VisitId { get; set; }

        [Column("person_id")]
        [MaxLength(16)]
        [Required]
        public long PersonId { get; set; }

        [Column("visit_type")]
        [StringLength(20)]
        [Required]
        public string VisitType { get; set; } = null!;

        [Column("planned_datetime")]
        public DateTime? PlannedDateTime { get; set; }

        [Column("actual_datetime")]
        public DateTime? ActualDateTime { get; set; }

        [Column("reason_not_planned")]
        [StringLength(16)]
        public string? ReasonNotPlanned { get; set; }

        [Column("reason_visit_not_made")]
        [StringLength(16)]
        public string? ReasonVisitNotMade { get; set; }

        [Column("seen_alone_flag")]
        [StringLength(1)]
        public string? SeenAloneFlag { get; set; }

        [Column("completed_flag")]
        [StringLength(1)]
        public string? CompletedFlag { get; set; }

        [Column("org_id")]
        [MaxLength(9)]
        public long? OrgId { get; set; }

        [Column("worker_id")]
        [MaxLength(9)]
        public long? WorkerId { get; set; }

        [Column("cp_registration_id")]
        [MaxLength(9)]
        public long? CpRegistrationId { get; set; }

        [Column("cp_visit_schedule_step_id")]
        [MaxLength(9)]
        public long? CpVisitScheduleStepId { get; set; }

        [Column("cp_visit_schedule_days")]
        [MaxLength(3)]
        public long? CpVisitScheduleDays { get; set; }

        [Column("cp_visit_on_time")]
        [StringLength(1)]
        public string? CpVisitOnTime { get; set; }

        // nav props
        public HistoricalWorker Worker { get; set; } = null!;
    }
}
