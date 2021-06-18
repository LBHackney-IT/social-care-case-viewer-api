using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SocialCareCaseViewerApi.V1.Infrastructure
{
    [Table("sccv_personal_relationship", Schema = "dbo")]
    public class PersonalRelationship : IAuditEntity
    {
        [Column("id")]
        [MaxLength(16)]
        [Key]
        public long Id { get; set; }

        [Column("fk_person_id")]
        [MaxLength(16)]
        public long PersonId { get; set; }

        [Column("fk_other_person_id")]
        [MaxLength(16)]
        public long OtherPersonId { get; set; }

        [Column("fk_personal_relationship_type_id")]
        [MaxLength(16)]
        public long PersonalRelationshipTypeId { get; set; }

        [Column("start_date")]
        public DateTime StartDate { get; set; }

        [Column("end_date")]
        public DateTime? EndDate { get; set; }

        [Column("is_informal_carer")]
        [MaxLength(1)]
        public string IsInformalCarer { get; set; }

        [Column("parental_responsibility")]
        [MaxLength(1)]
        public string ParentalResponsibility { get; set; }

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
