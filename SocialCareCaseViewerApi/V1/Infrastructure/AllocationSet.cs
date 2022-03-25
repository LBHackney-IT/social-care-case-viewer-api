using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SocialCareCaseViewerApi.V1.Infrastructure
{
    [Table("sccv_allocations_combined", Schema = "dbo")]
    public class AllocationSet : IAuditEntity
    {
        [Column("id")]
        [Key]
        public int Id { get; set; }

        [Column("mosaic_id")]
        [Required]
        public long PersonId { get; set; }

        [Column("worker_id")]
        [MaxLength(62)]
        public int? WorkerId { get; set; }

        [Column("team_id")]
        [MaxLength(62)]
        public int? TeamId { get; set; }

        [Column("rag_rating")]
        public string? RagRating { get; set; }

        [Column("summary")]
        public string? Summary { get; set; }

        [Column("care_package")]
        public string? CarePackage { get; set; }

        [Column("allocation_start_date")]
        public DateTime? AllocationStartDate { get; set; }

        [Column("allocation_end_date")]
        public DateTime? AllocationEndDate { get; set; }

        [Column("case_status")]
        public string CaseStatus { get; set; }

        [Column("closure_date_if_closed")]
        public DateTime? CaseClosureDate { get; set; }

        [Column("marked_for_deletion")]
        public bool MarkedForDeletion { get; set; }

        //nav props
        public Worker Worker { get; set; }
        public Team Team { get; set; }

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
