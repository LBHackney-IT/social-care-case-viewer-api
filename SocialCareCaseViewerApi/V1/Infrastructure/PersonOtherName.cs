using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SocialCareCaseViewerApi.V1.Infrastructure
{
    [Table("sccv_person_other_name", Schema = "dbo")]
    public class PersonOtherName : IAuditEntity
    {
        [ForeignKey("PersonId")]
        public Person Person { get; set; }

        [Column("id")]
        [Key]
        public int Id { get; set; }

        [Column("person_id")]
        public long PersonId { get; set; }

        [Column("first_name")]
        [MaxLength(100)]
        public string FirstName { get; set; }

        [Column("last_name")]
        [MaxLength(100)]
        public string LastName { get; set; }

        [Column("marked_for_deletion")]
        public bool MarkedForDeletion { get; set; }

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
