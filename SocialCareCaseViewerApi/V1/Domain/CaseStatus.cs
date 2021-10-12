using SocialCareCaseViewerApi.V1.Infrastructure;
using System;
using System.Collections.Generic;

#nullable enable
namespace SocialCareCaseViewerApi.V1.Domain
{
    public class CaseStatus
    {
        public long Id { get; set; }
        public string Type { get; set; } 
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Notes { get; set; }
        public Person Person { get; set; }
        public List<CaseStatusAnswer> Answers { get; set; }
    }
}
