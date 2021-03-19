using Newtonsoft.Json;

namespace SocialCareCaseViewerApi.V1.Infrastructure
{
    public class WarningNoteCaseNote : CaseNoteBase
    {
        [JsonProperty("note")]
        public string Note { get; set; }

        [JsonProperty("warning_note_id")]
        public string WarningNoteId { get; set; }
    }
}
