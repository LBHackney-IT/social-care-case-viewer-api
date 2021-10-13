using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using SocialCareCaseViewerApi.Tests.V1.Helpers;
using SocialCareCaseViewerApi.V1.Factories;
using SocialCareCaseViewerApi.V1.Gateways;
using SocialCareCaseViewerApi.V1.Helpers;
using System;

#nullable enable
namespace SocialCareCaseViewerApi.Tests.V1.Gateways.CaseStatusGatewayTests
{
    [TestFixture]
    public class GetCaseStatusByCaseStatusIdTests : DatabaseTests
    {
        private CaseStatusGateway _caseStatusGateway = null!;
        private Mock<ISystemTime> _mockSystemTime = null!;

        [SetUp]
        public void Setup()
        {
            _mockSystemTime = new Mock<ISystemTime>();
            _caseStatusGateway = new CaseStatusGateway(DatabaseContext, _mockSystemTime.Object);
            DatabaseContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        }

        [Test]
        public void GetCaseStatusByIdReturnsTheAssociatedCaseStatus()
        {
            var (caseStatus, _, _) = CaseStatusHelper.SavePersonWithCaseStatusToDatabase(DatabaseContext);

            var response = _caseStatusGateway.GetCasesStatusByCaseStatusId(caseStatus.Id);

            CaseStatusHelper.TrimMilliseconds(response?.StartDate).Should().Be(CaseStatusHelper.TrimMilliseconds(caseStatus.ToDomain().StartDate));
            CaseStatusHelper.TrimMilliseconds(response?.EndDate).Should().Be(CaseStatusHelper.TrimMilliseconds(caseStatus.ToDomain().EndDate));
            response?.Id.Should().Be(caseStatus.ToDomain().Id);
            response?.Type.Should().Be(caseStatus.ToDomain().Type);

            response?.Answers.Should().BeEquivalentTo(caseStatus.ToDomain().Answers,
                options =>
                {
                    options.Using<DateTime>(ctx => ctx.Subject.Should().BeCloseTo(ctx.Expectation, 1000)).WhenTypeIs<DateTime>();
                    return options;
                }
               );
        }

        [Test]
        public void GetCaseStatusByIdReturnsNullWhenNoAssociatedCaseStatus()
        {
            const long nonExistentCaseStatusId = 1L;
            var response = _caseStatusGateway.GetCasesStatusByCaseStatusId(nonExistentCaseStatusId);

            response.Should().BeNull();
        }
    }
}
