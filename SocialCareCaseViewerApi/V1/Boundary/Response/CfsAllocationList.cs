using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SocialCareCaseViewerApi.V1.Boundary.Response
{
    public class CfsAllocationList
    {
        public IList<CfsAllocation> CfsAllocations { get; set; }
    }
}
