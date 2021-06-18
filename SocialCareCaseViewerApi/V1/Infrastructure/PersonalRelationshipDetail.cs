using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SocialCareCaseViewerApi.V1.Infrastructure
{
    [Table("sccv_personal_relationship_detail", Schema = "dbo")]
    public class PersonalRelationshipDetail
    {
        [Column("id")]
        [MaxLength(16)]
        [Key]
        public long Id { get; set; }

        [Column("fk_personal_relationship_id")]
        [MaxLength(16)]
        public long PersonalRelationshipId { get; set; }

        [Column("details")]
        [MaxLength(100)]
        public string Details { get; set; }

        //audit props
        [Column("sccv_created_at")]
        public DateTime? CreatedAt { get; set; }

        [Column("sccv_created_by")]
        public string CreatedBy { get; set; }

        [Column("sccv_last_modified_at")]
        public DateTime? LastModifiedAt { get; set; }

        [Column("sccv_last_modified_by")]
        public string LastModifiedBy { get; set; }
    }
}
