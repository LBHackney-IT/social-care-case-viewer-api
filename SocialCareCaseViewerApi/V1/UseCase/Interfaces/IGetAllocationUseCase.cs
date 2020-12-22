using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Boundary.Response;

namespace SocialCareCaseViewerApi.V1.UseCase
{
    public interface IGetAllocationUseCase
    {
        AllocationList Execute(ListAllocationsRequest request);
    }
}
