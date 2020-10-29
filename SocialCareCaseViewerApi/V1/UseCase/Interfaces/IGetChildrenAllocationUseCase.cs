using SocialCareCaseViewerApi.V1.Boundary.Response;

namespace SocialCareCaseViewerApi.V1.UseCase
{
    public interface IGetChildrenAllocationUseCase
    {
        CfsAllocationList Execute(string officerEmail, long mosaicId);
    }
}
