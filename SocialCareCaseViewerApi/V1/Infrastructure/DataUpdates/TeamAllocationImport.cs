using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

#nullable enable
namespace SocialCareCaseViewerApi.V1.Infrastructure.DataUpdates
{
    [Table("sccv_team_allocations_import_review_team", Schema = "dbo")]
    public class TeamAllocationImport
    {
        [Column("id")]
        [MaxLength(16)]
        [Key]
        public long Id { get; set; }

        [Column("mosaic_id")]
        public long MosaicId { get; set; }

        [Column("referral_date")]
        public DateTime? ReferralDate { get; set; }

        [Column("team_id")]
        public int TeamId { get; set; }
    }
}
