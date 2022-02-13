using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SocialCareCaseViewerApi.V1.Infrastructure
{
    [Table("DM_OTHER_EMAIL_ADDRESSES", Schema = "dbo")]
    public class Email
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
        public string? EmailAddress { get; set; }

    }
}
