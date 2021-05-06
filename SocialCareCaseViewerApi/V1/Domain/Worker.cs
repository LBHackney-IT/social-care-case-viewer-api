using System;
using System.Collections.Generic;

#nullable enable
namespace SocialCareCaseViewerApi.V1.Domain
{
    public class Worker
    {
        public int Id { get; set; }

        public string Email { get; set; } = null!;

        public string FirstName { get; set; } = null!;

        public string LastName { get; set; } = null!;

        public string? Role { get; set; }

        public string? ContextFlag { get; set; }

        public string? CreatedBy { get; set; }

        public DateTime? DateStart { get; set; }

        public int AllocationCount { get; set; }

        public IList<Team> Teams { get; set; } = new List<Team>();
    }
}
