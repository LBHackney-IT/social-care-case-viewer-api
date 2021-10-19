using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using SocialCareCaseViewerApi.Tests.V1.Helpers;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Exceptions;
using SocialCareCaseViewerApi.V1.Gateways;
using SocialCareCaseViewerApi.V1.Helpers;
using System;

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
            DatabaseContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        }

        [Test]
        public void WhenACaseStatusIsFoundAndTheEndDateIsNotSetItUpdatesTheEndDate()
        {
            var request = TestHelpers.CreateUpdateCaseStatusRequest();
            var (caseStatus, _, _) = CaseStatusHelper.SavePersonWithCaseStatusToDatabase(DatabaseContext);
            caseStatus.EndDate = null;
            DatabaseContext.SaveChanges();
            request.CaseStatusId = caseStatus.Id;
            
            var response = _caseStatusGateway.UpdateCaseStatus(request);

            response.EndDate.Should().Be(request.EndDate);
            response.Answers.Should().ContainEquivalentOf(request.Answers[0]);
        }

        [Test]
        public void WhenACaseStatusIsFoundAndTheEndDateIsNotSetItUpdatesTheEndDateAddingNewAnswers()
        {
            var request = TestHelpers.CreateUpdateCaseStatusRequest();
            var (caseStatus, _, _) = CaseStatusHelper.SavePersonWithCaseStatusToDatabase(DatabaseContext);
            caseStatus.EndDate = null;
            DatabaseContext.SaveChanges();

            CaseStatusValue updateValue = new CaseStatusValue();
            updateValue.Option = "newValueUpdateRequest";
            updateValue.Value = "N3";

            request.CaseStatusId = caseStatus.Id;
            request.Answers.Add(updateValue);

            var response = _caseStatusGateway.UpdateCaseStatus(request);

            response.EndDate.Should().Be(request.EndDate);
            response.Answers.Should().ContainEquivalentOf(updateValue);
            response.Answers[0].GroupId.Should().NotBeNull();
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
        public void WhenCaseStatusHasEndDateAlreadyItThrowsAnException()
        {
            var request = TestHelpers.CreateUpdateCaseStatusRequest();
            var (caseStatus, _, _) = CaseStatusHelper.SavePersonWithCaseStatusToDatabase(DatabaseContext);
            DatabaseContext.SaveChanges();
            request.CaseStatusId = caseStatus.Id;

            Action act = () => _caseStatusGateway.UpdateCaseStatus(request);

            act.Should().Throw<CaseStatusAlreadyClosedException>()
                .WithMessage($"Case status with {request.CaseStatusId} has already been closed.");
        }
    }
}
