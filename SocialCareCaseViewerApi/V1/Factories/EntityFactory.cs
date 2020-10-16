using System.Collections.Generic;
using System.Linq;
using SocialCareCaseViewerApi.V1.Domain;
using SocialCareCaseViewerApi.V1.Infrastructure;
using Address = SocialCareCaseViewerApi.V1.Domain.Address;
using DbAddress = SocialCareCaseViewerApi.V1.Infrastructure.Address;

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
    }
}
