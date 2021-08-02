using System;
using System.Collections.Generic;
using Bogus;
using Microsoft.EntityFrameworkCore;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Infrastructure;
using DbPerson = SocialCareCaseViewerApi.V1.Infrastructure.Person;

namespace SocialCareCaseViewerApi.Tests.V1.IntegrationTests
{
    public static class IntegrationTestHelpers
    {
        public static DbPerson CreateExistingPerson(DatabaseContext context, int? personId = null, string ageContext = null, string restricted = null)
        {
            var person = new Faker<DbPerson>()
                .RuleFor(p => p.Id, f => personId ?? f.UniqueIndex + 1)
                .RuleFor(p => p.Title, f => f.Name.Prefix())
                .RuleFor(p => p.FirstName, f => f.Person.FirstName)
                .RuleFor(p => p.LastName, f => f.Person.FirstName)
                .RuleFor(p => p.FullName, f => f.Person.FullName)
                .RuleFor(p => p.DateOfBirth, f => f.Person.DateOfBirth)
                .RuleFor(p => p.DateOfDeath, f => f.Date.Recent())
                .RuleFor(p => p.Ethnicity, f => f.Commerce.Color())
                .RuleFor(p => p.FirstLanguage, f => f.Random.Word())
                .RuleFor(p => p.Religion, f => f.Random.String2(1, 5))
                .RuleFor(p => p.EmailAddress, f => f.Person.Email)
                .RuleFor(p => p.Gender, f => f.Random.String2(1, "MF"))
                .RuleFor(p => p.Nationality, f => f.Address.Country())
                .RuleFor(p => p.NhsNumber, f => f.Random.Number(int.MaxValue))
                .RuleFor(p => p.PersonIdLegacy, f => f.Random.String2(16))
                .RuleFor(p => p.AgeContext, f => ageContext ?? f.Random.String2(1, "AC"))
                .RuleFor(p => p.DataIsFromDmPersonsBackup, f => f.Random.String2(1, "YN"))
                .RuleFor(p => p.SexualOrientation, f => f.Random.String2(10))
                .RuleFor(p => p.PreferredMethodOfContact, f => f.Random.String2(10))
                .RuleFor(p => p.Restricted, f => restricted ?? f.Random.String2(1, "YN")).Generate();

            var insertPersonQuery = SeedPerson(person);

            context.Database.ExecuteSqlRaw(insertPersonQuery);

            return person;
        }

        public static (Worker, Team, WorkerTeam) SetupExistingWorker(DatabaseContext context)
        {
            var workerId = new Faker().Random.Int(1, 100);
            var workerContext = new Faker().Random.String2(1, "AC");

            var team = new Faker<Team>()
                .RuleFor(t => t.Id, f => f.UniqueIndex + 1)
                .RuleFor(t => t.Context, f => workerContext)
                .RuleFor(t => t.Name, f => f.Random.String2(10, 15)).Generate();

            var insertTeamQuery = SeedTeam(team);

            context.Database.ExecuteSqlRaw(insertTeamQuery);

            var workerTeam = new Faker<WorkerTeam>()
                .RuleFor(t => t.Id, f => f.UniqueIndex + 1)
                .RuleFor(t => t.WorkerId, f => workerId)
                .RuleFor(t => t.TeamId, f => team.Id)
                .RuleFor(t => t.Team, team).Generate();

            var insertWorkerTeamQuery = SeedWorkerTeam(workerTeam);

            context.Database.ExecuteSqlRaw(insertWorkerTeamQuery);

            var worker = new Faker<Worker>().RuleFor(w => w.Id, workerId)
                .RuleFor(w => w.FirstName, f => f.Person.FirstName)
                .RuleFor(w => w.LastName, f => f.Person.LastName)
                .RuleFor(w => w.Email, f => f.Person.Email)
                .RuleFor(w => w.Role, f => f.Name.JobTitle())
                .RuleFor(w => w.ContextFlag, f => workerContext)
                .RuleFor(w => w.CreatedBy, f => f.Person.Email)
                .RuleFor(w => w.CreatedAt, f => f.Date.Soon())
                .RuleFor(w => w.DateStart, f => f.Date.Recent())
                .RuleFor(w => w.DateEnd, f => f.Date.Soon())
                .RuleFor(w => w.CreatedAt, f => f.Date.Soon())
                .RuleFor(w => w.LastModifiedAt, f => f.Date.Soon())
                .RuleFor(w => w.IsActive, true)
                .RuleFor(w => w.Allocations, new List<AllocationSet>())
                .RuleFor(w => w.WorkerTeams, new List<WorkerTeam> { workerTeam })
                .RuleFor(w => w.LastModifiedBy, f => f.Person.Email).Generate();

            var insertWorkerQuery = SeedWorker(worker);

            context.Database.ExecuteSqlRaw(insertWorkerQuery);

            return (worker, team, workerTeam);
        }

