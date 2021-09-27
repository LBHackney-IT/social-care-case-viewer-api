using System.Linq;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SocialCareCaseViewerApi.V1.Helpers;
using SocialCareCaseViewerApi.V1.Gateways;
using SocialCareCaseViewerApi.Tests.V1.Helpers;
using SocialCareCaseViewerApi.V1.Gateways.Interfaces;
using SocialCareCaseViewerApi.V1.Infrastructure;

namespace SocialCareCaseViewerApi.Tests.V1.Gateways.Database
{
    [TestFixture]
    public class DeleteRelationshipTests : DatabaseTests
    {
        private DatabaseGateway _databaseGateway;
        private Mock<IProcessDataGateway> _mockProcessDataGateway;
        private Mock<IWorkerGateway> _mockWorkerGateway;
        private Mock<ISystemTime> _mockSystemTime;
        private PersonalRelationship _relationship;
        private PersonalRelationship _oppositeRelationship;

        [SetUp]
        public void Setup()
        {
            _mockProcessDataGateway = new Mock<IProcessDataGateway>();
            _mockSystemTime = new Mock<ISystemTime>();
            _mockWorkerGateway = new Mock<IWorkerGateway>();

            _databaseGateway = new DatabaseGateway(DatabaseContext, _mockProcessDataGateway.Object,
                _mockSystemTime.Object);

            (_, _, _relationship, _, _, _oppositeRelationship) = PersonalRelationshipsHelper.SavePersonWithPersonalRelationshipAndOppositeToDatabase(DatabaseContext);
        }

        [Test]
        public void DeletesAPersonalRelationship()
        {
            _databaseGateway.DeleteRelationship(_relationship.Id);

            var personalRelationship = DatabaseContext.PersonalRelationships.FirstOrDefault(pr => pr.Id == _relationship.Id);

            personalRelationship.Should().BeNull();
        }

        [Test]
        public void DeletesAPersonalRelationshipInverse()
        {
            _databaseGateway.DeleteRelationship(_relationship.Id);

            var personalRelationship = DatabaseContext.PersonalRelationships.FirstOrDefault(pr => pr.Id == _oppositeRelationship.Id);

            personalRelationship.Should().BeNull();
        }

        [Test]
        public void DeletesAPersonalRelationshipDetails()
        {
            _databaseGateway.DeleteRelationship(_relationship.Id);

            var personalRelationshipDetails = DatabaseContext.PersonalRelationshipDetails.FirstOrDefault(pr => pr.Id == _relationship.Id);

            personalRelationshipDetails.Should().BeNull();
        }

        [Test]
        public void DeletesAPersonalRelationshipInverseDetails()
        {
            _databaseGateway.DeleteRelationship(_relationship.Id);

            var reverseRelationshipDetails = DatabaseContext.PersonalRelationshipDetails.FirstOrDefault(pr => pr.Id == _oppositeRelationship.Id);

            reverseRelationshipDetails.Should().BeNull();
        }
    }
}
