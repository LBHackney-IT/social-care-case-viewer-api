using System;
using System.Collections.Generic;

#nullable enable
namespace SocialCareCaseViewerApi.V1.Domain
{
    public class Worker
    {
        public int Id { get; set; }

        public string Email { get; set; } = null!;

        public string? FirstName { get; set; } = null;

        public string? LastName { get; set; } = null;

        public string? Role { get; set; } = null;

        public string? ContextFlag { get; set; } = null;

        public string? CreatedBy { get; set; } = null;

        public DateTime? DateStart { get; set; } = null;

        public int? AllocationCount { get; set; } = null;

        public IList<Team>? Teams { get; set; }
    }
}
