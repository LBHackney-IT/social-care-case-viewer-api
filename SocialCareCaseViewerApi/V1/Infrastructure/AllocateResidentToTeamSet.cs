using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SocialCareCaseViewerApi.V1.Infrastructure
{
    [Table("sccv_residentteam", Schema = "dbo")]
    public class AllocateResidentToTheTeam : IAuditEntity
    {
        [Column("id")]
        [Key]
        public int Id { get; set; }

        [Column("rag_rating")]
        public string? RagRating { get; set; }

        [Column("team_id")]
        [MaxLength(62)]
        public int? TeamId { get; set; }

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
