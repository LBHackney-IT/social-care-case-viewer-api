using System.Linq;
using FluentAssertions;
using NUnit.Framework;
using SocialCareCaseViewerApi.V1.Gateways;
using Microsoft.EntityFrameworkCore;
using SocialCareCaseViewerApi.Tests.V1.Helpers;
using SocialCareCaseViewerApi.V1.Factories;
using System;
using SocialCareCaseViewerApi.V1.Exceptions;

namespace SocialCareCaseViewerApi.Tests.V1.Gateways.CaseStatusGatewayTests
{
    [TestFixture]
    public class UpdateCaseStatusTests : DatabaseTests
    {
        private CaseStatusGateway _caseStatusGateway;

        [SetUp]
        public void Setup()
        {
            _caseStatusGateway = new CaseStatusGateway(DatabaseContext);
            DatabaseContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        }

        [Test]
        public void WhenACaseStatusIsFoundItUpdatesTheEndDate()
        {
            var request = TestHelpers.CreateUpdateCaseStatusRequest();
            var (caseStatus, _) = CaseStatusHelper.SavePersonWithCaseStatusToDatabase(DatabaseContext);
            caseStatus.EndDate = null;
            DatabaseContext.SaveChanges();

            var response = _caseStatusGateway.UpdateCaseStatus(caseStatus.Id, request);

            response.EndDate.Should().Be(request.EndDate);
        }

        [Test]
        public void WhenACaseStatusIsNotFoundItThrowsAnException()
        {
            var request = TestHelpers.CreateUpdateCaseStatusRequest();
            const long nonExistentCaseStatusId = 1L;

            Action act = () => _caseStatusGateway.UpdateCaseStatus(nonExistentCaseStatusId, request);

            act.Should().Throw<CaseStatusDoesNotExistException>()
            .WithMessage($"Case status with {nonExistentCaseStatusId} not found");
        }


    }
}
