using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
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

        public Infrastructure.Team? GetTeamByTeamId(int teamId)
        {
            var team = _databaseContext.Teams
                .Where(x => x.Id == teamId)
                .Include(x => x.WorkerTeams)
                .ThenInclude(x => x.Worker)
                .ThenInclude(x => x.Allocations)
                .FirstOrDefault();

            if (team != null && team.WorkerTeams != null)
            {
                foreach (var wt in team.WorkerTeams.ToList())
                {
                    if (wt.EndDate != null) team.WorkerTeams.Remove(wt);
                }
            }

            return team;
        }

        public IEnumerable<Infrastructure.Team> GetTeamsByTeamContextFlag(string context)
        {
            return _databaseContext.Teams.Where(x => x.Context.ToUpper().Equals(context.ToUpper()));
        }
    }
}
