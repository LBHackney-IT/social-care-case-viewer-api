using Bogus;
using SocialCareCaseViewerApi.V1.Infrastructure;
using Person = SocialCareCaseViewerApi.V1.Infrastructure.Person;

namespace SocialCareCaseViewerApi.Tests.V1.Helpers.Gateway
{
    public static class PersonGatewayHelper
    {
        public static Person NewPerson()
        {
            return new Faker<Person>()
                .RuleFor(p => p.Id, f => f.UniqueIndex)
                .RuleFor(p => p.Title, f => f.Name.Prefix())
                .RuleFor(p => p.FirstName, f => f.Person.FirstName)
                .RuleFor(p => p.LastName, f => f.Person.FirstName)
                .RuleFor(p => p.FullName, f => f.Person.FullName)
                .RuleFor(p => p.DateOfBirth, f => f.Person.DateOfBirth)
                .RuleFor(p => p.DateOfDeath, f => f.Date.Recent())
                .RuleFor(p => p.Ethnicity, f => f.Random.String2(0, 30))
                .RuleFor(p => p.FirstLanguage, f => f.Random.String2(10, 100))
                .RuleFor(p => p.Religion, f => f.Random.String2(10, 80))
                .RuleFor(p => p.EmailAddress, f => f.Person.Email)
                .RuleFor(p => p.Gender, f => f.Random.String2(1, "MF"))
                .RuleFor(p => p.Nationality, f => f.Address.Country())
                .RuleFor(p => p.NhsNumber, f => f.Random.Number(int.MaxValue))
                .RuleFor(p => p.PersonIdLegacy, f => f.Random.String2(16))
                .RuleFor(p => p.AgeContext, f => f.Random.String2(1))
                .RuleFor(p => p.DataIsFromDmPersonsBackup, f => f.Random.String2(1))
                .RuleFor(p => p.SexualOrientation, f => f.Random.String2(100))
                .RuleFor(p => p.PreferredMethodOfContact, f => f.Random.String2(100))
                .RuleFor(p => p.Restricted, f => f.Random.String2(1));
        }

        public static Person StoreNewPerson(DatabaseContext databaseContext)
        {
            var person = NewPerson();

            databaseContext.Persons.Add(person);
            databaseContext.SaveChanges();

            return person;
        }
    }
}
