using System.Threading.Tasks;

namespace AlertsHandlerLambda.Gateways
{
    public interface IGoogleAPIGateway
    {
        Task<string> PostMessageToGoogleRoom(string message);
    }
}
