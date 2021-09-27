using System.Collections.Generic;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Factories;
using SocialCareCaseViewerApi.V1.Gateways.Interfaces;
using SocialCareCaseViewerApi.V1.Infrastructure;
using Team = SocialCareCaseViewerApi.V1.Domain.Team;

#nullable enable
namespace SocialCareCaseViewerApi.V1.Gateways
{
    public class TeamGateway : ITeamGateway
    {
        private readonly DatabaseContext _databaseContext;

        public TeamGateway(DatabaseContext databaseContext)
        {
            _databaseContext = databaseContext;
        }

        public Team CreateTeam(CreateTeamRequest request)
        {
            var team = new Infrastructure.Team { Name = request.Name, Context = request.Context, WorkerTeams = new List<WorkerTeam>() };

            _databaseContext.Teams.Add(team);
            _databaseContext.SaveChanges();

            return team.ToDomain();
        }
    }
}
