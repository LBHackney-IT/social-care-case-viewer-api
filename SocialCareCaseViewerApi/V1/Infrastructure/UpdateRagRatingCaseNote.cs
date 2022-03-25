using Newtonsoft.Json;

namespace SocialCareCaseViewerApi.V1.Infrastructure
{
    public class UpdateRagRatingCaseNote : CaseNoteBase
    {
        [JsonProperty("allocation_id")]
        public string AllocationId { get; set; }

        [JsonProperty("created_by")]
        public string CreatedBy { get; set; }
    }
}
