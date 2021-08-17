using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SocialCareCaseViewerApi.V1.Infrastructure
{
    [Table("sccv_case_status_field_option", Schema = "dbo")]
    public class CaseStatusTypeFieldOption
    {
        [Column("id")]
        [MaxLength(16)]
        [Key]
        public long Id { get; set; }

        [Column("fk_sccv_case_status_field_id")]
        [MaxLength(16)]
        public long FieldID { get; set; }
        public CaseStatusTypeField TypeField { get; set; }

        [Column("name")]
        [MaxLength(256)]
        public string Name { get; set; }

        [Column("description")]
        [MaxLength(256)]
        public string Description { get; set; }
    }
}
