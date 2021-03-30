using System.Collections.Generic;
using SocialCareCaseViewerApi.V1.Infrastructure;

namespace SocialCareCaseViewerApi.Tests.V1.Helpers
{
    public static class DatabaseGatewayHelper
    {
        public static Worker CreateWorkerDatabaseEntity(
            int id = 1,
            string email = "testemail@example.com",
            string firstName = "test-first-name",
            string lastName = "test-last-name")
        {
            return new Worker {Id = id, Email = email, FirstName = firstName, LastName = lastName};
        }

        public static Team CreateTeamDatabaseEntity(
            List<WorkerTeam> workerTeams,
            string content = "t",
            int id = 1,
            string name = "test-name"
        )
        {
            return new Team {Context = content, Id = id, Name = name, WorkerTeams = workerTeams};
        }

        public static WorkerTeam CreateWorkerTeamDatabaseEntity(
            Worker worker,
            int id = 1,
            int workerId = 1,
            int teamId = 1
            )
        {
            return new WorkerTeam {Worker = worker, Id = id, WorkerId = workerId, TeamId = teamId};
        }
    }
}
