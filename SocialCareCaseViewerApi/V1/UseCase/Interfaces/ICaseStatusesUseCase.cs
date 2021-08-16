using SocialCareCaseViewerApi.V1.Boundary.Response;

namespace SocialCareCaseViewerApi.V1.UseCase.Interfaces
{
    public interface ICaseStatusesUseCase
    {
        ListCaseStatusesResponse ExecuteGet(long personId, string startDate, string endDate);
    }
}
