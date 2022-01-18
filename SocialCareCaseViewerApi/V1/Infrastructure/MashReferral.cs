using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

#nullable enable
namespace SocialCareCaseViewerApi.V1.Infrastructure
{
    [Table("ref_mash_referrals", Schema = "dbo")]
    public class MashReferral : IAuditEntity
    {
        [Column("id")]
        [MaxLength(16)]
        [Key]
        public long Id { get; set; }

        [Column("worker_id")]
        public int? WorkerId { get; set; }

        [ForeignKey("WorkerId")]
        public Worker? Worker { get; set; }

        [Column("referrer")]
        public string Referrer { get; set; } = null!;

        [Column("requested_support")]
        public string RequestedSupport { get; set; } = null!;

        [Column("created_at")]
        public DateTime ReferralCreatedAt { get; set; }

        [Column("referral_doc_url")]
        public string ReferralDocumentURI { get; set; } = null!;

        [Column("stage")]
        public string Stage { get; set; } = null!;

        [Column("referral_category")]
        public string? ReferralCategory { get; set; }

        [Column("contact_decision_created_at")]
        public DateTime? ContactDecisionCreatedAt { get; set; }

        [Column("contact_decision_urgent_contact")]
        public bool? ContactDecisionUrgentContactRequired { get; set; }

        [Column("initial_decision")]
        public string? InitialDecision { get; set; }
        [Column("initial_decision_referral_category")]
        public string? InitialDecisionReferralCategory { get; set; }
        [Column("initial_decision_urgent_contact")]
        public bool? InitialDecisionUrgentContactRequired { get; set; }

        [Column("initial_decision_created_at")]
        public DateTime? InitialDecisionCreatedAt { get; set; }

        [Column("screening_decision")]
        public string? ScreeningDecision { get; set; }

        [Column("screening_decision_urgent_contact")]
        public bool? ScreeningUrgentContactRequired { get; set; }

        [Column("screening_decision_created_at")]
        public DateTime? ScreeningCreatedAt { get; set; }

        [Column("final_decision")]
        public string? FinalDecision { get; set; }

        [Column("final_decision_referral_category")]
        public string? FinalDecisionReferralCategory { get; set; }

        [Column("final_decision_urgent_contact")]
        public bool? FinalDecisionUrgentContactRequired { get; set; }

        [Column("final_decision_created_at")]
        public DateTime? FinalDecisionCreatedAt { get; set; }

        //audit fields
        [Column("sccv_created_at")]
        public DateTime? CreatedAt { get; set; }

        [Column("sccv_created_by")]
        public string? CreatedBy { get; set; }

        [Column("sccv_last_modified_at")]
        public DateTime? LastModifiedAt { get; set; }

        [Column("sccv_last_modified_by")]
        public string? LastModifiedBy { get; set; }

        public ICollection<MashResident>? MashResidents { get; set; }


    }
}
