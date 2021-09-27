using SocialCareCaseViewerApi.V1.Domain;

#nullable enable
namespace SocialCareCaseViewerApi.V1.Gateways.Interfaces
{
    public interface IWorkerGateway
    {
        Worker? GetWorkerByWorkerId(int workerId);
    }
}
