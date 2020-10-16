using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SocialCareCaseViewerApi.V1.Boundary.Requests
{
    public class ListCasesRequest
    {
        [FromQuery(Name = "mosaic_id")]
        public string MosaicId { get; set; }

        [FromQuery(Name = "worker_email")]
        public string WorkerEmail { get; set; }
    }
}
