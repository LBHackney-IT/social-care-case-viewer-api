using System;
using Bogus;
using SocialCareCaseViewerApi.V1.Infrastructure;
using Person = SocialCareCaseViewerApi.V1.Infrastructure.Person;

#nullable enable
namespace SocialCareCaseViewerApi.Tests.V1.Helpers
{
    public static class PersonalRelationshipsHelper
    {
        public static (Person, Person, PersonalRelationship?, PersonalRelationshipType?, PersonalRelationshipDetail?) SavePersonWithPersonalRelationshipToDatabase(
            DatabaseContext databaseContext,
            Boolean withRelationship = true,
            string relationshipType = "parent",
            string otherRelationshipType = "child",
            Boolean hasEnded = false,
            string? details = null
        )
        {
            var person = TestHelpers.CreatePerson();
            var otherPerson = TestHelpers.CreatePerson();

            databaseContext.Persons.Add(person);
            databaseContext.Persons.Add(otherPerson);
            databaseContext.SaveChanges();

            if (withRelationship == false) return (person, otherPerson, null, null, null);

            var (personalRelationshipType, _) = CreatePersonalRelationshipTypes(relationshipType, otherRelationshipType);
            var personalRelationship = CreatePersonalRelationship(person.Id, otherPerson.Id, personalRelationshipType.Id, hasEnded);
            var personalRelationshipDetail = CreatePersonalRelationshipDetail(personalRelationship.Id, details);

            databaseContext.PersonalRelationshipTypes.Add(personalRelationshipType);
            databaseContext.PersonalRelationships.Add(personalRelationship);
            databaseContext.PersonalRelationshipDetails.Add(personalRelationshipDetail);
            databaseContext.SaveChanges();

            return (person, otherPerson, personalRelationship, personalRelationshipType, personalRelationshipDetail);
        }

        public static PersonalRelationship CreatePersonalRelationship(
            long personId,
            long otherPersonId,
            long typeId,
            Boolean hasEnded = false
        )
        {
            return new Faker<PersonalRelationship>()
                .RuleFor(pr => pr.Id, f => f.UniqueIndex)
                .RuleFor(pr => pr.PersonId, personId)
                .RuleFor(pr => pr.OtherPersonId, otherPersonId)
                .RuleFor(pr => pr.TypeId, typeId)
                .RuleFor(pr => pr.StartDate, f => f.Date.Past())
                .RuleFor(pr => pr.EndDate, f => hasEnded ? f.Date.Future() : (DateTime?) null)
                .RuleFor(pr => pr.IsInformalCarer, f => f.Random.String2(1, "YN"))
                .RuleFor(pr => pr.ParentalResponsibility, f => f.Random.String2(1, "YN"));
        }

        public static PersonalRelationshipDetail CreatePersonalRelationshipDetail(long personalRelationshipId, string? details = null)
        {
            return new Faker<PersonalRelationshipDetail>()
                .RuleFor(prd => prd.Id, f => f.UniqueIndex)
                .RuleFor(prd => prd.PersonalRelationshipId, personalRelationshipId)
                .RuleFor(prd => prd.Details, f => details ?? f.Random.String2(1000));
        }

        public static (PersonalRelationshipType, PersonalRelationshipType) CreatePersonalRelationshipTypes(string? description = null, string? inverseDescription = null)
        {
            PersonalRelationshipType personalRelationshipType = new Faker<PersonalRelationshipType>()
                .RuleFor(prt => prt.Id, f => f.UniqueIndex)
                .RuleFor(prt => prt.Description, f => description ?? f.Random.String2(20));

            PersonalRelationshipType inversePersonalRelationshipType = new Faker<PersonalRelationshipType>()
                .RuleFor(prt => prt.Id, f => f.UniqueIndex)
                .RuleFor(prt => prt.Description, f => inverseDescription ?? f.Random.String2(20));

            personalRelationshipType.InverseTypeId = inversePersonalRelationshipType.Id;
            inversePersonalRelationshipType.InverseTypeId = personalRelationshipType.Id;

            return (personalRelationshipType, inversePersonalRelationshipType);
        }
    }
}
