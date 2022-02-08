using System;
using Microsoft.AspNetCore.Mvc;

#nullable enable
namespace SocialCareCaseViewerApi.V1.Boundary.Requests
{
    public class ListCasesRequest
    {
        [FromQuery(Name = "mosaic_id")]
        public string? MosaicId { get; set; }

        [FromQuery(Name = "exact_name_match")]
        public bool ExactNameMatch { get; set; } = false;

        [FromQuery(Name = "first_name")]
        public string? FirstName { get; set; }

        [FromQuery(Name = "last_name")]
        public string? LastName { get; set; }

        [FromQuery(Name = "worker_email")]
        public string? WorkerEmail { get; set; }

        [FromQuery(Name = "form_name")]
        public string? FormName { get; set; }

        [FromQuery(Name = "start_date")]
        public DateTime? StartDate { get; set; }

        [FromQuery(Name = "end_date")]
        public DateTime? EndDate { get; set; }

        [FromQuery(Name = "cursor")]
        public int Cursor { get; set; } = 0;

        [FromQuery(Name = "limit")]
        public int Limit { get; set; } = 20;

        [FromQuery(Name = "sort_by")]
        public string? SortBy { get; set; } = null!;

        [FromQuery(Name = "order_by")]
        public string? OrderBy { get; set; } = null!;

        [FromQuery(Name = "include_deleted_records")]
        public bool IncludeDeletedRecords { get; set; } = false;

        [FromQuery(Name = "include_deleted_records_count")]
        public bool IncludeDeletedRecordsCount { get; set; } = false;

        [FromQuery(Name = "exclude_audit_trail_events")]
        public bool ExcludeAuditTrailEvents { get; set; } = false;
    }
}
