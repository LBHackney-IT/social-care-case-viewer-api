using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SocialCareCaseViewerApi.V1.Infrastructure
{
    [Table("sccv_person_case_status_field_option", Schema = "dbo")]
    public class CaseStatusFieldOption
    {
        [Column("id")]
        [MaxLength(16)]
        [Key]
        public long Id { get; set; }

        [Column("fk_person_case_status_id")]
        [MaxLength(16)]
        public long StatusId { get; set; }
        public CaseStatus Status { get; set; }

        [Column("fk_case_status_field_option_id")]
        [MaxLength(16)]
        public long FieldOptionId { get; set; }
        public CaseStatusTypeFieldOption FieldOption { get; set; }
    }
}
