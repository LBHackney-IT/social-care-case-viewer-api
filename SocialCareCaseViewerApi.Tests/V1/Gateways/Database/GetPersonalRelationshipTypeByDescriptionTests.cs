using FluentAssertions;
using Moq;
using NUnit.Framework;
using SocialCareCaseViewerApi.V1.Gateways;
using SocialCareCaseViewerApi.V1.Helpers;
using Microsoft.EntityFrameworkCore;

namespace SocialCareCaseViewerApi.Tests.V1.Gateways.Database
{
    [TestFixture]
    public class GetPersonalRelationshipTypeByDescriptionTests : DatabaseTests
    {
        private DatabaseGateway _databaseGateway;
        private Mock<IProcessDataGateway> _mockProcessDataGateway;
        private Mock<ISystemTime> _mockSystemTime;

        [SetUp]
        public void Setup()
        {
            _mockProcessDataGateway = new Mock<IProcessDataGateway>();
            _mockSystemTime = new Mock<ISystemTime>();


            _databaseGateway = new DatabaseGateway(DatabaseContext, _mockProcessDataGateway.Object,
                _mockSystemTime.Object);
            DatabaseContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        }

        [Test]
        public void WhenNoMatchingDescriptionReturnsNull()
        {
            var response = _databaseGateway.GetPersonalRelationshipTypeByDescription("foobar");

            response.Should().BeNull();
        }

        [Test]
        [TestCase("parent")]
        [TestCase("child")]
        [TestCase("friend")]
        public void WhenMatchingDescriptionReturnsPersonalRelationshipType(string description)
        {
            var response = _databaseGateway.GetPersonalRelationshipTypeByDescription(description);

            response.Id.Should().NotBe(null);
            response.Description.Should().Be(description);
            response.InverseTypeId.Should().NotBe(null);
        }

        [Test]
        [TestCase("parentofunbornchild", "parentOfUnbornChild")]
        [TestCase("parentOfUnbornChild", "parentOfUnbornChild")]
        [TestCase("auntuncle", "auntUncle")]
        [TestCase("auntUncle", "auntUncle")]
        public void WhenDescriptionIsADifferentCaseReturnsPersonalRelationshipType(string description, string expectedDescription)
        {
            var response = _databaseGateway.GetPersonalRelationshipTypeByDescription(description);

            response.Id.Should().NotBe(null);
            response.Description.Should().Be(expectedDescription);
            response.InverseTypeId.Should().NotBe(null);
        }
    }
}
