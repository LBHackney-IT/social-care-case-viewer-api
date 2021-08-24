using AlertsHandlerLambda.Infrastructure;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace AlertsHandlerLambda.Gateways
{
    public class GoogleAPIGateway : IGoogleAPIGateway
    {
        private readonly HttpClient _httpClient;
        private readonly string _url = Environment.GetEnvironmentVariable("GOOGLE_CHAT_ROOM_PATH") ?? throw new ConfigurationException("GOOGLE_CHAT_ROOM_PATH not set");

        public GoogleAPIGateway(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string> PostMessageToGoogleRoom(string message)
        {
            string googleRoomMessage = JsonConvert.SerializeObject(new GoogleRoomMessage() { Text = message });

            var response = await _httpClient.PostAsync(_url, new StringContent(googleRoomMessage));

            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw new GoogleApiException(((int) response.StatusCode).ToString());
            }

            return "Message sent successfully";
        }
    }
}
