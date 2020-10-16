using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Boundary.Response;

namespace SocialCareCaseViewerApi.V1.UseCase.Interfaces
{
    public interface IGetAllUseCase
    {
        ResidentInformationList Execute(ResidentQueryParam rqp, int cursor, int limit);
    }
}
