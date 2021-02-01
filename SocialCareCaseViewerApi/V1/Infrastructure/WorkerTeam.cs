using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SocialCareCaseViewerApi.V1.Infrastructure
{
    public class WorkerTeam
    {
        [Key]
        public int Id { get; set; }


        [Column("worker_id")]
        public int WorkerId { get; set; }

        [Column("team_id")]
        public int TeamId { get; set; }

        public Worker Worker { get; set; }

        public Team Team { get; set; }
    }
}
