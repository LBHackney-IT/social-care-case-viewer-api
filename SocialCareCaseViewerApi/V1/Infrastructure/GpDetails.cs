using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SocialCareCaseViewerApi.V1.Infrastructure
{
    [Table("dm_gp_details", Schema = "dbo")]
    public class GpDetails
    {
        [ForeignKey("PersonId")]
        public Person Person { get; set; }

        [Column("person_id")]
        [Required]
        public long PersonId { get; set; }

        [Column("gp_details_id")]
        [Key]
        public int Id { get; set; }

        [Column("name")]
        [Required]
        public string? Name { get; set; }

        [Column("address")]
        [Required]
        public string? Address { get; set; } = null!;

        [Column("postcode")]
        [Required]
        public string? Postcode { get; set; } = null!;

        [Column("phone_nr")]
        [Required]
        public string? PhoneNr { get; set; } = null!;

        [Column("email")]
        [Required]
        public string? Email { get; set; } = null!;                    
    }
}
