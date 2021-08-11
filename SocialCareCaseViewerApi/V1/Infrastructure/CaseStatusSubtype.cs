using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SocialCareCaseViewerApi.V1.Infrastructure
{
    [Table("sccv_case_status_subtype", Schema = "dbo")]
    public class CaseStatusSubtype
    {
        [Column("id")]
        [MaxLength(16)]
        [Key]
        public long Id { get; set; }

        [Column("fk_case_status_type_id")]
        [MaxLength(16)]
        public long TypeID { get; set; }
        public CaseStatusType Type { get; set; }

        [Column("name")]
        [MaxLength(300)]
        public string Name { get; set; }

        [Column("description")]
        [MaxLength(16)]
        public string Description { get; set; }
    }
}
