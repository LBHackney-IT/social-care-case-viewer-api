using System;
using System.Collections.Generic;
using Bogus;
using Microsoft.EntityFrameworkCore;
using SocialCareCaseViewerApi.Tests.V1.Helpers;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Infrastructure;
using DbPerson = SocialCareCaseViewerApi.V1.Infrastructure.Person;
using InfrastructureCaseStatus = SocialCareCaseViewerApi.V1.Infrastructure.CaseStatus;

namespace SocialCareCaseViewerApi.Tests.V1.IntegrationTests
{
    public static class IntegrationTestHelpers
    {
        public static DbPerson CreateExistingPerson(DatabaseContext context, int? personId = null, string ageContext = null, string restricted = null)
        {
            var person = new Faker<DbPerson>()
                .RuleFor(p => p.Id, f => personId ?? f.IndexGlobal + f.Random.Int(0))
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

            context.Persons.Add(person);
            context.SaveChanges();

            return person;
        }

        public static (Worker, Team) SetupExistingWorker(DatabaseContext context)
        {
            var workerId = new Faker().Random.Int(1, 100);
            var workerContext = new Faker().Random.String2(1, "AC");

            var team = CreateTeam(context, workerContext);

            var workerTeam = new Faker<WorkerTeam>()
                .RuleFor(t => t.Id, f => f.IndexGlobal + f.Random.Int(0))
                .RuleFor(t => t.WorkerId, f => workerId)
                .RuleFor(t => t.TeamId, f => team.Id)
                .RuleFor(t => t.Team, team).Generate();

            context.WorkerTeams.Add(workerTeam);

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

            context.Workers.Add(worker);
            context.SaveChanges();

            return (worker, team);
        }

        public static Team CreateTeam(DatabaseContext context, string workerContext)
        {
            var team = new Faker<Team>()
                .RuleFor(t => t.Id, f => f.IndexGlobal + f.Random.Int(0))
                .RuleFor(t => t.Context, f => workerContext)
                .RuleFor(t => t.Name, f => f.Random.String2(10, 15)).Generate();

            context.Teams.Add(team);
            context.SaveChanges();

            return team;
        }

        public static UpdateWorkerRequest CreatePatchRequest(Worker worker, WorkerTeamRequest teamRequest)
        {
            var updateWorkerRequest = new Faker<UpdateWorkerRequest>()
                .RuleFor(f => f.WorkerId, f => worker.Id)
                .RuleFor(f => f.ModifiedBy, f => f.Person.Email)
                .RuleFor(f => f.FirstName, f => worker.FirstName)
                .RuleFor(f => f.LastName, f => worker.LastName)
                .RuleFor(f => f.ContextFlag, f => worker.ContextFlag)
                .RuleFor(f => f.Teams, f => new List<WorkerTeamRequest> { teamRequest })
                .RuleFor(f => f.Role, f => worker.Role)
                .RuleFor(f => f.DateStart, f => worker.DateStart).Generate();

            return updateWorkerRequest;
        }

        public static CreateAllocationRequest CreateAllocationRequest(long personId, int teamId, int workerId, Worker createdByWorker)
        {
            var createAllocationRequest = new Faker<CreateAllocationRequest>()
                .RuleFor(c => c.MosaicId, f => personId)
                .RuleFor(c => c.AllocatedTeamId, f => teamId)
                .RuleFor(c => c.AllocatedWorkerId, f => workerId)
                .RuleFor(c => c.CreatedBy, f => createdByWorker.Email)
                .RuleFor(c => c.AllocationStartDate, f => f.Date.Recent()).Generate();

            return createAllocationRequest;
        }

        public static (List<InfrastructureCaseStatus>, DbPerson) SavePersonWithMultipleCaseStatusesToDatabase(
          DatabaseContext databaseContext, bool addClosedCaseStatuses = false)
        {
            var caseStatuses = new List<InfrastructureCaseStatus>();
            var person = TestHelpers.CreatePerson();

            for (var i = 0; i < new Random().Next(1, 5); i++)
            {
                var caseStatus = TestHelpers.CreateCaseStatus(person.Id, resident: person, startDate: DateTime.Today.AddDays(-(20 + 2 * i)));
                caseStatus.EndDate = null;
                caseStatuses.Add(caseStatus);
            }

            if (addClosedCaseStatuses)
            {
                for (var i = 0; i < new Random().Next(1, 5); i++)
                {
                    var startDate = DateTime.Today.AddDays(-(50 + 2 * i));
                    var endDate = startDate.AddDays(2);
                    caseStatuses.Add(TestHelpers.CreateCaseStatus(person.Id, resident: person, startDate: startDate, endDate: endDate));
                }
            }

            databaseContext.CaseStatuses.AddRange(caseStatuses);
            databaseContext.Persons.Add(person);
            databaseContext.SaveChanges();

            return (caseStatuses, person);
        }

        public static MashReferral SaveMashReferralToDatabase(DatabaseContext databaseContext, string referrer = null, DateTime? createdTime = null)
        {
            var mashReferral = new Faker<MashReferral>()
                .RuleFor(w => w.Id, f => f.UniqueIndex)
                .RuleFor(w => w.Referrer, f => referrer ?? f.Person.FullName)
                .RuleFor(w => w.ReferralCreatedAt, f => createdTime ?? f.Date.Past())
                .RuleFor(w => w.RequestedSupport, f => f.Random.String2(20))
                .RuleFor(w => w.ReferralDocumentURI, f => f.Random.String2(20))
                .RuleFor(w => w.Stage, f => f.Random.String2(20))
                .Generate(); ;

            databaseContext.MashReferrals.Add(mashReferral);
            databaseContext.SaveChanges();

            return mashReferral;
        }
    }
}
