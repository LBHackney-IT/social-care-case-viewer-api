using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Domain;
using SocialCareCaseViewerApi.V1.Infrastructure;
using Address = SocialCareCaseViewerApi.V1.Domain.Address;
using DbAddress = SocialCareCaseViewerApi.V1.Infrastructure.Address;
using dbPhoneNumber = SocialCareCaseViewerApi.V1.Infrastructure.PhoneNumber;
using DbTeam = SocialCareCaseViewerApi.V1.Infrastructure.Team;
using DbWorker = SocialCareCaseViewerApi.V1.Infrastructure.Worker;
using PhoneNumber = SocialCareCaseViewerApi.V1.Domain.PhoneNumber;
using Team = SocialCareCaseViewerApi.V1.Domain.Team;
using Worker = SocialCareCaseViewerApi.V1.Domain.Worker;

namespace SocialCareCaseViewerApi.V1.Factories
{
    public static class EntityFactory
    {
        public static ResidentInformation ToDomain(this Person databaseEntity)
        {
            return new ResidentInformation
            {
                PersonId = databaseEntity.Id.ToString(),
                Title = databaseEntity.Title,
                FirstName = databaseEntity.FirstName,
                LastName = databaseEntity.LastName,
                DateOfBirth = databaseEntity.DateOfBirth?.ToString("O"),
                Gender = databaseEntity.Gender,
                Nationality = databaseEntity.Nationality,
                NhsNumber = databaseEntity.NhsNumber?.ToString(),

            };
        }
        public static List<ResidentInformation> ToDomain(this IEnumerable<Person> people)
        {
            return people.Select(p => p.ToDomain()).ToList();
        }

        public static Address ToDomain(this DbAddress address)
        {
            return new Address
            {
                AddressLine1 = address.AddressLines,
                PostCode = address.PostCode
            };
        }

        public static Worker ToDomain(this DbWorker worker, bool includeTeamData)
        {
            return new Worker
            {
                Id = worker.Id,
                Email = worker.Email,
                FirstName = worker.FirstName,
                LastName = worker.LastName,
                Role = worker.Role,
                AllocationCount = worker?.Allocations == null ? 0 : worker.Allocations.Where(x => x.CaseStatus.ToUpper() == "OPEN").Count(),
                Teams = includeTeamData ? worker.WorkerTeams.Select(x => new Team() { Id = x.Team.Id, Name = x.Team.Name }).ToList() : null
            };
        }

        public static List<Worker> ToDomain(this IEnumerable<DbWorker> workers, bool includeTeamData)
        {
            return workers.Select(w => w.ToDomain(includeTeamData)).ToList();
        }

        public static Team ToDomain(this DbTeam team)
        {
            return new Team
            {
                Id = team.Id,
                Name = team.Name
            };
        }

        public static List<Team> ToDomain(this IEnumerable<DbTeam> teams)
        {
            return teams.Select(t => t.ToDomain()).ToList();
        }

        public static AllocationSet ToEntity(this CreateAllocationRequest request, int workerId, DateTime allocationStartDate, string caseStatus)
        {
            return new AllocationSet
            {
                PersonId = request.MosaicId,
                WorkerId = workerId,
                AllocationStartDate = allocationStartDate,
                CaseStatus = caseStatus,
                CreatedBy = request.CreatedBy
            };
        }

        public static PersonOtherName ToEntity(this OtherName name, long personId, string createdBy)
        {
            return new PersonOtherName
            {
                FirstName = name.FirstName,
                LastName = name.LastName,
                PersonId = personId,
                CreatedBy = createdBy
            };
        }

        public static dbPhoneNumber ToEntity(this PhoneNumber number, long personId, string createdBy)
        {
            return new dbPhoneNumber
            {
                Number = number.Number.ToString(),
                Type = number.Type,
                PersonId = personId,
                CreatedBy = createdBy
            };
        }

        public static CaseNotesDocument ToEntity(this CreateCaseNoteRequest request)
        {
            GenericCaseNote note = new GenericCaseNote()
            {
                Timestamp = DateTime.Now.ToString("dd/MM/yyy hh:mm"),
                DateOfBirth = request.DateOfBirth.ToString(),
                FirstName = request.FirstName,
                LastName = request.LastName,
                FormName = request.FormName,
                FormNameOverall = request.FormNameOverall,
                WorkerEmail = request.WorkerEmail,
                MosaicId = request.PersonId.ToString()
            };

            //serialize core properties
            JObject coreProps = JObject.Parse(JsonConvert.SerializeObject(note));

            //take the custom properties
            JObject caseFormData = JObject.Parse(request.CaseFormData);

            //merge both to one object
            coreProps.Merge(caseFormData);

            return new CaseNotesDocument()
            {
                CaseFormData = coreProps.ToString()
            };
        }
    }
}
