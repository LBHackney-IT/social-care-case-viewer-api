using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SocialCareCaseViewerApi.V1.Infrastructure
{
    [Table("sccv_personal_relationship_type", Schema = "dbo")]
    public class PersonalRelationshipType
    {
        [Column("id")]
        [MaxLength(16)]
        [Key]
        public long Id { get; set; }

        [Column("description")]
        [MaxLength(300)]
        public string Description { get; set; }

        [Column("inverse_type_id")]
        [MaxLength(16)]
        public long InverseTypeId { get; set; }
    }
}
