using Newtonsoft.Json;

namespace SocialCareCaseViewerApi.V1.Infrastructure
{
    public class CreatePersonCaseNote : CaseNoteBase
    {
        [JsonProperty("note")]
        public string Note { get; set; }

        [JsonProperty("created_by")]
        public string CreatedBy { get; set; }
    }
}
