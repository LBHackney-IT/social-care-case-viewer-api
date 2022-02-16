using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SocialCareCaseViewerApi.V1.Infrastructure
{
    [Table("dm_tech_use", Schema = "dbo")]
    public class TechUse
    {
        [ForeignKey("PersonId")]
        public Person Person { get; set; }

        [Column("person_id")]
        [Required]
        public long PersonId { get; set; }

        [Column("tech_use_id")]
        [Key]
        public int Id { get; set; }

        [Column("tech_type")]
        [Required]
        public string? TechType { get; set; }

    }
}
