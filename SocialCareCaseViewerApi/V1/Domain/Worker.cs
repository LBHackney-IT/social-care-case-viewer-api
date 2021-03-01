using System.Collections.Generic;

namespace SocialCareCaseViewerApi.V1.Domain
{
    public class Worker
    {
        public int Id { get; set; }

        public string Email { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Role { get; set; }

        public int AllocationCount { get; set; }

        public IList<Team> Teams { get; set; } = new List<Team>();
    }
}
