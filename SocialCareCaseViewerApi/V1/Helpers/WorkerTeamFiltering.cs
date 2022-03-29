using SocialCareCaseViewerApi.V1.Infrastructure;
using System.Linq;

#nullable enable
namespace SocialCareCaseViewerApi.V1.Helpers
{
    public static class WorkerTeamFiltering
    {
        public static void RemoveHistoricalWorkerTeamsFromAWorker(Worker? worker)
        {
            if (worker != null && worker.WorkerTeams != null)
            {
                foreach (var wt in worker.WorkerTeams.ToList())
                {
                    if (wt.EndDate != null) worker.WorkerTeams.Remove(wt);
                }
            }
        }

        public static void RemoveHistoricalWorkerTeamsFromATeam(Team? team)
        {
            if (team != null && team.WorkerTeams != null)
            {
                foreach (var wt in team.WorkerTeams.ToList())
                {
                    if (wt.EndDate != null) team.WorkerTeams.Remove(wt);
                }
            }
        }
    }
}
