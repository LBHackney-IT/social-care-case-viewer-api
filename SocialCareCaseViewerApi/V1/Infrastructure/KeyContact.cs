using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SocialCareCaseViewerApi.V1.Infrastructure
{
    [Table("dm_key_contacts", Schema = "dbo")]
    public class KeyContact
    {
        [ForeignKey("PersonId")]
        public Person Person { get; set; }

        [Column("person_id")]
        [Required]
        public long PersonId { get; set; }

        [Column("key_contact_id")]
        [Key]
        public int Id { get; set; }

        [Column("name")]
        [Required]
        [MaxLength(100)]
        public string? Name { get; set; }

        [Column("email")]
        [MaxLength(62)]
        [Required]
        public string? Email { get; set; } = null!;
    }
}
