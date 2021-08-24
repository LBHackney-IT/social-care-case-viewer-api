using Newtonsoft.Json;

namespace AlertsHandlerLambda.Infrastructure
{
    public class GoogleRoomMessage
    {
        [JsonProperty("text")]
        public string? Text { get; set; }
    }
}
