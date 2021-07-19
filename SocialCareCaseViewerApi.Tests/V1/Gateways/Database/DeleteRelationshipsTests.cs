using System;
using System.Linq;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SocialCareCaseViewerApi.V1.Helpers;
using SocialCareCaseViewerApi.V1.Gateways;
using SocialCareCaseViewerApi.Tests.V1.Helpers;
using SocialCareCaseViewerApi.V1.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace SocialCareCaseViewerApi.Tests.V1.Gateways.Database
{
    [TestFixture]
    public class DeleteRelationshipTests : DatabaseTests
    {
        private DatabaseGateway _databaseGateway;
        private readonly Mock<IProcessDataGateway> _mockProcessDataGateway = new Mock<IProcessDataGateway>();
        private readonly Mock<ISystemTime> _mockSystemTime = new Mock<ISystemTime>();
        private PersonalRelationship _relationship;
        private PersonalRelationship _oppositeRelationship;

        [SetUp]
        public void Setup()
        {
            _databaseGateway = new DatabaseGateway(DatabaseContext, _mockProcessDataGateway.Object, _mockSystemTime.Object);

            (_, _, _relationship, _, _, _oppositeRelationship) = PersonalRelationshipsHelper.SavePersonWithPersonalRelationshipAndOppositeToDatabase(DatabaseContext);
        }

        [Test]
        public void DeletesAPersonalRelationship()
        {
            _databaseGateway.DeleteRelationships(_relationship);

            var personalRelationship = DatabaseContext.PersonalRelationships.FirstOrDefault(pr => pr.Id == _relationship.Id);

            personalRelationship.Should().BeNull();
        }

        [Test]
        public void DeletesAPersonalRelationshipInverse()
        {
            _databaseGateway.DeleteRelationships(_relationship);

            var personalRelationship = DatabaseContext.PersonalRelationships.FirstOrDefault(pr => pr.Id == _oppositeRelationship.Id);

            personalRelationship.Should().BeNull();
        }

        [Test]
        public void DeletesAPersonalRelationshipDetails()
        {
            _databaseGateway.DeleteRelationships(_relationship);

            var personalRelationshipDetails = DatabaseContext.PersonalRelationshipDetails.FirstOrDefault(pr => pr.Id == _relationship.Id);

            personalRelationshipDetails.Should().BeNull();
        }

        [Test]
        public void DeletesAPersonalRelationshipInverseDetails()
        {
            _databaseGateway.DeleteRelationships(_relationship);

            var reverseRelationshipDetails = DatabaseContext.PersonalRelationshipDetails.FirstOrDefault(pr => pr.Id == _oppositeRelationship.Id);

            reverseRelationshipDetails.Should().BeNull();
        }
    }
}
