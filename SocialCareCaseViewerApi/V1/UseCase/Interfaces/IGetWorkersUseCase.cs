using System.Collections.Generic;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Boundary.Response;

namespace SocialCareCaseViewerApi.V1.UseCase.Interfaces
{
    public interface IGetWorkersUseCase
    {
        List<WorkerResponse> Execute(GetWorkersRequest request);
    }
}
