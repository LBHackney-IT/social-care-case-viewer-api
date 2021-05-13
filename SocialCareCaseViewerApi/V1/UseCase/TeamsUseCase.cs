using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Boundary.Response;
using SocialCareCaseViewerApi.V1.Factories;
using SocialCareCaseViewerApi.V1.Gateways;
using SocialCareCaseViewerApi.V1.UseCase.Interfaces;

namespace SocialCareCaseViewerApi.V1.UseCase
{
    public class TeamsUseCase : ITeamsUseCase
    {
        private readonly IDatabaseGateway _databaseGateway;

        public TeamsUseCase(IDatabaseGateway databaseGateway)
        {
            _databaseGateway = databaseGateway;
        }

        public ListTeamsResponse ExecuteGet(ListTeamsRequest request)
        {
            var teams = _databaseGateway.GetTeams(request.ContextFlag);

            return new ListTeamsResponse() { Teams = EntityFactory.ToDomain(teams) };
        }

        public TeamResponse ExecutePost(CreateTeamRequest request)
        {

            // todo check if team name already exists, if so throw an error

            var team = _databaseGateway.CreateTeam(request);

            return team.ToDomain().ToResponse();
        }
    }
}
