using FluentAssertions;
using Moq;
using NUnit.Framework;
using SocialCareCaseViewerApi.Tests.V1.Helpers;
using SocialCareCaseViewerApi.V1.Exceptions;
using SocialCareCaseViewerApi.V1.Gateways;
using SocialCareCaseViewerApi.V1.Helpers;
using SocialCareCaseViewerApi.V1.Infrastructure;
using System;
using System.Linq;

namespace SocialCareCaseViewerApi.Tests.V1.Gateways.CaseStatusGatewayTests
{
    [TestFixture]
    public class UpdateCaseStatusTests : DatabaseTests
    {
        private CaseStatusGateway _caseStatusGateway;
        private Mock<ISystemTime> _mockSystemTime;

        [SetUp]
        public void Setup()
        {
            _mockSystemTime = new Mock<ISystemTime>();
            _caseStatusGateway = new CaseStatusGateway(DatabaseContext, _mockSystemTime.Object);
        }

        [Test]
        public void WhenACaseStatusIsNotFoundItThrowsAnException()
        {
            var request = TestHelpers.CreateUpdateCaseStatusRequest();
            request.CaseStatusId = 1L;

            Action act = () => _caseStatusGateway.UpdateCaseStatus(request);

            act.Should().Throw<CaseStatusDoesNotExistException>()
            .WithMessage($"Case status with {request.CaseStatusId} not found");
        }

        [Test]
        public void WhenCaseStatusHasEndDateAlreadyAndTheProvidedEndDateIsInThePastItThrowsAnException()
        {
            var request = TestHelpers.CreateUpdateCaseStatusRequest();
            var (caseStatus, _, _) = CaseStatusHelper.SavePersonWithCaseStatusToDatabase(DatabaseContext);
            DatabaseContext.SaveChanges();
            request.CaseStatusId = caseStatus.Id;
            request.EndDate = DateTime.Today.AddDays(-2);

            Action act = () => _caseStatusGateway.UpdateCaseStatus(request);

            act.Should().Throw<InvalidEndDateException>()
                .WithMessage($"Invalid end date.");
        }

        [Test]
        public void WhenCaseStatusHasEndDateAlreadyAndTheProvidedEndDateIsInTheFutureItUpdatesTheEndDate()
        {
            var request = TestHelpers.CreateUpdateCaseStatusRequest();
            var (caseStatus, _, _) = CaseStatusHelper.SavePersonWithCaseStatusToDatabase(DatabaseContext);
            DatabaseContext.SaveChanges();

            request.CaseStatusId = caseStatus.Id;
            request.EndDate = DateTime.Today.AddDays(1);

            _caseStatusGateway.UpdateCaseStatus(request);

            var updatedCaseStatus = DatabaseContext.CaseStatuses.FirstOrDefault(x => x.Id == caseStatus.Id);
            updatedCaseStatus.EndDate.Should().Be(request.EndDate);
            updatedCaseStatus.LastModifiedBy.Should().Be(request.EditedBy);
        }

        [Test]
        public void WhenCaseStatusHasEndDateAlreadyAndTheProvidedEndDateIsTodayItUpdatesTheEnd()
        {
            var request = TestHelpers.CreateUpdateCaseStatusRequest();
            var (caseStatus, _, _) = CaseStatusHelper.SavePersonWithCaseStatusToDatabase(DatabaseContext);
            DatabaseContext.SaveChanges();

            request.CaseStatusId = caseStatus.Id;
            request.EndDate = DateTime.Today;

            _caseStatusGateway.UpdateCaseStatus(request);

            var updatedCaseStatus = DatabaseContext.CaseStatuses.FirstOrDefault(x => x.Id == caseStatus.Id);
            updatedCaseStatus.EndDate.Should().Be(request.EndDate);
            updatedCaseStatus.LastModifiedBy.Should().Be(request.EditedBy);
        }
    }
}
