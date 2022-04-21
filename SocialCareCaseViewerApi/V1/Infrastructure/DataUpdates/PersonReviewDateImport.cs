using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

#nullable enable
namespace SocialCareCaseViewerApi.V1.Infrastructure.DataUpdates
{
    [Table("sccv_person_review_date_import_review_team", Schema = "dbo")]
    public class PersonReviewDateImport
    {
        [Column("id")]
        [MaxLength(16)]
        [Key]
        public long Id { get; set; }

        [Column("mosaic_id")]
        public long MosaicId { get; set; }

        [Column("review_date")]
        public DateTime? Reviewdate { get; set; }
    }
}
