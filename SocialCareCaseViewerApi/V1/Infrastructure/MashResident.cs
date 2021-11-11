using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

#nullable enable
namespace SocialCareCaseViewerApi.V1.Infrastructure
{
    [Table("ref_mash_residents", Schema = "dbo")]
    public class MashResident
    {
        //TODO: add column lenghts
        [Column("id")]
        [Key]
        public long Id { get; set; }

        [Column("first_name")]
        public string? FirstName { get; set; }

        [Column("last_name")]
        public string? LastName { get; set; }

        [Column("fk_ref_mash_referrals_id")]
        public string? ReferralId { get; set; }


    }
}
