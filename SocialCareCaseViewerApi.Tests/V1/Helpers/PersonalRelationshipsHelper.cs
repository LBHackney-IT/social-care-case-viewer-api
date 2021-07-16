using System;
using System.Collections.Generic;
using Bogus;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Infrastructure;
using Person = SocialCareCaseViewerApi.V1.Infrastructure.Person;

#nullable enable
namespace SocialCareCaseViewerApi.Tests.V1.Helpers
{
    public static class PersonalRelationshipsHelper
    {
        public static (Person, Person, PersonalRelationship?, PersonalRelationshipType?, PersonalRelationshipDetail?) SavePersonWithPersonalRelationshipToDatabase(
            DatabaseContext databaseContext,
            bool withRelationship = true,
            string relationshipType = "parent",
            bool hasEnded = false,
            string? details = null
        )
        {
            var person = TestHelpers.CreatePerson();
            var otherPerson = TestHelpers.CreatePerson();

            databaseContext.Persons.Add(person);
            databaseContext.Persons.Add(otherPerson);
            databaseContext.SaveChanges();

            if (withRelationship == false) return (person, otherPerson, null, null, null);

            var personalRelationshipType = CreatePersonalRelationshipType(relationshipType);
            var personalRelationship = CreatePersonalRelationship(person, otherPerson, personalRelationshipType, hasEnded);
            var personalRelationshipDetail = CreatePersonalRelationshipDetail(personalRelationship.Id, details);

            databaseContext.PersonalRelationshipTypes.Add(personalRelationshipType);
            databaseContext.PersonalRelationships.Add(personalRelationship);
            databaseContext.PersonalRelationshipDetails.Add(personalRelationshipDetail);

            databaseContext.SaveChanges();

            return (person, otherPerson, personalRelationship, personalRelationshipType, personalRelationshipDetail);
        }

        public static (Person, Person, PersonalRelationship?, PersonalRelationshipType?, PersonalRelationshipDetail?, PersonalRelationship?) SavePersonWithPersonalRelationshipAndOppositeToDatabase(
            DatabaseContext databaseContext,
            bool withRelationship = true,
            string relationshipType = "parent",
            int relationshipTypeId = 1,
            int oppositeRelationshipTypeId = 2,
            string oppositeRelationshipType = "child",
            bool hasEnded = false,
            string? details = null
        )
        {
            var person = TestHelpers.CreatePerson();
            var otherPerson = TestHelpers.CreatePerson();

            databaseContext.Persons.Add(person);
            databaseContext.Persons.Add(otherPerson);
            databaseContext.SaveChanges();

            if (withRelationship == false) return (person, otherPerson, null, null, null, null);

            var personalRelationshipType = CreatePersonalRelationshipType(relationshipType, relationshipTypeId, oppositeRelationshipTypeId);
            var personalRelationship = CreatePersonalRelationship(person, otherPerson, personalRelationshipType, hasEnded);
            var personalRelationshipDetail = CreatePersonalRelationshipDetail(personalRelationship.Id, details);

            var oppositePersonalRelationshipType = CreatePersonalRelationshipType(oppositeRelationshipType, oppositeRelationshipTypeId, relationshipTypeId);
            var inversePersonalRelationship = CreatePersonalRelationship(otherPerson, person, oppositePersonalRelationshipType, hasEnded);
            personalRelationshipType.InverseTypeId = oppositePersonalRelationshipType.Id;

            databaseContext.PersonalRelationships.Add(personalRelationship);
            databaseContext.PersonalRelationshipDetails.Add(personalRelationshipDetail);

            databaseContext.PersonalRelationshipTypes.Add(personalRelationshipType);
            databaseContext.PersonalRelationshipTypes.Add(oppositePersonalRelationshipType);

            databaseContext.PersonalRelationships.Add(inversePersonalRelationship);

            databaseContext.SaveChanges();

            return (person, otherPerson, personalRelationship, personalRelationshipType, personalRelationshipDetail, inversePersonalRelationship);
        }

