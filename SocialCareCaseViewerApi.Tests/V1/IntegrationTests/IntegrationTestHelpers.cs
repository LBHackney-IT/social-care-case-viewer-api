using System.Collections.Generic;
using System.Linq;
using Bogus;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Infrastructure;
using DomainWorker = SocialCareCaseViewerApi.V1.Domain.Worker;
using DomainTeam = SocialCareCaseViewerApi.V1.Domain.Team;

namespace SocialCareCaseViewerApi.Tests.V1.IntegrationTests
{
    public static class IntegrationTestHelpers
    {
        public static (DomainWorker, Team) SetupExistingWorker(DatabaseContext context)
        {
            var workerId = new Faker().Random.Int(1, 100);
            var workerContext = new Faker().Random.String2(1, "AC");

            var team = new Faker<Team>()
                .RuleFor(t => t.Id, f => f.UniqueIndex + 1)
                .RuleFor(t => t.Context, f => workerContext)
                .RuleFor(t => t.Name, f => f.Name.JobType()).Generate();

            context.Teams.Add(team);
            context.SaveChanges();

            var newTeam = new Faker<Team>()
                .RuleFor(t => t.Id, f => f.UniqueIndex + team.Id)
                .RuleFor(t => t.Context, f => workerContext)
                .RuleFor(t => t.Name, f => f.Name.JobType()).Generate();

            context.Teams.Add(newTeam);
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

            var domainWorker = new DomainWorker
            {
                Id = worker.Id,
                Email = worker.Email,
                FirstName = worker.FirstName,
                LastName = worker.LastName,
                Role = worker.Role,
                ContextFlag = worker.ContextFlag,
                CreatedBy = worker.CreatedBy,
                DateStart = worker.DateStart,
                AllocationCount = worker.Allocations?.Where(x => x.CaseStatus.ToUpper() == "OPEN").Count() ?? 0,
                Teams = (worker.WorkerTeams?.Select(x =>
                            new DomainTeam { Id = x.Team.Id, Name = x.Team.Name, Context = x.Team.Context }).ToList()) ??
                        new List<DomainTeam>()
            };

            return (domainWorker, newTeam);
        }
    }
}
