using System.Collections.Generic;
using System.Linq;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Boundary.Response;
using SocialCareCaseViewerApi.V1.Exceptions;
using SocialCareCaseViewerApi.V1.Factories;
using SocialCareCaseViewerApi.V1.Gateways;
using SocialCareCaseViewerApi.V1.Infrastructure;
using SocialCareCaseViewerApi.V1.UseCase.Interfaces;

#nullable enable
namespace SocialCareCaseViewerApi.V1.UseCase
{
    public class TeamsUseCase : ITeamsUseCase
    {
        private readonly IDatabaseGateway _databaseGateway;

        public TeamsUseCase(IDatabaseGateway databaseGateway)
        {
            _databaseGateway = databaseGateway;
        }

        public TeamResponse? ExecuteGetById(int id)
        {
            var team = _databaseGateway.GetTeamByTeamId(id);
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

            var team = _databaseGateway.CreateTeam(request);

            return team.ToDomain().ToResponse();
        }
    }
}
