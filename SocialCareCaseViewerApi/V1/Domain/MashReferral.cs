using System;
using System.Collections.Generic;

#nullable enable
namespace SocialCareCaseViewerApi.V1.Domain
{
    public class MashReferral
    {
        public long Id { get; set; }
        public string Referrer { get; set; } = null!;
        public string RequestedSupport { get; set; } = null!;
        public DateTime ReferralCreatedAt { get; set; }
        public string ReferralDocumentURI { get; set; } = null!;
        public string Stage { get; set; } = null!;
        public string? ReferralCategory { get; set; }
        public DateTime? ContactDecisionCreatedAt { get; set; }
        public bool? ContactDecisionUrgentContactRequired { get; set; }
        public string? InitialDecision { get; set; }
        public string? InitialDecisionReferralCategory { get; set; }
        public bool? InitialDecisionUrgentContactRequired { get; set; }
        public DateTime? InitialDecisionCreatedAt { get; set; }
        public string? ScreeningDecision { get; set; }
        public bool? ScreeningUrgentContactRequired { get; set; }
        public DateTime? ScreeningCreatedAt { get; set; }
        public string? FinalDecision { get; set; }
        public string? FinalDecisionReferralCategory { get; set; }
        public bool? FinalDecisionUrgentContactRequired { get; set; }
        public DateTime? FinalDecisionCreatedAt { get; set; }
        public List<MashResident> MashResidents { get; set; } = null!;
        public Worker? Worker { get; set; }
    }
}
