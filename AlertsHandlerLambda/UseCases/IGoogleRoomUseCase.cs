using Amazon.Lambda.SNSEvents;
using System.Threading.Tasks;

namespace AlertsHandlerLambda.UseCases
{
    public interface IGoogleRoomUseCase
    {
        Task<string> SendMessage(SNSEvent snsEvent);
    }
}
