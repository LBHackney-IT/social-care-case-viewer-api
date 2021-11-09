
using System.Collections.Generic;

#nullable enable
namespace SocialCareCaseViewerApi.V1.Boundary.Response
{
    public class MashReferral_2
    {
        public string Id { get; set; }
        public string Referrer { get; set; } = null!;
        public string RequestedSupport { get; set; } = null!;
        public WorkerResponse? AssignedTo { get; set; }
        public string ReferralCreatedAt { get; set; }
        public IEnumerable<string> Clients { get; set; } = null!;
        public string ReferralDocumentURI { get; set; } = null!;
        public string Stage { get; set; } = null!;
        public string? ReferralCategory { get; set; }
        public string? InitialDecision { get; set; }
        public string? InitialDecisionReferralCategory { get; set; }
        public string? InitialDecisionUrgentContactRequired { get; set; }
        public string? InitialDecisionCreatedAt { get; set; }
        public string? ScreeningDecision { get; set; }
        public string? ScreeningUrgentContactRequired { get; set; }
        public string? ScreeningCreatedAt { get; set; }
        public string? FinalDecision { get; set; }
        public string? FinalDecisionReferralCategory { get; set; }
        public string? FinalDecisionUrgentContactRequired { get; set; }
        public string? FinalDecisionCreatedAt { get; set; }  }
}
