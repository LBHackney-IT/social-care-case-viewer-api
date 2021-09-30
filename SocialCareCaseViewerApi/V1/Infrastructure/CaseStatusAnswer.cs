using System;
using System.ComponentModel.DataAnnotations.Schema;

#nullable enable
namespace SocialCareCaseViewerApi.V1.Infrastructure
{
    [Table("sccv_person_case_status_answers", Schema = "dbo")]
    public class CaseStatusAnswer
    {
        [Column("id")]
        public long id { get; set; }

        [Column("fk_person_case_status_id")]
        public long CaseStatusId { get; set; }

        public CaseStatus CaseStatus { get; set; }

        [Column("questions")]
        public string Question { get; set; }

        [Column("answers")]
        public string Answer { get; set; }

        [Column("start_date")]
        public DateTime StartDate { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }
    }
}
