using SocialCareCaseViewerApi.V1.Domain;

namespace SocialCareCaseViewerApi.V1.Gateways.Interfaces
{
    public interface IWorkerGateway
    {
        Worker GetWorkerByWorkerId(int workerId);
    }
}
