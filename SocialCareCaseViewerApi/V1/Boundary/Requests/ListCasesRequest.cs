using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace SocialCareCaseViewerApi.V1.Boundary.Requests
{
    public class ListCasesRequest
    {
        [FromQuery(Name = "mosaic_id")]
        public string MosaicId { get; set; }

        [FromQuery(Name = "worker_email")]
        public string WorkerEmail { get; set; }
    }

    public class ListAllocationsRequest
    {
        [FromQuery(Name = "mosaic_id")]
        public long? MosaicId { get; set; }

        [FromQuery(Name = "worker_email")]
        public string WorkerEmail { get; set; }
    }

    public class ListAscAllocationsRequest
    {
        [FromQuery(Name = "mosaic_id")]
        public long? MosaicId { get; set; }

        [FromQuery(Name = "allocated_worker")]

        public string AllocatedWorker { get; set; }
    }
}
