using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using SocialCareCaseViewerApi.V1.Gateways;
using Microsoft.EntityFrameworkCore;
using SocialCareCaseViewerApi.Tests.V1.Helpers;
using SocialCareCaseViewerApi.V1.Factories;

namespace SocialCareCaseViewerApi.Tests.V1.Gateways.Database
{
    [TestFixture]
    public class GetCaseStatusesByPersonIdTests : DatabaseTests
    {
        private CaseStatusGateway _caseStatusGateway;

        [SetUp]
        public void Setup()
        {
            _caseStatusGateway = new CaseStatusGateway(DatabaseContext);
            DatabaseContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        }

        [Test]
        public void WhenNoMatchingIDReturnsEmptyList()
        {
            var person = DatabaseGatewayHelper.CreatePersonDatabaseEntity();
            DatabaseContext.Persons.Add(person);
            DatabaseContext.SaveChanges();

            var response = _caseStatusGateway.GetCaseStatusesByPersonId(person.Id);
            response.Should().BeEmpty();
        }

        [Test]
        public void WhenMatchingIDReturnsCaseStatuses()
        {
            var (caseStatus, person) = CaseStatusHelper.SavePersonWithCaseStatusToDatabase(DatabaseContext);

            var response = _caseStatusGateway.GetCaseStatusesByPersonId(person.Id);

            response.Should().NotBeEmpty();
            response.First().Should().BeEquivalentTo(caseStatus.ToDomain());
        }

        [Test]
        public void WhenMatchingIDAndPastCaseStatusReturnEmptyList()
        {
            var (_, person) = CaseStatusHelper.SavePersonWithPastCaseStatusToDatabase(DatabaseContext);

            var response = _caseStatusGateway.GetCaseStatusesByPersonId(person.Id);

            response.Should().BeEmpty();
        }

        [Test]
        public void WhenMatchingIDReturnsActiveCaseStatusesWhenMultiple()
        {
            var (_, person) = CaseStatusHelper.SavePersonWithMultipleCaseStatusToDatabase(DatabaseContext);

            var response = _caseStatusGateway.GetCaseStatusesByPersonId(person.Id);

            response.Count.Should().Be(2);
        }
    }
}
