using SocialCareCaseViewerApi.V1.Boundary.Requests;
namespace SocialCareCaseViewerApi.V1.UseCase.Interfaces
{
    public interface IPersonalRelationshipsUseCase
    {
        void ExecutePost(CreatePersonalRelationshipRequest request);
    }
}
