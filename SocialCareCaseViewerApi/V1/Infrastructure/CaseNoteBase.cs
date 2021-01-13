using Newtonsoft.Json;
using System;

namespace SocialCareCaseViewerApi.V1.Infrastructure
{
    public class CaseNoteBase
    {
        [JsonProperty("first_name")]
        public string FirstName { get; set; }

        [JsonProperty("last_name")]
        public string LastName { get; set; }

        [JsonProperty("timestamp")]
        public DateTime Timestamp { get; set; }

        [JsonProperty("mosaic_id")]
        public string MosaicId { get; set; }

        [JsonProperty("worker_email")]
        public string WorkerEmail { get; set; }

        [JsonProperty("form_name_overall")]
        public string FormNameOverall { get; set; }
    }
}
