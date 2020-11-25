using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace SocialCareCaseViewerApi.V1.Boundary.Requests
{
    public class ListCasesRequest
    {
        [FromQuery(Name = "first_name")]
        public string FirstName { get; set; }

        [FromQuery(Name = "last_name")]
        public string LastName { get; set; }

        [FromQuery(Name = "worker_email")]
        public string WorkerEmail { get; set; }

        [FromQuery(Name = "case_note_type")]
        public string CaseNoteType { get; set; }
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
