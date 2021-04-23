using System.Collections.Generic;
using SocialCareCaseViewerApi.V1.Domain;

namespace SocialCareCaseViewerApi.V1.Boundary.Response
{
    public class WorkerResponse
    {
        public int Id { get; set; }

        public string Email { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }

        public string Role { get; set; }

        public string ContextFlag { get; set; }

        public string CreatedBy { get; set; }

        public string DateStart { get; set; }

        public int AllocationCount { get; set; }

        public IList<Team> Teams { get; set; } = new List<Team>();
    }
}
