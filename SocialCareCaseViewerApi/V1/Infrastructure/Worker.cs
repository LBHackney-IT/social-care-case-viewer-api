using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SocialCareCaseViewerApi.V1.Infrastructure
{
    [Table("sccv_worker", Schema = "dbo")]
    public class Worker
    {
        [Column("id")]
        [MaxLength(16)]
        [Key]
        public int Id { get; set; }

        [Column("email")]
        [MaxLength(62)]
        [Required]
        public string Email { get; set; }

        [Column("first_name")]
        [MaxLength(100)]
        [Required]
        public string FirstName { get; set; }

        [Column("last_name")]
        [MaxLength(100)]
        [Required]
        public string LastName { get; set; }

        [Column("role")]
        [MaxLength(200)]
        public string Role { get; set; }


        //nav props
        public ICollection<WorkerTeam> WorkerTeams { get; set; }

        public ICollection<AllocationSet> Allocations { get; set; }
    }
}
