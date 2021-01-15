using Newtonsoft.Json;

namespace SocialCareCaseViewerApi.V1.Infrastructure
{
    public class DeallocationCaseNote : CaseNoteBase
    {
        [JsonProperty("deallocation_reason")]
        public string DeallocationReason { get; set; }
    }
}
