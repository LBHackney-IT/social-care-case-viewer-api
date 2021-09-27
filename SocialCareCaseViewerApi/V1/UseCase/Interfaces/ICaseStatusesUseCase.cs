using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Boundary.Response;
using SocialCareCaseViewerApi.V1.Domain;

namespace SocialCareCaseViewerApi.V1.UseCase.Interfaces
{
    public interface ICaseStatusesUseCase
    {
        GetCaseStatusFieldsResponse ExecuteGetFields(GetCaseStatusFieldsRequest request);

        ListCaseStatusesResponse ExecuteGet(long personId);
        CaseStatus ExecutePost(CreateCaseStatusRequest request);
    }
}
