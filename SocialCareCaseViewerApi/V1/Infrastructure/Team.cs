using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SocialCareCaseViewerApi.V1.Infrastructure
{
    [Table("sccv_team", Schema = "dbo")]
    public class Team
    {
        [Column("id")]
        [MaxLength(16)]
        [Key]
        public int Id { get; set; }

        [Column("name")]
        [Required]
        [MaxLength(200)]
        public string Name { get; set; }

        [Column("context")]
        [MaxLength(1)]
        public string Context { get; set; }

        public ICollection<WorkerTeam> WorkerTeams { get; set; }
    }
}
