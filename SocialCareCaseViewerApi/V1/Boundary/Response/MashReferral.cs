
using System.Collections.Generic;

#nullable enable
namespace SocialCareCaseViewerApi.V1.Boundary.Response
{
    public class MashReferral
    {
        public string Id { get; set; }
        public string Referrer { get; set; } = null!;
        public string RequestedSupport { get; set; } = null!;
        public WorkerResponse? AssignedTo { get; set; }
        public string CreatedAt { get; set; }
        public IEnumerable<string> Clients { get; set; } = null!;
        public string ReferralDocumentURI { get; set; } = null!;
        public string Stage { get; set; } = null!;
        public string? InitialDecision { get; set; }
        public string? ScreeningDecision { get; set; }
        public bool? ScreeningUrgentContactRequired { get; set; }
        public string? ScreeningCreatedAt { get; set; }
        public string? FinalDecision { get; set; }
        public string? ReferralCategory { get; set; }
    }
}
