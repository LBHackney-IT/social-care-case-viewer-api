using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Boundary.Response;
using SocialCareCaseViewerApi.V1.Infrastructure;

namespace SocialCareCaseViewerApi.V1.UseCase.Interfaces
{
    public interface ICaseStatusesUseCase
    {
        ListCaseStatusesResponse ExecuteGet(long personId);
        CaseStatus ExecutePost(CreateCaseStatusRequest request);
    }
}
