using System;
using System.Collections.Generic;

#nullable enable
namespace SocialCareCaseViewerApi.V1.Domain
{
    public class MashReferral
    {
        public string Id { get; set; }
        public string Referrer { get; set; } = null!;
        public string RequestedSupport { get; set; } = null!;
        public Worker? AssignedTo { get; set; }
        public DateTime CreatedAt { get; set; }
        public IEnumerable<string> Clients { get; set; } = null!;
        public string ReferralDocumentURI { get; set; } = null!;
        public string Stage { get; set; } = null!;
        public string? InitialDecision { get; set; }
        public bool? InitialUrgentContactRequired { get; set; }
        public string? InitialReferralCategory { get; set; }
        public DateTime? InitialCreatedAt { get; set; }
        public string? ScreeningDecision { get; set; }
        public bool? ScreeningUrgentContactRequired { get; set; }
        public DateTime? ScreeningCreatedAt { get; set; }
        public string? FinalDecision { get; set; }
    }
}
