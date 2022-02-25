using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SocialCareCaseViewerApi.V1.Infrastructure
{
    [Table("sccv_residentteam", Schema = "dbo")]
    public class ResidentTeam
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

        [Column("rag_rating")]
        public string RagRating { get; set; }

        [Column("summary")]
        public string Summary { get; set; }

        [Column("care_package")]
        public string CarePackage { get; set; }

        [Column("updated_at")]
        public DateTime AllocationDate { get; set; }

        [ForeignKey("CreatedBy")]
        public Worker Worker { get; set; }

        [Column("created_by")]
        public int CreatedBy { get; set; }

    }
}
