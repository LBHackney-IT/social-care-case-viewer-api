using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

#nullable enable
namespace SocialCareCaseViewerApi.V1.Infrastructure
{
    [Table("sccv_person_case_status_answers", Schema = "dbo")]
    public class CaseStatusAnswer : IAuditEntity
    {
        //TODO: add column lenghts
        [Column("id")]
        [Key]
        public long Id { get; set; }

        [Column("option")]
        public string? Option { get; set; }

        [Column("value")]
        public string? Value { get; set; }

        [Column("start_date")]
        public DateTime? StartDate { get; set; }

        [Column("fk_person_case_status_id")]
        public long CaseStatusId { get; set; }

        [ForeignKey("CaseStatusId")]
        public CaseStatus? CaseStatus { get; set; }

        [Column("discarded_at")]
        public DateTime? DiscardedAt { get; set; }

        [Column("group_id")]
        public String? GroupId { get; set; }

        [Column("end_date")]
        public DateTime? EndDate { get; set; }

        //audit props
        [Column("sccv_created_at")]
        public DateTime? CreatedAt { get; set; }

        [Column("sccv_created_by")]
        public string? CreatedBy { get; set; }

        [Column("sccv_last_modified_at")]
        public DateTime? LastModifiedAt { get; set; }

        [Column("sccv_last_modified_by")]
        public string? LastModifiedBy { get; set; }
    }
}
