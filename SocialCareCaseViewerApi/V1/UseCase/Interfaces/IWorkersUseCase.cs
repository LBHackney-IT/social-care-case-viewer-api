using System.Collections.Generic;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Boundary.Response;

namespace SocialCareCaseViewerApi.V1.UseCase.Interfaces
{
    public interface IWorkersUseCase
    {
        List<WorkerResponse> ExecuteGet(GetWorkersRequest request);

        WorkerResponse ExecutePost(CreateWorkerRequest createWorkerRequest);

        void ExecutePatch(UpdateWorkerRequest updateWorkerRequest);
    }
}
