using System;

#nullable enable

namespace SocialCareCaseViewerApi.V1.Domain
{
    public class Visit
    {
        public long VisitId { get; set; }

        public long PersonId { get; set; }

        public string VisitType { get; set; } = null!;

        public DateTime? PlannedDateTime { get; set; }

        public DateTime? ActualDateTime { get; set; }

        public string? CreatedByName { get; set; }

        public string? CreatedByEmail { get; set; }

        public string? ReasonNotPlanned { get; set; }

        public string? ReasonVisitNotMade { get; set; }

        public bool SeenAloneFlag { get; set; }

        public bool CompletedFlag { get; set; }
    }
}
