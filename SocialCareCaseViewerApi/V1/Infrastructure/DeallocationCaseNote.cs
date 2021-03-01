using Newtonsoft.Json;

namespace SocialCareCaseViewerApi.V1.Infrastructure
{
    public class DeallocationCaseNote : CaseNoteBase
    {
        [JsonProperty("deallocation_reason")]
        public string DeallocationReason { get; set; }

        [JsonProperty("allocation_id")]
        public string AllocationId { get; set; }

        [JsonProperty("created_by")]
        public string CreatedBy { get; set; }
    }
}
