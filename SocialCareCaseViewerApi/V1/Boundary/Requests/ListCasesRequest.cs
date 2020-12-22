using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

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

        [FromQuery(Name = "form_name")]
        public string FormName { get; set; }

        [FromQuery(Name = "start_date")]
        public string StartDate { get; set; }

        [FromQuery(Name = "end_date")]
        public string EndDate { get; set; }

        [FromQuery(Name = "cursor")]
        public int Cursor { get; set; } = 0;

        [FromQuery(Name = "limit")]
        public int Limit { get; set; } = 20;
    }

    public class ListAllocationsRequest
    {
        [FromQuery(Name = "mosaic_id")]
        [Required]
        public string MosaicId { get; set; }
    }
}
