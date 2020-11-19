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

        [FromQuery(Name = "first_name")]
        public string FirstName { get; set; }

        [FromQuery(Name = "last_name")]
        public string LastName { get; set; }

        [FromQuery(Name = "date_of_birth")]
        public string DateOfBirth { get; set; }

        [FromQuery(Name = "postcode")]
        public string Postcode { get; set; }
    }

    public class ListAllocationsRequest
    {
        [FromQuery(Name = "mosaic_id")]
        public long? MosaicId { get; set; }

        [FromQuery(Name = "worker_email")]
        public string WorkerEmail { get; set; }
    }
}
