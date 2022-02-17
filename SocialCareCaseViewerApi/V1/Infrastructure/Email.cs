using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SocialCareCaseViewerApi.V1.Infrastructure
{
    [Table("dm_email_addresses", Schema = "dbo")]
    public class EmailAddress
    {
        [ForeignKey("PersonId")]
        public Person Person { get; set; }

        [Column("person_id")]
        [Required]
        public long PersonId { get; set; }

        [Column("email_id")]
        [Key]
        public int Id { get; set; }

        [Column("email")]
        [Required]
        public string? Email { get; set; }

        [Column("type")]
        [Required]
        public string? Type { get; set; }

    }
}
