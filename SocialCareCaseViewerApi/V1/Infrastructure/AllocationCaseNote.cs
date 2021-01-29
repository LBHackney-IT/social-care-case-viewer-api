using Newtonsoft.Json;

namespace SocialCareCaseViewerApi.V1.Infrastructure
{
    public class AllocationCaseNote : CaseNoteBase
    {
        [JsonProperty("note")]
        public string Note { get; set; }

        [JsonProperty("allocation_id")]
        public string AllocationId { get; set; }

        [JsonProperty("created_by")]
        public string CreatedBy { get; set; }
    }
}
