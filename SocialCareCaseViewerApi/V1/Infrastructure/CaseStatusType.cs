using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SocialCareCaseViewerApi.V1.Infrastructure
{
    [Table("sccv_case_status_type", Schema = "dbo")]
    public class CaseStatusType
    {
        [Column("id")]
        [MaxLength(16)]
        [Key]
        public long Id { get; set; }

        [Column("name")]
        [MaxLength(300)]
        public string Name { get; set; }

        [Column("description")]
        [MaxLength(16)]
        public string Description { get; set; }
    }
}
