using SocialCareCaseViewerApi.V1.Boundary.Response;

namespace SocialCareCaseViewerApi.V1.UseCase.Interfaces
{
    public interface IRelationshipsUseCase
    {
        ListRelationshipsResponse ExecuteGet(long personId);
    }
}
