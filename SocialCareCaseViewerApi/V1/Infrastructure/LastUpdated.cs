using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SocialCareCaseViewerApi.V1.Infrastructure
{
    [Table("dm_person_last_updated", Schema = "dbo")]
    public class LastUpdated
    {
        [ForeignKey("PersonId")]
        public Person Person { get; set; }

        [Column("person_id")]
        [Required]
        public long PersonId { get; set; }

        [Column("person_last_updated_id")]
        [Key]
        public int Id { get; set; }

        [Column("type")]
        public string? Type { get; set; }

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }
    }
}
