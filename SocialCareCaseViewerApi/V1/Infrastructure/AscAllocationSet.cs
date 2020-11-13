using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SocialCareCaseViewerApi.V1.Infrastructure
{
    [Table("asc_allocations", Schema = "dbo")]
    public class AscAllocationSet
    {
        [Column("mosaic_id"), Required]
        [MaxLength(16)]
        public long? Id { get; set; }

        [Column("last_name"), Required]
        [MaxLength(30)]
        public string LastName { get; set; }

        [Column("first_name"), Required]
        [MaxLength(30)]
        public string FirstName { get; set; }

        [Column("date_of_birth")]
        public DateTime? DateOfBirth { get; set; }

        [Column("age")]
        public int? Age { get; set; }

        [Column("primary_support_reason")]
        [MaxLength(100)]
        public string PrimarySupportReason { get; set; }

        [Column("allocated_team")]
        [MaxLength(50)]
        public string AllocatedTeam { get; set; }

        [Column("allocated_worker")]
        [MaxLength(90)]
        public string AllocatedWorker { get; set; }

        [Column("address")]
        [MaxLength(255)]
        public string Address { get; set; }

        [Column("post_code")]
        [MaxLength(10)]
        public string PostCode { get; set; }

        [Column("uprn")]
        public long? Uprn { get; set; }

        [Column("long_term_service")]
        [MaxLength(4)]
        public string LongTermService { get; set; }

        [Column("social_care_involvement")]
        [MaxLength(4)]
        public string SocialCareInvolvement { get; set; }

        [Column("short_term_support")]
        [MaxLength(4)]
        public string ShortTermSupport { get; set; }

        [Column("household_composition")]
        [MaxLength(50)]
        public string HouseholdComposition { get; set; }

        [Column("full_name"), Required]
        [MaxLength(62)]
        public string FullName { get; set; }

    }
}
