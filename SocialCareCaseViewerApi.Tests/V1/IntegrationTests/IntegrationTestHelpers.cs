using System.Collections.Generic;
using System.Linq;
using Bogus;
using Microsoft.EntityFrameworkCore;
using Npgsql;
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
            // var seedCommand = new NpgsqlCommand();
            // seedCommand.Connection = new NpgsqlConnection(ConnectionString.TestDatabase());
            // seedCommand.Connection.Open();

            // var npgsqlCommand = seedCommand.Connection.CreateCommand();
            // npgsqlCommand.CommandText = "SET deadlock_timeout TO 30";
            // npgsqlCommand.ExecuteNonQuery();

            //             var insertWorkerQuery = @"insert into dbo.sccv_worker 
            // (id, email, first_name, last_name, role, context_flag, created_by, date_start, date_end, last_modified_by, created_at, last_modified_at, is_active ) 
            // values (@Id, @Email, @FirstName, @LastName, @Role, @ContextFlag, @CreatedBy, @DateStart, @DateEnd, @LastModifiedBy, @CreatedAt, @LastModifiedAt, @IsActive);";

            var insertWorkerQuery = $@"insert into dbo.sccv_worker (id, email, first_name, last_name, role, context_flag, created_by, date_start, date_end, last_modified_by, created_at, last_modified_at, is_active ) values ({worker.Id}, '{worker.Email}', '{worker.FirstName}', '{worker.LastName}', '{worker.Role}', '{worker.ContextFlag}', '{worker.CreatedBy}', NULL, NULL, '{worker.LastModifiedBy}', NULL, NULL, {worker.IsActive});";


            // seedCommand.CommandText = insertWorkerQuery;

            // seedCommand.Parameters.AddWithValue("@Id", worker.Id);
            // seedCommand.Parameters.AddWithValue("@Email", $"{worker.Email}");
            // seedCommand.Parameters.AddWithValue("@FirstName", $"{worker.FirstName}");
            // seedCommand.Parameters.AddWithValue("@LastName", $"{worker.LastName}");
            // seedCommand.Parameters.AddWithValue("@Role", $"{worker.Role}");
            // seedCommand.Parameters.AddWithValue("@ContextFlag", $"{worker.ContextFlag}");
            // seedCommand.Parameters.AddWithValue("@CreatedBy", $"{worker.CreatedBy}");
            // seedCommand.Parameters.AddWithValue("@DateStart", worker.DateStart);
            // seedCommand.Parameters.AddWithValue("@DateEnd", worker.DateEnd);
            // seedCommand.Parameters.AddWithValue("@LastModifiedBy", $"{worker.LastModifiedBy}");
            // seedCommand.Parameters.AddWithValue("@CreatedAt", worker.CreatedAt);
            // seedCommand.Parameters.AddWithValue("@LastModifiedAt", worker.LastModifiedAt);
            // seedCommand.Parameters.AddWithValue("@IsActive", worker.IsActive);

            return insertWorkerQuery;
        }

        private static string SeedTeam(Team team)
        {
            // var seedCommand = new NpgsqlCommand();
            // seedCommand.Connection = new NpgsqlConnection(ConnectionString.TestDatabase());
            // seedCommand.Connection.Open();

            // var npgsqlCommand = seedCommand.Connection.CreateCommand();
            // npgsqlCommand.CommandText = "SET deadlock_timeout TO 30";
            // npgsqlCommand.ExecuteNonQuery();

            // var insertTeamQuery = "insert into dbo.sccv_team (id, name, context) values (@Id, @Name, @Context);";

            var insertTeamQuery = $"insert into dbo.sccv_team (id, name, context) values ({team.Id}, '{team.Name}', '{team.Context}');";


            // seedCommand.CommandText = insertTeamQuery;

            // seedCommand.Parameters.AddWithValue("@Id", team.Id);
            // seedCommand.Parameters.AddWithValue("@Name", $"{team.Name}");
            // seedCommand.Parameters.AddWithValue("@Context", $"{team.Context}");

            return insertTeamQuery;
        }

        private static string SeedWorkerTeam(WorkerTeam workerTeam)
        {
            // var seedCommand = new NpgsqlCommand();
            // seedCommand.Connection = new NpgsqlConnection(ConnectionString.TestDatabase());
            // seedCommand.Connection.Open();

            // var npgsqlCommand = seedCommand.Connection.CreateCommand();
            // npgsqlCommand.CommandText = "SET deadlock_timeout TO 30";
            // npgsqlCommand.ExecuteNonQuery();

            // var insertWorkerTeamQuery = "insert into dbo.sccv_workerteam (id, worker_id, team_id) values (@Id, @WorkerId, @TeamId);";

            var insertWorkerTeamQuery = $"insert into dbo.sccv_workerteam (id, worker_id, team_id) values ({workerTeam.Id}, {workerTeam.WorkerId}, {workerTeam.TeamId});";

            // seedCommand.CommandText = insertWorkerTeamQuery;

            // seedCommand.Parameters.AddWithValue("@Id", workerTeam.Id);
            // seedCommand.Parameters.AddWithValue("@WorkerId", workerTeam.WorkerId);
            // seedCommand.Parameters.AddWithValue("@TeamId", workerTeam.TeamId);

            return insertWorkerTeamQuery;
        }
    }
}
