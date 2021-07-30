using System.Collections.Generic;
using System.Linq;
using Bogus;
using Microsoft.EntityFrameworkCore;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Infrastructure;

namespace SocialCareCaseViewerApi.Tests.V1.IntegrationTests
{
    public static class IntegrationTestHelpers
    {
        public static Worker SetupExistingWorker(DatabaseContext context)
        {
            var workerId = new Faker().Random.Int(1, 100);
            var workerContext = new Faker().Random.String2(1, "AC");

            var team = new Faker<Team>()
                .RuleFor(t => t.Id, f => f.UniqueIndex + 1)
                .RuleFor(t => t.Context, f => workerContext)
                .RuleFor(t => t.Name, f => f.Random.String2(10, 100)).Generate();

            context.Teams.Add(team);
            context.SaveChanges();

            var workerTeam = new Faker<WorkerTeam>()
                .RuleFor(t => t.Id, f => f.UniqueIndex + 1)
                .RuleFor(t => t.WorkerId, f => workerId)
                .RuleFor(t => t.TeamId, f => team.Id)
                .RuleFor(t => t.Team, team).Generate();

            context.WorkerTeams.Add(workerTeam);
            context.SaveChanges();

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

            context.Workers.Add(worker);
            context.SaveChanges();

            return worker;
        }

        public static Team CreateAnotherTeam(DatabaseContext context, string workerContext)
        {
            var team = new Faker<Team>()
                .RuleFor(t => t.Id, f => f.UniqueIndex + 1)
                .RuleFor(t => t.Context, f => workerContext)
                .RuleFor(t => t.Name, f => f.Random.String2(10, 100)).Generate();

            context.Teams.Add(team);
            context.SaveChanges();

            return team;
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

        public static Worker UpdateWorkerWithRequest(DatabaseContext databaseContext, Worker worker, UpdateWorkerRequest request)
        {
            worker.LastModifiedBy = request.ModifiedBy;
            worker.FirstName = request.FirstName;
            worker.LastName = request.LastName;
            worker.ContextFlag = request.ContextFlag;
            worker.Role = request.Role;
            worker.DateStart = request.DateStart;
            worker.IsActive = true;

            var workerTeams = GetTeams(request.Teams, databaseContext);

            worker.WorkerTeams = new List<WorkerTeam>();
            foreach (var team in workerTeams)
            {
                worker.WorkerTeams.Add(new WorkerTeam { Team = team, Worker = worker });
            }

            databaseContext.SaveChanges();

            return worker;
        }

        public static ICollection<Team> GetTeams(List<WorkerTeamRequest> request, DatabaseContext context)
        {
            var teamsWorkerBelongsIn = new List<Team>();
            foreach (var requestTeam in request)
            {
                var team = GetTeamByTeamId(context, requestTeam.Id);
                teamsWorkerBelongsIn.Add(team);
            }

            return teamsWorkerBelongsIn;
        }

        public static Team GetTeamByTeamId(DatabaseContext databaseContext, int teamId)
        {
            return databaseContext.Teams
                .Where(x => x.Id == teamId)
                .Include(x => x.WorkerTeams)
                .ThenInclude(x => x.Worker)
                .ThenInclude(x => x.Allocations)
                .FirstOrDefault();
        }
    }
}
