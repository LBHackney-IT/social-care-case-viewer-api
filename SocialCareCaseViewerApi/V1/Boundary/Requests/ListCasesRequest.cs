using Microsoft.AspNetCore.Mvc;
using System;

namespace SocialCareCaseViewerApi.V1.Boundary.Requests
{
    public class ListCasesRequest
    {
        [FromQuery(Name = "mosaic_id")]
        public string MosaicId { get; set; }

        [FromQuery(Name = "exact_name_match")]
        public Boolean ExactNameMatch { get; set; } = false;
        [FromQuery(Name = "first_name")]
        public string FirstName { get; set; }

        [FromQuery(Name = "last_name")]
        public string LastName { get; set; }

        [FromQuery(Name = "worker_email")]
        public string WorkerEmail { get; set; }

        [FromQuery(Name = "case_note_type")]
        public string CaseNoteType { get; set; }

        [FromQuery(Name = "start_date")]
        public string StartDate { get; set; }

        [FromQuery(Name = "end_date")]
        public string EndDate { get; set; }
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
