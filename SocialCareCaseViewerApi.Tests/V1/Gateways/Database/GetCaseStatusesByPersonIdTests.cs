using FluentAssertions;
using Moq;
using System;
using System.Linq;
using NUnit.Framework;
using SocialCareCaseViewerApi.V1.Gateways;
using SocialCareCaseViewerApi.V1.Helpers;
using Microsoft.EntityFrameworkCore;
using SocialCareCaseViewerApi.Tests.V1.Helpers;

namespace SocialCareCaseViewerApi.Tests.V1.Gateways.Database
{
    [TestFixture]
    public class GetCaseStatusesByPersonIdTests : DatabaseTests
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
        public void WhenNoMatchingIDReturnsEmptyList()
        {
            var person = DatabaseGatewayHelper.CreatePersonDatabaseEntity();
            DatabaseContext.Persons.Add(person);
            DatabaseContext.SaveChanges();


            var response = _databaseGateway.GetCaseStatusesByPersonId(person.Id);
            response.Should().BeEmpty();
        }

        [Test]
        public void WhenMatchingIDReturnsCaseStatuses()
        {
            var (_, person) = CaseStatusHelper.SavePersonWithCaseStatusToDatabase(DatabaseContext);

            var response = _databaseGateway.GetCaseStatusesByPersonId(person.Id);

            response.Should().NotBeEmpty();
        }

        [Test]
        public void WhenMatchingIDAndPastCaseStatusReturnEmptyList()
        {
            var (_, person) = CaseStatusHelper.SavePersonWithPastCaseStatusToDatabase(DatabaseContext);

            var response = _databaseGateway.GetCaseStatusesByPersonId(person.Id);

            response.Should().BeEmpty();
        }

        [Test]
        public void WhenMatchingIDReturnsActiveCaseStatusesWhenMultiple()
        {
            var (_, person) = CaseStatusHelper.SavePersonWithMultipleCaseStatusToDatabase(DatabaseContext);

            var response = _databaseGateway.GetCaseStatusesByPersonId(person.Id);

            response.Count().Should().Be(1);
        }
    }
}
