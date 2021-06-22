using System;
using System.Collections.Generic;
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
            bool withRelationship = true,
            string relationshipType = "parent",
            string otherRelationshipType = "child",
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

            var (personalRelationshipType, _) = CreatePersonalRelationshipTypes(relationshipType, otherRelationshipType);
            var personalRelationship = CreatePersonalRelationship(person, otherPerson, personalRelationshipType, hasEnded);
            var personalRelationshipDetail = CreatePersonalRelationshipDetail(personalRelationship.Id, details);

            databaseContext.PersonalRelationshipTypes.Add(personalRelationshipType);
            databaseContext.PersonalRelationships.Add(personalRelationship);
            databaseContext.PersonalRelationshipDetails.Add(personalRelationshipDetail);
            databaseContext.SaveChanges();

            return (person, otherPerson, personalRelationship, personalRelationshipType, personalRelationshipDetail);
        }

        public static Person CreatePersonWithPersonalRelationships(bool withRelationships = true)
        {
            var person = TestHelpers.CreatePerson();
            var otherPerson = TestHelpers.CreatePerson();
            var (personalRelationshipType, _) = CreatePersonalRelationshipTypes("parent", "child");

            if (withRelationships)
            {
                var (personalRelationships, _) = CreatePersonalRelationshipsWithSomeTypes(person);
                person.PersonalRelationships = personalRelationships;
            }

            return person;
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

        public static (PersonalRelationshipType, PersonalRelationshipType) CreatePersonalRelationshipTypes(string? description = null, string? inverseDescription = null, bool isOwnInverse = false)
        {
            PersonalRelationshipType personalRelationshipType = new Faker<PersonalRelationshipType>()
                .RuleFor(prt => prt.Id, f => f.UniqueIndex)
                .RuleFor(prt => prt.Description, f => description ?? f.Random.String2(20));

            if (isOwnInverse)
            {
                personalRelationshipType.InverseTypeId = personalRelationshipType.Id;

                return (personalRelationshipType, new PersonalRelationshipType());
            }
            else
            {
                PersonalRelationshipType inversePersonalRelationshipType = new Faker<PersonalRelationshipType>()
                    .RuleFor(prt => prt.Id, f => f.UniqueIndex)
                    .RuleFor(prt => prt.Description, f => inverseDescription ?? f.Random.String2(20));

                personalRelationshipType.InverseTypeId = inversePersonalRelationshipType.Id;
                inversePersonalRelationshipType.InverseTypeId = personalRelationshipType.Id;

                return (personalRelationshipType, inversePersonalRelationshipType);
            }
        }

        public static List<PersonalRelationship> CreatePersonalRelationshipsWithAllTypes()
        {
            var person = TestHelpers.CreatePerson();

            var (acquaintance, _) = CreatePersonalRelationshipTypes("acquaintance", isOwnInverse: true);
            var (auntUncle, nieceNephew) = CreatePersonalRelationshipTypes("auntUncle", "nieceNephew");
            var (cousin, _) = CreatePersonalRelationshipTypes("cousin", isOwnInverse: true);
            var (exPartner, _) = CreatePersonalRelationshipTypes("exPartner", isOwnInverse: true);
            var (exSpouse, _) = CreatePersonalRelationshipTypes("exSpouse", isOwnInverse: true);
            var (fosterCarer, fosterChild) = CreatePersonalRelationshipTypes("fosterCarer", "fosterChild");
            var (fosterCarerSupportCarer, supportCarerFosterCarer) = CreatePersonalRelationshipTypes("fosterCarerSupportCarer", "supportCarerFosterCarer");
            var (friend, _) = CreatePersonalRelationshipTypes("friend", isOwnInverse: true);
            var (grandchild, grandparent) = CreatePersonalRelationshipTypes("grandchild", "grandparent");
            var (greatGrandchild, greatGrandparent) = CreatePersonalRelationshipTypes("greatGrandchild", "greatGrandparent");
            var (halfSibling, _) = CreatePersonalRelationshipTypes("halfSibling", isOwnInverse: true);
            var (inContactWith, _) = CreatePersonalRelationshipTypes("inContactWith", isOwnInverse: true);
            var (neighbour, _) = CreatePersonalRelationshipTypes("neighbour", isOwnInverse: true);
            var (other, _) = CreatePersonalRelationshipTypes("other", isOwnInverse: true);
            var (parent, child) = CreatePersonalRelationshipTypes("parent", "child");
            var (partner, _) = CreatePersonalRelationshipTypes("partner", isOwnInverse: true);
            var (privateFosterCarer, privateFosterChild) = CreatePersonalRelationshipTypes("privateFosterCarer", "privateFosterChild");
            var (sibling, _) = CreatePersonalRelationshipTypes("sibling", isOwnInverse: true);
            var (spouse, _) = CreatePersonalRelationshipTypes("spouse", isOwnInverse: true);
            var (stepParent, stepChild) = CreatePersonalRelationshipTypes("stepParent", "stepChild");
            var (stepSibling, _) = CreatePersonalRelationshipTypes("stepSibling", isOwnInverse: true);
            var (unbornChild, parentOfUnbornChild) = CreatePersonalRelationshipTypes("unbornChild", "parentOfUnbornChild");
            var (unbornSibling, siblingOfUnbornChild) = CreatePersonalRelationshipTypes("unbornSibling", "siblingOfUnbornChild");

            return new List<PersonalRelationship>()
            {
                CreatePersonalRelationship(person, TestHelpers.CreatePerson(), acquaintance),
                CreatePersonalRelationship(person, TestHelpers.CreatePerson(), auntUncle),
                CreatePersonalRelationship(person, TestHelpers.CreatePerson(), child),
                CreatePersonalRelationship(person, TestHelpers.CreatePerson(), cousin),
                CreatePersonalRelationship(person, TestHelpers.CreatePerson(), exPartner),
                CreatePersonalRelationship(person, TestHelpers.CreatePerson(), exSpouse),
                CreatePersonalRelationship(person, TestHelpers.CreatePerson(), fosterCarer),
                CreatePersonalRelationship(person, TestHelpers.CreatePerson(), fosterCarerSupportCarer),
                CreatePersonalRelationship(person, TestHelpers.CreatePerson(), fosterChild),
                CreatePersonalRelationship(person, TestHelpers.CreatePerson(), friend),
                CreatePersonalRelationship(person, TestHelpers.CreatePerson(), grandchild),
                CreatePersonalRelationship(person, TestHelpers.CreatePerson(), grandparent),
                CreatePersonalRelationship(person, TestHelpers.CreatePerson(), greatGrandchild),
                CreatePersonalRelationship(person, TestHelpers.CreatePerson(), greatGrandparent),
                CreatePersonalRelationship(person, TestHelpers.CreatePerson(), halfSibling),
                CreatePersonalRelationship(person, TestHelpers.CreatePerson(), inContactWith),
                CreatePersonalRelationship(person, TestHelpers.CreatePerson(), neighbour),
                CreatePersonalRelationship(person, TestHelpers.CreatePerson(), nieceNephew),
                CreatePersonalRelationship(person, TestHelpers.CreatePerson(), other),
                CreatePersonalRelationship(person, TestHelpers.CreatePerson(), parent),
                CreatePersonalRelationship(person, TestHelpers.CreatePerson(), parentOfUnbornChild),
                CreatePersonalRelationship(person, TestHelpers.CreatePerson(), partner),
                CreatePersonalRelationship(person, TestHelpers.CreatePerson(), privateFosterCarer),
                CreatePersonalRelationship(person, TestHelpers.CreatePerson(), privateFosterChild),
                CreatePersonalRelationship(person, TestHelpers.CreatePerson(), sibling),
                CreatePersonalRelationship(person, TestHelpers.CreatePerson(), siblingOfUnbornChild),
                CreatePersonalRelationship(person, TestHelpers.CreatePerson(), spouse),
                CreatePersonalRelationship(person, TestHelpers.CreatePerson(), stepChild),
                CreatePersonalRelationship(person, TestHelpers.CreatePerson(), stepParent),
                CreatePersonalRelationship(person, TestHelpers.CreatePerson(), stepSibling),
                CreatePersonalRelationship(person, TestHelpers.CreatePerson(), supportCarerFosterCarer),
                CreatePersonalRelationship(person, TestHelpers.CreatePerson(), unbornChild),
                CreatePersonalRelationship(person, TestHelpers.CreatePerson(), unbornSibling)
            };
        }

        public static (List<PersonalRelationship>, List<Person>) CreatePersonalRelationshipsWithSomeTypes(Person? providedPerson = null)
        {
            var person = providedPerson ?? TestHelpers.CreatePerson();

            var otherPersons = new List<Person>()
            {
                TestHelpers.CreatePerson(),
                TestHelpers.CreatePerson(),
                TestHelpers.CreatePerson(),
                TestHelpers.CreatePerson(),
                TestHelpers.CreatePerson()
            };

            var (acquaintance, _) = CreatePersonalRelationshipTypes("acquaintance", isOwnInverse: true);
            var (grandchild, grandparent) = CreatePersonalRelationshipTypes("grandchild", "grandparent");
            var (greatGrandchild, greatGrandparent) = CreatePersonalRelationshipTypes("greatGrandchild", "greatGrandparent");
            var (inContactWith, _) = CreatePersonalRelationshipTypes("inContactWith", isOwnInverse: true);
            var (neighbour, _) = CreatePersonalRelationshipTypes("neighbour", isOwnInverse: true);
            var (parent, child) = CreatePersonalRelationshipTypes("parent", "child");

            var personalrelationships = new List<PersonalRelationship>()
            {
                CreatePersonalRelationship(person, otherPersons[0], acquaintance),
                CreatePersonalRelationship(person, otherPersons[1], child),
                CreatePersonalRelationship(person, otherPersons[2], grandparent),
                CreatePersonalRelationship(person, otherPersons[3], neighbour),
                CreatePersonalRelationship(person, otherPersons[4], parent)
            };

            return (personalrelationships, otherPersons);
        }
    }
}
