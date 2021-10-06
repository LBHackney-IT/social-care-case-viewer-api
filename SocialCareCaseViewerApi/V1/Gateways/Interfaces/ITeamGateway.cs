using System.Collections.Generic;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Domain;

#nullable enable
namespace SocialCareCaseViewerApi.V1.Gateways.Interfaces
{
    public interface ITeamGateway
    {
        Team CreateTeam(CreateTeamRequest request);
        Infrastructure.Team? GetTeamByTeamId(int id);
        IEnumerable<Infrastructure.Team> GetTeamsByTeamContextFlag(string context);
    }
}
