using System.Linq;
using FluentAssertions;
using MongoDB.Driver;
using Moq;
using NUnit.Framework;
using SocialCareCaseViewerApi.Tests.V1.Helpers;
using SocialCareCaseViewerApi.V1.Gateways;
using Microsoft.EntityFrameworkCore;
using SocialCareCaseViewerApi.V1.Helpers;

namespace SocialCareCaseViewerApi.Tests.V1.Gateways.Database
{
    [TestFixture]
    public class GetPersonWithPersonalRelationshipsByPersonIdTests : DatabaseTests
    {
        private DatabaseGateway _databaseGateway;
        private Mock<IProcessDataGateway> _mockProcessDataGateway = new Mock<IProcessDataGateway>();
        private Mock<ISystemTime> _mockSystemTime;

        [SetUp]
        public void Setup()
        {
            _mockSystemTime = new Mock<ISystemTime>();
            _databaseGateway = new DatabaseGateway(DatabaseContext, _mockProcessDataGateway.Object, _mockSystemTime.Object);
            DatabaseContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        }

        [Test]
        public void WhenNoMatchingPersonIdReturnsNull()
        {
            var response = _databaseGateway.GetPersonWithPersonalRelationshipsByPersonId(123456789);

            response.Should().BeNull();
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

            response.PersonalRelationships.FirstOrDefault().Id.Should().Be(personalRelationship.Id);
            response.PersonalRelationships.FirstOrDefault().PersonId.Should().Be(personalRelationship.PersonId);
            response.PersonalRelationships.FirstOrDefault().OtherPersonId.Should().Be(personalRelationship.OtherPersonId);
            response.PersonalRelationships.FirstOrDefault().TypeId.Should().Be(personalRelationship.TypeId);
        }

        [Test]
        public void WhenThereIsARelationshipReturnsTheDetailsOfTheOtherPerson()
        {
            var (person, otherPerson, _, _, _) = PersonalRelationshipsHelper.SavePersonWithPersonalRelationshipToDatabase(DatabaseContext);

            var response = _databaseGateway.GetPersonWithPersonalRelationshipsByPersonId(person.Id);
            var otherPersonInResponse = response.PersonalRelationships.FirstOrDefault().OtherPerson;

            otherPersonInResponse.Id.Should().Be(otherPerson.Id);
            otherPersonInResponse.FirstName.Should().Be(otherPerson.FirstName);
            otherPersonInResponse.LastName.Should().Be(otherPerson.LastName);
            otherPersonInResponse.Gender.Should().Be(otherPerson.Gender);
        }

        [Test]
        public void WhenThereIsARelationshipReturnsTheDescriptionOfThePersonalRelationshipType()
        {
            var (person, _, _, personalRelationshipType, _) = PersonalRelationshipsHelper.SavePersonWithPersonalRelationshipToDatabase(DatabaseContext, relationshipType: "stepParent");

            var response = _databaseGateway.GetPersonWithPersonalRelationshipsByPersonId(person.Id);
            var personalRelationshipTypeInResponse = response.PersonalRelationships.FirstOrDefault().Type;

            personalRelationshipTypeInResponse.Description.Should().Equals(personalRelationshipType.Description);
        }

        [Test]
        public void WhenIncludeEndedRelationshipsIsFalseReturnsEmptyListForPersonalRelationships()
        {
            var (person, _, _, _, _) = PersonalRelationshipsHelper.SavePersonWithPersonalRelationshipToDatabase(DatabaseContext, hasEnded: true);

            var response = _databaseGateway.GetPersonWithPersonalRelationshipsByPersonId(person.Id, includeEndedRelationships: false);

            response.PersonalRelationships.Should().BeEmpty();
        }

        [Test]
        public void WhenIncludeEndedRelationshipsIsTrueReturnsPersonalRelationships()
        {
            var (person, _, _, _, _) = PersonalRelationshipsHelper.SavePersonWithPersonalRelationshipToDatabase(DatabaseContext, hasEnded: true);

            var response = _databaseGateway.GetPersonWithPersonalRelationshipsByPersonId(person.Id, includeEndedRelationships: true);

            response.PersonalRelationships.Should().NotBeEmpty();
        }

        [Test]
        public void WhenThereIsARelationshipWithDetailsReturnsTheDetails()
        {
            var (person, _, _, _, personalRelationshipDetail) = PersonalRelationshipsHelper.SavePersonWithPersonalRelationshipToDatabase(DatabaseContext, details: "Emergency contact");

            var response = _databaseGateway.GetPersonWithPersonalRelationshipsByPersonId(person.Id);

            response.PersonalRelationships.FirstOrDefault().Details.Details.Should().BeEquivalentTo(personalRelationshipDetail.Details);
        }

        [Test]
        public void WhenThereAreMultipleRelationshipsReturnsAllRelevantPersonalRelationships()
        {
            var (person, _, _, personalRelationshipType, _) = PersonalRelationshipsHelper.SavePersonWithPersonalRelationshipToDatabase(DatabaseContext);
            var anotherPerson = TestHelpers.CreatePerson();
            var anotherRelationship = PersonalRelationshipsHelper.CreatePersonalRelationship(person, anotherPerson, personalRelationshipType, id: 1);
            var andAnotherPerson = TestHelpers.CreatePerson();
            var andAnotherRelationship = PersonalRelationshipsHelper.CreatePersonalRelationship(person, andAnotherPerson, personalRelationshipType, hasEnded: true, id: 2);
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
