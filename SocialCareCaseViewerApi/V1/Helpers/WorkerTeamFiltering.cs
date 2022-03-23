using SocialCareCaseViewerApi.V1.Infrastructure;
using System.Linq;

#nullable enable
namespace SocialCareCaseViewerApi.V1.Helpers
{
    public static class WorkerTeamFiltering
    {
        public static void RemoveHistoricalWorkerTeamsFromAWorker<T>(T? worker) where T : Worker
        {
            if (worker != null && worker.WorkerTeams != null)
            {
                foreach (var wt in worker.WorkerTeams.ToList())
                {
                    if (wt.EndDate != null) worker.WorkerTeams.Remove(wt);
                }
            }
        }

        public static void RemoveHistoricalWorkerTeamsFromATeam<T>(T? team) where T : Team
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
