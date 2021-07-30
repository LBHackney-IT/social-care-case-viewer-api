using System.Collections.Generic;
using Bogus;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Infrastructure;

namespace SocialCareCaseViewerApi.Tests.V1.IntegrationTests
{
    public static class IntegrationTestHelpers
    {
        public static (Worker, string, string, string) SetupExistingWorker()
        {
            var workerId = new Faker().Random.Int(1, 100);
            var workerContext = new Faker().Random.String2(1, "AC");

            var team = new Faker<Team>()
                .RuleFor(t => t.Id, f => f.UniqueIndex + 1)
                .RuleFor(t => t.Context, f => workerContext)
                .RuleFor(t => t.Name, f => f.Random.String2(10, 15)).Generate();

            var insertTeam = SeedTeam(team);

            var workerTeam = new Faker<WorkerTeam>()
                .RuleFor(t => t.Id, f => f.UniqueIndex + 1)
                .RuleFor(t => t.WorkerId, f => workerId)
                .RuleFor(t => t.TeamId, f => team.Id)
                .RuleFor(t => t.Team, team).Generate();

            var insertWorkerTeam = SeedWorkerTeam(workerTeam);

            var worker = new Faker<Worker>().RuleFor(w => w.Id, workerId)
                .RuleFor(w => w.FirstName, f => f.Person.FirstName)
                .RuleFor(w => w.LastName, f => f.Person.LastName)
                .RuleFor(w => w.Email, f => f.Person.Email)
                .RuleFor(w => w.Role, f => f.Random.Word())
                .RuleFor(w => w.ContextFlag, f => workerContext)
                .RuleFor(w => w.CreatedBy, f => f.Person.Email)
                .RuleFor(w => w.CreatedAt, f => f.Date.Soon())
                .RuleFor(w => w.DateStart, f => f.Date.Recent())
                .RuleFor(w => w.DateEnd, f => f.Date.Soon())
                .RuleFor(w => w.IsActive, true)
                .RuleFor(w => w.Allocations, new List<AllocationSet>())
                .RuleFor(w => w.WorkerTeams, new List<WorkerTeam> { workerTeam })
                .RuleFor(w => w.LastModifiedBy, f => f.Person.Email).Generate();

            var insertWorker = SeedWorker(worker);

            return (worker, insertTeam, insertWorkerTeam, insertWorker);
        }

        public static (Team, string) CreateAnotherTeam(string workerContext)
        {
            var team = new Faker<Team>()
                .RuleFor(t => t.Id, f => f.UniqueIndex + 1)
                .RuleFor(t => t.Context, f => workerContext)
                .RuleFor(t => t.Name, f => f.Random.String2(10, 15)).Generate();

            var insertTeam = SeedTeam(team);

            return (team, insertTeam);
        }

        public static UpdateWorkerRequest CreatePatchRequest(Worker worker, WorkerTeamRequest teamRequest)
        {
            return new UpdateWorkerRequest
            {
                WorkerId = worker.Id,
                ModifiedBy = new Faker().Person.Email,
                FirstName = worker.FirstName,
                LastName = worker.LastName,
                ContextFlag = worker.ContextFlag,
                Teams = new List<WorkerTeamRequest> { teamRequest },
                Role = worker.Role,
                DateStart = worker.DateStart
            };
        }

        private static string SeedWorker(Worker worker)
        {
            var insertWorkerQuery = $@"insert into dbo.sccv_worker 
            (id, email, first_name, last_name, role, context_flag, created_by, date_start, date_end, last_modified_by, created_at, last_modified_at, is_active ) 
            values ({worker.Id}, '{worker.Email}', '{worker.FirstName}', '{worker.LastName}', '{worker.Role}', '{worker.ContextFlag}', '{worker.CreatedBy}', NULL, NULL, '{worker.LastModifiedBy}', NULL, NULL, {worker.IsActive});";

            return insertWorkerQuery;
        }

        private static string SeedTeam(Team team)
        {
            var insertTeamQuery = $"insert into dbo.sccv_team (id, name, context) values ({team.Id}, '{team.Name}', '{team.Context}');";

            return insertTeamQuery;
        }

        private static string SeedWorkerTeam(WorkerTeam workerTeam)
        {
            var insertWorkerTeamQuery = $"insert into dbo.sccv_workerteam (id, worker_id, team_id) values ({workerTeam.Id}, {workerTeam.WorkerId}, {workerTeam.TeamId});";

            return insertWorkerTeamQuery;
        }
    }
}
