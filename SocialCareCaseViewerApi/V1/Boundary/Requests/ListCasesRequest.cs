using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SocialCareCaseViewerApi.V1.Boundary.Requests
{
    public class ListCasesRequest
    {
        public string MosaicId { get; set; }
        public string WorkerEmail { get; set; }
    }
}
