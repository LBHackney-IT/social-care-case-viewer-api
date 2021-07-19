using SocialCareCaseViewerApi.V1.Boundary.Requests;

namespace SocialCareCaseViewerApi.V1.UseCase.Interfaces
{
    public interface ICreateRequestAuditUseCase
    {
        void Execute(CreateRequestAuditRequest request);
    }
}
