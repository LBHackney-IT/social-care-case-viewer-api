using System.Collections.Generic;

namespace SocialCareCaseViewerApi.V1.Boundary.Response
{
    public class AllocationList
    {
        public IList<Allocation> Allocations { get; set; }

        public string NextCursor { get; set; }
    }
}
