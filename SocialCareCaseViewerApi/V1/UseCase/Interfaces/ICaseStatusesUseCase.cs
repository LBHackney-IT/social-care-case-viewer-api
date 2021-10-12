using System.Collections.Generic;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Boundary.Response;
using SocialCareCaseViewerApi.V1.Domain;

namespace SocialCareCaseViewerApi.V1.UseCase.Interfaces
{
    public interface ICaseStatusesUseCase
    {
        List<CaseStatusResponse> ExecuteGet(long personId);
        CaseStatus ExecutePost(CreateCaseStatusRequest request);
        CaseStatusResponse ExecuteUpdate(UpdateCaseStatusRequest request);
    }
}
