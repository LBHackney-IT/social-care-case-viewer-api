using System;
using System.Collections.Generic;
using System.Linq;
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

        public static Worker ToDomain(this DbWorker worker, List<dynamic> allocationDetails)
        {
            var allocation = allocationDetails.Where(x => x.WorkerId == worker.Id).FirstOrDefault();

            return new Worker
            {
                Id = worker.Id,
                Email = worker.Email,
                FirstName = worker.FirstName,
                LastName = worker.LastName,
                TeamId = worker.TeamId,
                Role = worker.Role,
                AllocationCount = allocation?.AllocationCount == null ? 0 : allocation.AllocationCount
            };
        }

        public static List<Worker> ToDomain(this IEnumerable<DbWorker> workers, List<dynamic> allocationDetails)
        {
            return workers.Select(w => w.ToDomain(allocationDetails)).ToList();
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

        public static AllocationSet ToEntity(this CreateAllocationRequest request, long workerId, DateTime allocationStartDate, string caseStatus)
        {
            return new AllocationSet
            {
                MosaicId = request.MosaicId,
                WorkerId = workerId,
                AllocationStartDate = allocationStartDate,
                CaseStatus = caseStatus
            };
        }

        public static PersonOtherName ToEntity(this OtherName name, long personId)
        {
            return new PersonOtherName
            {
                FirstName = name.FirstName,
                LastName = name.LastName,
                PersonId = personId
            };
        }

        public static dbPhoneNumber ToEntity(this PhoneNumber number, long personId)
        {
            return new dbPhoneNumber
            {
                Number = number.Number,
                Type = number.Type,
                PersonId = personId
            };
        }
    }
}
