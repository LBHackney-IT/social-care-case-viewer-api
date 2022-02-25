using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SocialCareCaseViewerApi.V1.Infrastructure
{
    [Table("sccv_residentworker", Schema = "dbo")]
    public class ResidentWorker
    {
        [Key]
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [ForeignKey("PersonId")]
        public Person Person { get; set; }

        [Column("person_id")]
        [Required]
        public long PersonId { get; set; }

        [ForeignKey("TeamId")]
        public Team Team { get; set; }

        [Column("team_id")]
        [Required]
        public int TeamId { get; set; }

        [Column("worker_id")]
        public int WorkerId { get; set; }
        public Worker Worker { get; set; }

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }
    }
}