        public static (Person, List<Person>, List<PersonalRelationship>) CreatePersonWithPersonalRelationships()
        {
            var person = TestHelpers.CreatePerson();
            var otherPersons = new List<Person>()
            {
                TestHelpers.CreatePerson(),
                TestHelpers.CreatePerson(),
                TestHelpers.CreatePerson()
            };

            var parent = CreatePersonalRelationshipType("parent");
            var child = CreatePersonalRelationshipType("child");
            var neighbour = CreatePersonalRelationshipType("neighbour");

            var personalrelationships = new List<PersonalRelationship>()
            {
                CreatePersonalRelationship(person, otherPersons[0], parent),
                CreatePersonalRelationship(person, otherPersons[1], child),
                CreatePersonalRelationship(person, otherPersons[2], neighbour)
            };

            person.PersonalRelationships = personalrelationships;

            var details = CreatePersonalRelationshipDetail(personalrelationships[0].Id);

            person.PersonalRelationships[0].Details = details;
            person.PersonalRelationships[1].Details = details;
            person.PersonalRelationships[2].Details = details;

            return (person, otherPersons, personalrelationships);
        }

        public static (Person, List<Person>, List<PersonalRelationship>, List<PersonalRelationshipDetail>) CreatePersonWithPersonalRelationshipsOfSameType()
        {
            var person = TestHelpers.CreatePerson();
            var otherPersons = new List<Person>()
            {
                TestHelpers.CreatePerson(),
                TestHelpers.CreatePerson()
            };

            var parent = CreatePersonalRelationshipType("parent");

            var personalrelationships = new List<PersonalRelationship>()
            {
                CreatePersonalRelationship(person, otherPersons[0], parent),
                CreatePersonalRelationship(person, otherPersons[1], parent),
            };

            person.PersonalRelationships = personalrelationships;

            var details = new List<PersonalRelationshipDetail>()
            {
                CreatePersonalRelationshipDetail(personalrelationships[0].Id),
                CreatePersonalRelationshipDetail(personalrelationships[1].Id),
            };

            person.PersonalRelationships[0].Details = details[0];
            person.PersonalRelationships[1].Details = details[1];

            return (person, otherPersons, personalrelationships, details);
        }

        public static PersonalRelationship CreatePersonalRelationship(
            Person person,
            Person otherPerson,
            PersonalRelationshipType type,
            bool hasEnded = false,
            long? id = null
        )
        {
            return new Faker<PersonalRelationship>()
                .RuleFor(pr => pr.Id, f => id ?? f.UniqueIndex)
                .RuleFor(pr => pr.PersonId, person.Id)
                .RuleFor(pr => pr.OtherPersonId, otherPerson.Id)
                .RuleFor(pr => pr.OtherPerson, otherPerson)
                .RuleFor(pr => pr.TypeId, type.Id)
                .RuleFor(pr => pr.Type, type)
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

        public static PersonalRelationshipType CreatePersonalRelationshipType(string description = "parent", long? id = null, long? inverseTypeId = null)
        {
            return new Faker<PersonalRelationshipType>()
                .RuleFor(prt => prt.Id, f => id ?? f.UniqueIndex)
                .RuleFor(prt => prt.InverseTypeId, f => inverseTypeId ?? f.UniqueIndex)
                .RuleFor(prt => prt.Description, f => description ?? f.Random.String2(20));
        }

        public static CreatePersonalRelationshipRequest CreatePersonalRelationshipRequest(
            long? personId = null,
            long? otherPersonId = null,
            long? typeId = null,
            string type = "parent",
            string? isMainCarer = null,
            string? isInformalCarer = null,
            string? details = null,
            string? createdBy = null
        )
        {
            return new Faker<CreatePersonalRelationshipRequest>()
                .RuleFor(pr => pr.PersonId, f => personId ?? f.UniqueIndex + 1)
                .RuleFor(pr => pr.OtherPersonId, f => otherPersonId ?? f.UniqueIndex + 2)
                .RuleFor(pr => pr.Type, type)
                .RuleFor(pr => pr.TypeId, f => typeId ?? f.UniqueIndex)
                .RuleFor(pr => pr.IsMainCarer, f => isMainCarer ?? f.Random.String2(1, "YNyn"))
                .RuleFor(pr => pr.IsInformalCarer, f => isInformalCarer ?? f.Random.String2(1, "YNyn"))
                .RuleFor(pr => pr.Details, f => details ?? f.Random.String2(1000))
                .RuleFor(pr => pr.CreatedBy, f => createdBy ?? f.Internet.Email());
        }

        public static (Person, Person) SavePersonAndOtherPersonToDatabase(DatabaseContext databaseContext)
        {
            var person = TestHelpers.CreatePerson();
            var otherPerson = TestHelpers.CreatePerson();

            databaseContext.Persons.Add(person);
            databaseContext.Persons.Add(otherPerson);
            databaseContext.SaveChanges();

            return (person, otherPerson);
        }
    }
}
