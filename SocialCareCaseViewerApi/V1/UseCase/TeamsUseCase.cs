using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Boundary.Response;
using SocialCareCaseViewerApi.V1.Factories;
using SocialCareCaseViewerApi.V1.Gateways;
using SocialCareCaseViewerApi.V1.UseCase.Interfaces;

namespace SocialCareCaseViewerApi.V1.UseCase
{
    public class TeamsUseCase : ITeamsUseCase
    {
        private IDatabaseGateway _databaseGateway;

        public TeamsUseCase(IDatabaseGateway databaseGateway)
        {
            _databaseGateway = databaseGateway;
        }

        public ListTeamsResponse ExecuteGet(ListTeamsRequest request)
        {
            var teams = _databaseGateway.GetTeams(request.ContextFlag);

            return new ListTeamsResponse() { Teams = EntityFactory.ToDomain(teams) };
        }
    }
}
