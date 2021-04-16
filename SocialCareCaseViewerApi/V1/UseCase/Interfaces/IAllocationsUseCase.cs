using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Boundary.Response;

namespace SocialCareCaseViewerApi.V1.UseCase.Interfaces
{
    public interface IAllocationsUseCase
    {
        AllocationList Execute(ListAllocationsRequest request);

        UpdateAllocationResponse ExecuteUpdate(UpdateAllocationRequest request);

        CreateAllocationResponse ExecutePost(CreateAllocationRequest request);
    }
}
