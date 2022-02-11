using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SocialCareCaseViewerApi.V1.Infrastructure
{
    [Table("dm_disability", Schema = "dbo")]
    public class Disability
    {
        [ForeignKey("PersonId")]
        public Person Person { get; set; }

        [Column("person_id")]
        [Required]
        public long PersonId { get; set; }

        [Column("disability_id")]
        [Key]
        public int Id { get; set; }

        [Column("disability_type")]
        [Required]
        public string? DisabilityType { get; set; }

    }
}