        public static Team CreateAnotherTeam(DatabaseContext context, string workerContext)
        {
            var team = new Faker<Team>()
                .RuleFor(t => t.Id, f => f.UniqueIndex + 1)
                .RuleFor(t => t.Context, f => workerContext)
                .RuleFor(t => t.Name, f => f.Random.String2(10, 15)).Generate();

            var insertTeamQuery = SeedTeam(team);

            context.Database.ExecuteSqlRaw(insertTeamQuery);

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

        public static CreateAllocationRequest CreateAllocationRequest(long personId, int teamId, int workerId, Worker createdByWorker)
        {
            var createAllocationRequest = new Faker<CreateAllocationRequest>()
                .RuleFor(c => c.MosaicId, f => personId)
                .RuleFor(c => c.AllocatedTeamId, f => teamId)
                .RuleFor(c => c.AllocatedWorkerId, f => workerId)
                .RuleFor(c => c.CreatedBy, f => createdByWorker.Email)
                .RuleFor(c => c.AllocationStartDate, DateTime.Now);

            return createAllocationRequest;
        }

        private static string SeedPerson(DbPerson person)
        {
            var insertPersonQuery = $@"insert into dbo.dm_persons 
            (person_id, ssda903_id, nhs_id, scn_id, upn_id, former_upn_id, full_name, 
            title, first_name, last_name, date_of_birth, date_of_death, gender, 
            restricted, person_id_legacy, full_ethnicity_code, country_of_birth_code, is_child_legacy, is_adult_legacy, 
            nationality, religion, marital_status, first_language, fluency_in_english, email_address, 
            context_flag, scra_id, interpreter_required, from_dm_person) 
            values ({person.Id}, NULL, {person.NhsNumber}, NULL, NULL, NULL, '{person.FullName}', 
            '{person.Title}', '{person.FirstName}', '{person.LastName}', '{person.DateOfBirth?.ToString("s")}', '{person.DateOfDeath?.ToString("s")}', '{person.Gender}', 
            '{person.Restricted}', '{person.PersonIdLegacy}', '{person.Ethnicity}', NULL, 'Y', 'Y', 
            '{person.Nationality}', '{person.Religion}', NULL, '{person.FirstLanguage}', 'N', '{person.EmailAddress}', 
            '{person.AgeContext}', NULL, 'N', '{person.DataIsFromDmPersonsBackup}');";

            return insertPersonQuery;
        }

        private static string SeedWorker(Worker worker)
        {
            var insertWorkerQuery = $@"insert into dbo.sccv_worker 
            (id, email, first_name, last_name, role, context_flag, created_by, date_start, date_end, last_modified_by, created_at, last_modified_at, is_active ) 
            values ({worker.Id}, '{worker.Email}', '{worker.FirstName}', '{worker.LastName}', '{worker.Role}', 
            '{worker.ContextFlag}', '{worker.CreatedBy}', '{worker.DateStart?.ToString("s")}', '{worker.DateEnd?.ToString("s")}', 
            '{worker.LastModifiedBy}', '{worker.CreatedAt?.ToString("s")}', '{worker.LastModifiedAt?.ToString("s")}', {worker.IsActive});";

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
