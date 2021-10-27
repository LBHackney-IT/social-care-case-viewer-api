using System;
using System.Linq;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using SocialCareCaseViewerApi.Tests.V1.Helpers;
using SocialCareCaseViewerApi.V1.Factories;
using SocialCareCaseViewerApi.V1.Gateways;
using SocialCareCaseViewerApi.V1.Helpers;

namespace SocialCareCaseViewerApi.Tests.V1.Gateways.CaseStatusGatewayTests
{
    [TestFixture]
    public class GetClosedCaseStatusesByPersonIdAndDate : DatabaseTests
    {
        private CaseStatusGateway _caseStatusGateway;

        private Mock<ISystemTime> _mockSystemTime;

        [SetUp]
        public void Setup()
        {
            _mockSystemTime = new Mock<ISystemTime>();
            _caseStatusGateway =
                new CaseStatusGateway(DatabaseContext, _mockSystemTime.Object);
            DatabaseContext.ChangeTracker.QueryTrackingBehavior =
                QueryTrackingBehavior.NoTracking;
        }

        [Test]
        public void WhenNoOverlappingDatesNoCaseStatusesReturned()
        {
            var (_, person) = CaseStatusHelper.SavePersonWithPastCaseStatusToDatabase(DatabaseContext);

            var response = _caseStatusGateway.GetClosedCaseStatusesByPersonIdAndDate(person.Id, DateTime.Today);

            response.Should().BeEmpty();
        }

        [Test]
        public void WhenOverlappingDatesReturnsCaseStatusWithOverlappingDates()
        {
            var (caseStatus, person) = CaseStatusHelper.SavePersonWithPastCaseStatusToDatabase(DatabaseContext);

            var response = _caseStatusGateway.GetClosedCaseStatusesByPersonIdAndDate(caseStatus.PersonId, DateTime.Today.AddDays(-2));

            response.Count.Should().Be(1);
        }
    }
}
