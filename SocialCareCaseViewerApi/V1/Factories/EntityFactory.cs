using System.Collections.Generic;
using System.Linq;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Domain;
using SocialCareCaseViewerApi.V1.Infrastructure;
using Address = SocialCareCaseViewerApi.V1.Domain.Address;
using DbAddress = SocialCareCaseViewerApi.V1.Infrastructure.Address;
using DbTeam = SocialCareCaseViewerApi.V1.Infrastructure.Team;
using DbWorker = SocialCareCaseViewerApi.V1.Infrastructure.Worker;
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

        public static Worker ToDomain(this DbWorker worker)
        {
            return new Worker
            {
                FirstName = worker.FirstName,
                LastName = worker.LastName,
                Id = worker.Id
            };
        }

        public static List<Worker> ToDomain(this IEnumerable<DbWorker> workers)
        {
            return workers.Select(w => w.ToDomain()).ToList();
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

        public static AllocationSet ToEntity(this CreateAllocationRequest request)
        {
            return new AllocationSet
            {
                Id = request.MosaicId.ToString(),
                WorkerEmail = request.WorkerEmail,
                AllocatedWorkerTeam = request.AllocatedWorkerTeam
            };
        }
    }
}
