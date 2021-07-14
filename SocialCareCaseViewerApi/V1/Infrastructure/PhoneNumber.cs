using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SocialCareCaseViewerApi.V1.Infrastructure
{
    [Table("dm_telephone_numbers", Schema = "dbo")]
    public class PhoneNumber : IAuditEntity
    {
        [ForeignKey("PersonId")]
        public Person Person { get; set; }

        [Column("telephone_number_id")]
        [Key]
        public int Id { get; set; }

        [Column("person_id")]
        [Required]
        public long PersonId { get; set; }

        [Column("telephone_number")]
        [MaxLength(32)]
        public string Number { get; set; } //TODO: tidy up data/migrate so it has only numbers in this field

        [Column("telephone_number_type")]
        [MaxLength(80)]
        [Required]
        public string Type { get; set; }

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

