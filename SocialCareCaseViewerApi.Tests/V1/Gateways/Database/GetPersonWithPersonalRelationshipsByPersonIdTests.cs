using System.Linq;
using FluentAssertions;
using MongoDB.Driver;
using Moq;
using NUnit.Framework;
using SocialCareCaseViewerApi.Tests.V1.Helpers;
using SocialCareCaseViewerApi.V1.Gateways;

namespace SocialCareCaseViewerApi.Tests.V1.Gateways.Database
{
    [TestFixture]
    public class GetPersonWithPersonalRelationshipsByPersonIdTests : DatabaseTests
    {
        private DatabaseGateway _databaseGateway;
        private Mock<IProcessDataGateway> _mockProcessDataGateway = new Mock<IProcessDataGateway>();

        [SetUp]
        public void Setup()
        {
            _databaseGateway = new DatabaseGateway(DatabaseContext, _mockProcessDataGateway.Object);
        }

        [Test]
        public void WhenThereAreNoRelationshipsReturnsEmptyListForPersonalRelationships()
        {
            var (person, _, _, _, _) = PersonalRelationshipsHelper.SavePersonWithPersonalRelationshipToDatabase(DatabaseContext, withRelationship: false);

            var response = _databaseGateway.GetPersonWithPersonalRelationshipsByPersonId(person.Id);

            response.PersonalRelationships.Should().BeEmpty();
        }

        [Test]
        public void WhenThereIsARelationshipReturnsThePersonalRelationship()
        {
            var (person, _, personalRelationship, _, _) = PersonalRelationshipsHelper.SavePersonWithPersonalRelationshipToDatabase(DatabaseContext);

            var response = _databaseGateway.GetPersonWithPersonalRelationshipsByPersonId(person.Id);

            response.PersonalRelationships.FirstOrDefault().Should().BeEquivalentTo(personalRelationship);
        }

        [Test]
        public void WhenThereIsARelationshipReturnsTheDetailsOfTheOtherPerson()
        {
            var (person, otherPerson, _, _, _) = PersonalRelationshipsHelper.SavePersonWithPersonalRelationshipToDatabase(DatabaseContext);

            var response = _databaseGateway.GetPersonWithPersonalRelationshipsByPersonId(person.Id);
            var otherPersonInResponse = response.PersonalRelationships.FirstOrDefault().OtherPerson;

            otherPersonInResponse.Should().BeEquivalentTo(otherPerson);
        }

        [Test]
        public void WhenThereIsARelationshipReturnsTheDescriptionOfThePersonalRelationshipType()
        {
            var (person, _, _, personalRelationshipType, _) = PersonalRelationshipsHelper.SavePersonWithPersonalRelationshipToDatabase(DatabaseContext, relationshipType: "stepParent", otherRelationshipType: "stepChild");

            var response = _databaseGateway.GetPersonWithPersonalRelationshipsByPersonId(person.Id);
            var personalRelationshipTypeInResponse = response.PersonalRelationships.FirstOrDefault().Type;

            personalRelationshipTypeInResponse.Description.Should().Equals(personalRelationshipType.Description);
        }

        [Test]
        public void WhenThereIsARelationshipHasAnEndDateReturnsEmptyListForPersonalRelationships()
        {
            var (person, _, _, _, _) = PersonalRelationshipsHelper.SavePersonWithPersonalRelationshipToDatabase(DatabaseContext, hasEnded: true);

            var response = _databaseGateway.GetPersonWithPersonalRelationshipsByPersonId(person.Id);

            response.PersonalRelationships.Should().BeEmpty();
        }

        [Test]
        public void WhenThereIsARelationshipWithDetailsReturnsTheDetails()
        {
            var (person, _, _, _, personalRelationshipDetail) = PersonalRelationshipsHelper.SavePersonWithPersonalRelationshipToDatabase(DatabaseContext, details: "Emergency contact");

            var response = _databaseGateway.GetPersonWithPersonalRelationshipsByPersonId(person.Id);

            response.PersonalRelationships.FirstOrDefault().Details.Should().BeEquivalentTo(personalRelationshipDetail);
        }

        [Test]
        public void WhenThereAreMultipleRelationshipsReturnsAllRelevantPersonalRelationships()
        {
            var (person, _, _, personalRelationshipType, _) = PersonalRelationshipsHelper.SavePersonWithPersonalRelationshipToDatabase(DatabaseContext);
            var anotherPerson = TestHelpers.CreatePerson();
            var anotherRelationship = PersonalRelationshipsHelper.CreatePersonalRelationship(person.Id, anotherPerson.Id, personalRelationshipType.Id, id: 1);
            var andAnotherPerson = TestHelpers.CreatePerson();
            var andAnotherRelationship = PersonalRelationshipsHelper.CreatePersonalRelationship(person.Id, andAnotherPerson.Id, personalRelationshipType.Id, hasEnded: true, id: 2);
            DatabaseContext.Persons.Add(anotherPerson);
            DatabaseContext.Persons.Add(andAnotherPerson);
            DatabaseContext.PersonalRelationships.Add(anotherRelationship);
            DatabaseContext.PersonalRelationships.Add(andAnotherRelationship);
            DatabaseContext.SaveChanges();

            var response = _databaseGateway.GetPersonWithPersonalRelationshipsByPersonId(person.Id);

            response.PersonalRelationships.Should().HaveCount(2);
        }
    }
}
