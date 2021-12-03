using SocialCareCaseViewerApi.V1.Domain;
using System.Collections.Generic;
#nullable enable
namespace SocialCareCaseViewerApi.V1.Boundary.Response
{
    public class MashReferral_2
    {
        public long Id { get; set; }
        public string Referrer { get; set; } = null!;
        public string RequestedSupport { get; set; } = null!;
        public string ReferralCreatedAt { get; set; } = null!;
        public string ReferralDocumentURI { get; set; } = null!;
        public string Stage { get; set; } = null!;
        public string? ReferralCategory { get; set; }
        public string? ContactDecisionCreatedAt { get; set; }
        public bool? ContactDecisionUrgentContactRequired { get; set; }
        public string? InitialDecision { get; set; }
        public string? InitialDecisionReferralCategory { get; set; }
        public bool? InitialDecisionUrgentContactRequired { get; set; }
        public string? InitialDecisionCreatedAt { get; set; }
        public string? ScreeningDecision { get; set; }
        public bool? ScreeningUrgentContactRequired { get; set; }
        public string? ScreeningCreatedAt { get; set; }
        public string? FinalDecision { get; set; }
        public string? FinalDecisionReferralCategory { get; set; }
        public bool? FinalDecisionUrgentContactRequired { get; set; }
        public string? FinalDecisionCreatedAt { get; set; }
        public List<MashResidentResponse> MashResidents { get; set; }
    }
}
