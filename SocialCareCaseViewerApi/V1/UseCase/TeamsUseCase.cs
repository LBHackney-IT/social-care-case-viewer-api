using System.Linq;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Boundary.Response;
using SocialCareCaseViewerApi.V1.Exceptions;
using SocialCareCaseViewerApi.V1.Factories;
using SocialCareCaseViewerApi.V1.Gateways.Interfaces;
using SocialCareCaseViewerApi.V1.UseCase.Interfaces;

#nullable enable
namespace SocialCareCaseViewerApi.V1.UseCase
{
    public class TeamsUseCase : ITeamsUseCase
    {
        private readonly IDatabaseGateway _databaseGateway;
        private readonly ITeamGateway _teamGateway;

        public TeamsUseCase(IDatabaseGateway databaseGateway, ITeamGateway teamGateway)
        {
            _databaseGateway = databaseGateway;
            _teamGateway = teamGateway;
        }

        public TeamResponse? ExecuteGetById(int id)
        {
            var team = _teamGateway.GetTeamByTeamId(id);
            return team?.ToDomain().ToResponse();
        }

        public TeamResponse? ExecuteGetByName(string name)
        {
            var team = _databaseGateway.GetTeamByTeamName(name);
            return team?.ToDomain().ToResponse();
        }

        public ListTeamsResponse ExecuteGet(GetTeamsRequest request)
        {
            var teams = _databaseGateway.GetTeamsByTeamContextFlag(request.ContextFlag);
            return new ListTeamsResponse() { Teams = teams.Select(team => team.ToDomain().ToResponse()).ToList() };
        }

        public TeamResponse ExecutePost(CreateTeamRequest request)
        {
            if (_databaseGateway.GetTeamByTeamName(request.Name) != null)
            {
                throw new PostTeamException($"Team with name \"{request.Name}\" already exists");
            }

            var team = _teamGateway.CreateTeam(request);

            return team.ToResponse();
        }
    }
}
