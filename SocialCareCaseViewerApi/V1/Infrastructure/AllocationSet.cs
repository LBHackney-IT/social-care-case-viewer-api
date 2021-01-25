using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SocialCareCaseViewerApi.V1.Infrastructure
{
    [Table("sccv_allocations_combined", Schema = "dbo")]
    public class AllocationSet
    {
        [Column("id")]
        [Key]
        public int Id { get; set; }

        [Column("mosaic_id")]
        [Required]
        public long MosaicId { get; set; }

        [Column("worker_id")]
        [MaxLength(62)]
        public long? WorkerId { get; set; }

        [Column("allocation_start_date")]
        public DateTime? AllocationStartDate { get; set; }

        [Column("allocation_end_date")]
        public DateTime? AllocationEndDate { get; set; }

        [Column("case_status")]
        public string CaseStatus { get; set; }

        [Column("closure_date_if_closed")]
        public DateTime? CaseClosureDate { get; set; }

        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}
