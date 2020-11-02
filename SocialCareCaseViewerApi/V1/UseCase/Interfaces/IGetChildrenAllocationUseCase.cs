using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Boundary.Response;

namespace SocialCareCaseViewerApi.V1.UseCase
{
    public interface IGetChildrenAllocationUseCase
    {
        CfsAllocationList Execute(ListAllocationsRequest request);
    }
}
