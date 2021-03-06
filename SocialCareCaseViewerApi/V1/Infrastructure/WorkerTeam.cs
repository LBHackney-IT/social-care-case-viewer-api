using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SocialCareCaseViewerApi.V1.Infrastructure
{
    [Table("sccv_workerteam", Schema = "dbo")]
    public class WorkerTeam
    {
        [Key]
        [Column("id")]
        public int Id { get; set; }


        [Column("worker_id")]
        public int WorkerId { get; set; }
        public Worker Worker { get; set; }


        [Column("team_id")]
        public int TeamId { get; set; }
        public Team Team { get; set; }
    }
}
