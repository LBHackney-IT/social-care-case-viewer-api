using AlertsHandlerLambda.Gateways;
using Amazon.Lambda.SNSEvents;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace AlertsHandlerLambda.UseCases
{
    public class GoogleRoomUseCase : IGoogleRoomUseCase
    {
        private readonly IGoogleAPIGateway _googleAPIGateway;

        public GoogleRoomUseCase(IGoogleAPIGateway googleAPIGateway)
        {
            _googleAPIGateway = googleAPIGateway;
        }
        public async Task<string> SendMessage(SNSEvent snsEvent)
        {
            //Lambda will trigger on event with a single message, so this should never be null
            string? message = snsEvent?.Records?.FirstOrDefault()?.Sns?.Message;

            if (message != null)
            {
                return await _googleAPIGateway.PostMessageToGoogleRoom(message);
            }
            else
            {
                throw new ArgumentNullException("Message");
            }
        }
    }
}
