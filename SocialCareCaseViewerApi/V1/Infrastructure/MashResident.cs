using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

#nullable enable
namespace SocialCareCaseViewerApi.V1.Infrastructure
{

    [Table("ref_mash_residents", Schema = "dbo")]
    public class MashResident
    {
        [Column("id")]
        [MaxLength(16)]
        [Key]
        public long Id { get; set; }

        [Column("first_name")]
        public string FirstName { get; set; } = null!;

        [Column("last_name")]
        public string LastName { get; set; } = null!;

        [Column("dob")]
        public DateTime? DateOfBirth { get; set; }

        [Column("gender")]
        public string? Gender { get; set; }

        [Column("ethnicity")]
        public string? Ethnicity { get; set; }

        [Column("first_language")]
        public string? FirstLanguage { get; set; }

        [Column("school")]
        public string? School { get; set; }

        [Column("address")]
        public string? Address { get; set; }

        [Column("postcode")]
        public string? Postcode { get; set; }

        [Column("fk_ref_mash_referrals_id")]
        public long MashReferralId { get; set; }

        [ForeignKey("MashReferralId")]
        public MashReferral MashReferral { get; set; }

        [Column("sccv_residents_id")]
        public long? SocialCareId { get; set; }
    }
}
