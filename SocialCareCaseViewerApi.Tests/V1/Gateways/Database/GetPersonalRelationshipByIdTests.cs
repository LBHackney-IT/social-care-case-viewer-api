using FluentAssertions;
using Moq;
using NUnit.Framework;
using SocialCareCaseViewerApi.V1.Gateways;
using SocialCareCaseViewerApi.V1.Helpers;
using Microsoft.EntityFrameworkCore;
using SocialCareCaseViewerApi.Tests.V1.Helpers;
using SocialCareCaseViewerApi.V1.Gateways.Interfaces;

namespace SocialCareCaseViewerApi.Tests.V1.Gateways.Database
{
    [TestFixture]
    public class GetPersonalRelationshipByIdTests : DatabaseTests
    {
        private DatabaseGateway _databaseGateway;
        private Mock<IProcessDataGateway> _mockProcessDataGateway;
        private Mock<IWorkerGateway> _mockWorkerGateway;
        private Mock<ISystemTime> _mockSystemTime;

        [SetUp]
        public void Setup()
        {
            _mockSystemTime = new Mock<ISystemTime>();
            _mockProcessDataGateway = new Mock<IProcessDataGateway>();
            _mockWorkerGateway = new Mock<IWorkerGateway>();

            _databaseGateway = new DatabaseGateway(DatabaseContext, _mockProcessDataGateway.Object,
                _mockSystemTime.Object);
            DatabaseContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        }

        [Test]
        public void WhenNoMatchingIDReturnsNull()
        {
            var response = _databaseGateway.GetPersonalRelationshipById(123456789);
            response.Should().BeNull();
        }

        [Test]
        public void WhenMatchingIDReturnsRelationship()
        {
            var (person, _, personalRelationship, _, _) = PersonalRelationshipsHelper.SavePersonWithPersonalRelationshipToDatabase(DatabaseContext);

            var response = _databaseGateway.GetPersonalRelationshipById(personalRelationship.Id);

            response.Id.Should().Be(personalRelationship.Id);
            response.PersonId.Should().Be(person.Id);
        }
    }
}
