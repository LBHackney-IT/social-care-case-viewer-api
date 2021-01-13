using SocialCareCaseViewerApi.V1.Boundary.Requests;

namespace SocialCareCaseViewerApi.V1.UseCase.Interfaces
{
    public interface ICreateAllocationUseCase
    {
        public CreateAllocationRequest Execute(CreateAllocationRequest request);
    }
}
