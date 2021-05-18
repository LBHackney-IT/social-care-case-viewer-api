using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Boundary.Response;

namespace SocialCareCaseViewerApi.V1.UseCase.Interfaces
{
    public interface ITeamsUseCase
    {
        TeamResponse ExecuteGetById(int id);

        TeamResponse ExecuteGetByName(string name);

        ListTeamsResponse ExecuteGet(GetTeamsRequest request);

        TeamResponse ExecutePost(CreateTeamRequest request);
    }
}
