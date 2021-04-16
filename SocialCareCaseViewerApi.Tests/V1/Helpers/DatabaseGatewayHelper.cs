using Bogus;
using SocialCareCaseViewerApi.V1.Infrastructure;
using System.Collections.Generic;
using dbPerson = SocialCareCaseViewerApi.V1.Infrastructure.Person;

namespace SocialCareCaseViewerApi.Tests.V1.Helpers
{
    public static class DatabaseGatewayHelper
    {
        public static Worker CreateWorkerDatabaseEntity(
            int id = 1,
            string email = "testemail@example.com",
            string firstName = "test-first-name",
            string lastName = "test-last-name")
        {
            return new Worker { Id = id, Email = email, FirstName = firstName, LastName = lastName };
        }

        public static Team CreateTeamDatabaseEntity(
            List<WorkerTeam> workerTeams,
            string content = "t",
            int id = 1,
            string name = "test-name"
        )
        {
            return new Team { Context = content, Id = id, Name = name, WorkerTeams = workerTeams };
        }

        public static WorkerTeam CreateWorkerTeamDatabaseEntity(
            Worker worker,
            int id = 1,
            int workerId = 1,
            int teamId = 1
            )
        {
            return new WorkerTeam { Worker = worker, Id = id, WorkerId = workerId, TeamId = teamId };
        }

        public static dbPerson CreatePersonDatabaseEntity()
        {
            string firstName = "First";
            string lastName = "Last";

            return new Faker<dbPerson>()
                .RuleFor(p => p.FirstLanguage, f => f.Random.Word())
                .RuleFor(p => p.SexualOrientation, f => f.Random.Word())
                .RuleFor(p => p.AgeContext, f => f.Random.String2(1))
                .RuleFor(p => p.CreatedAt, f => f.Date.Past())
                .RuleFor(p => p.EmailAddress, f => f.Internet.Email())
                .RuleFor(p => p.Ethnicity, f => f.Random.Word())
                .RuleFor(p => p.CreatedBy, f => f.Internet.Email())
                .RuleFor(p => p.DataIsFromDmPersonsBackup, f => f.Random.String2(1))
                .RuleFor(p => p.DateOfBirth, f => f.Date.Past())
                .RuleFor(p => p.DateOfDeath, f => f.Date.Past())
                .RuleFor(p => p.FirstName, firstName)
                .RuleFor(p => p.LastName, f => lastName)
                .RuleFor(p => p.FullName, $"{firstName} {lastName}") //TODO: how to combine two props?
                .RuleFor(p => p.Title, f => f.Random.String2(2))
                .RuleFor(p => p.Gender, f => f.Random.String2(1))
                .RuleFor(p => p.LastModifiedAt, f => f.Date.Past())
                .RuleFor(p => p.LastModifiedBy, f => f.Internet.Email())
                .RuleFor(p => p.Nationality, f => f.Random.Word())
                .RuleFor(p => p.NhsNumber, f => f.Random.Number())
                .RuleFor(p => p.PersonIdLegacy, f => f.Random.String2(16))
                .RuleFor(p => p.PreferredMethodOfContact, f => f.Random.Word())
                .RuleFor(p => p.Religion, f => f.Random.Word())
                .RuleFor(p => p.Restricted, f => f.Random.String2(1));
        }

        public static Address CreateAddressDatabaseEntity(
            long? personId = null,
            string isDisplayAddress = null
        )
        {
            return new Faker<Address>()
                 .RuleFor(a => a.AddressLines, f => f.Address.FullAddress())
                 .RuleFor(a => a.CreatedAt, f => f.Date.Past())
                 .RuleFor(a => a.CreatedBy, f => f.Internet.Email())
                 .RuleFor(a => a.DataIsFromDmPersonsBackup, f => f.Random.String2(1))
                 .RuleFor(a => a.EndDate, f => f.Date.Past())
                 .RuleFor(a => a.IsDisplayAddress, f => string.IsNullOrEmpty(isDisplayAddress) ? f.Random.String2(1) : isDisplayAddress)
                 .RuleFor(a => a.LastModifiedAt, f => f.Date.Past())
                 .RuleFor(a => a.LastModifiedBy, f => f.Internet.Email())
                 .RuleFor(a => a.PersonAddressId, f => f.Random.Int())
                 .RuleFor(a => a.PersonId, personId)
                 .RuleFor(a => a.PostCode, f => f.Address.ZipCode())
                 .RuleFor(a => a.Uprn, f => f.Random.Int());
        }

        internal static PhoneNumber CreatePhoneNumberEntity(long personId)
        {
            return new Faker<PhoneNumber>()
                .RuleFor(p => p.CreatedAt, p => p.Date.Past())
                .RuleFor(p => p.CreatedBy, p => p.Internet.Email())
                .RuleFor(p => p.LastModifiedAt, p => p.Date.Past())
                .RuleFor(p => p.LastModifiedBy, p => p.Internet.Email())
                .RuleFor(p => p.Number, p => p.Phone.PhoneNumber())
                .RuleFor(p => p.PersonId, personId)
                .RuleFor(p => p.Type, p => p.Random.String2(5));
        }

        internal static PersonOtherName CreatePersonOtherNameDatabaseEntity(long personId)
        {
            return new Faker<PersonOtherName>()
                .RuleFor(p => p.CreatedAt, p => p.Date.Past())
                .RuleFor(p => p.CreatedBy, p => p.Internet.Email())
                .RuleFor(p => p.LastModifiedAt, p => p.Date.Past())
                .RuleFor(p => p.LastModifiedBy, p => p.Internet.Email())
                .RuleFor(p => p.FirstName, p => p.Name.FirstName())
                .RuleFor(p => p.LastName, p => p.Name.LastName())
                .RuleFor(p => p.PersonId, personId);
        }
    }
}
