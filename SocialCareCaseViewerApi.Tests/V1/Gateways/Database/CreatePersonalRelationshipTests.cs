using System;
using System.Linq;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SocialCareCaseViewerApi.V1.Helpers;
using SocialCareCaseViewerApi.V1.Gateways;
using SocialCareCaseViewerApi.Tests.V1.Helpers;
using Microsoft.EntityFrameworkCore;

namespace SocialCareCaseViewerApi.Tests.V1.Gateways.Database
{
    [TestFixture]
    public class CreatePersonalRelationshipTests : DatabaseTests
    {
        private DatabaseGateway _databaseGateway;
        private readonly Mock<IProcessDataGateway> _mockProcessDataGateway = new Mock<IProcessDataGateway>();
        private readonly Mock<ISystemTime> _mockSystemTime = new Mock<ISystemTime>();

        [SetUp]
        public void Setup()
        {
            _databaseGateway = new DatabaseGateway(DatabaseContext, _mockProcessDataGateway.Object, _mockSystemTime.Object);
            DatabaseContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        }

        [Test]
        public void CreatesAPersonalRelationship()
        {
            var (person, otherPerson) = PersonalRelationshipsHelper.SavePersonAndOtherPersonToDatabase(DatabaseContext);
            var type = DatabaseContext.PersonalRelationshipTypes.FirstOrDefault(prt => prt.Description == "parent");
            var request = PersonalRelationshipsHelper.CreatePersonalRelationshipRequest(
                person.Id, otherPerson.Id, type.Id, type.Description
            );

            _databaseGateway.CreatePersonalRelationship(request);

            var personalRelationship = DatabaseContext.PersonalRelationships.FirstOrDefault();
            personalRelationship.PersonId.Should().Be(request.PersonId);
            personalRelationship.OtherPersonId.Should().Be(request.OtherPersonId);
            personalRelationship.TypeId.Should().Be(request.TypeId);
            personalRelationship.IsMainCarer.Should().Be(request.IsMainCarer);
            personalRelationship.IsInformalCarer.Should().Be(request.IsInformalCarer);
        }

        [Test]
        public void SetsStartDateToNow()
        {
            var fakeTime = new DateTime(2000, 1, 1, 15, 30, 0);
            _mockSystemTime.Setup(time => time.Now).Returns(fakeTime);

            var (person, otherPerson) = PersonalRelationshipsHelper.SavePersonAndOtherPersonToDatabase(DatabaseContext);
            var type = DatabaseContext.PersonalRelationshipTypes.FirstOrDefault(prt => prt.Description == "parent");
            var request = PersonalRelationshipsHelper.CreatePersonalRelationshipRequest(
                person.Id, otherPerson.Id, type.Id, type.Description
            );

            _databaseGateway.CreatePersonalRelationship(request);

            var personalRelationship = DatabaseContext.PersonalRelationships.FirstOrDefault();
            personalRelationship.StartDate.Should().Be(fakeTime);
        }

        [Test]
        public void CreatesAPersonalRelationshipDetails()
        {
            var (person, otherPerson) = PersonalRelationshipsHelper.SavePersonAndOtherPersonToDatabase(DatabaseContext);
            var type = DatabaseContext.PersonalRelationshipTypes.FirstOrDefault(prt => prt.Description == "parent");
            var request = PersonalRelationshipsHelper.CreatePersonalRelationshipRequest(
                person.Id, otherPerson.Id, type.Id, type.Description
            );

            var response = _databaseGateway.CreatePersonalRelationship(request);

            var details = DatabaseContext.PersonalRelationshipDetails.FirstOrDefault();
            details.PersonalRelationshipId.Should().Be(response.Id);
            details.Details.Should().Be(request.Details);
        }
    }
}
