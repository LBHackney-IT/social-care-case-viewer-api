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
using System.Collections.Generic;

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
        public void WhenTypeIsNotLACUpdateCaseStatus()
        {
            var request = TestHelpers.CreateUpdateCaseStatusRequest();
            var (caseStatus, _, _) = CaseStatusHelper.SavePersonWithCaseStatusToDatabase(DatabaseContext);

            caseStatus.Type = "CP";
            caseStatus.EndDate = null;

            DatabaseContext.SaveChanges();

            CaseStatusValue updateValue = new CaseStatusValue();
            updateValue.Option = "NewValue";
            updateValue.Value = "N3";

            request.EndDate = null;
            request.CaseStatusId = caseStatus.Id;
            request.Answers.Add(updateValue);
            request.StartDate = DateTime.Now;
            request.Notes = "this is a note";

            var response = _caseStatusGateway.UpdateCaseStatus(request);

            response.StartDate.Should().Be((DateTime) request.StartDate);
            response.Notes.Should().Be(request.Notes);

            response.Answers.Should().ContainEquivalentOf(updateValue);
            response.Answers[0].GroupId.Should().NotBeNull();
        }

        [Test]
        public void WhenTypeIsLACUpdateActiveAnswers()
        {
            var request = TestHelpers.CreateUpdateCaseStatusRequest();
            var (caseStatus, _, caseStatusAnswers) = CaseStatusHelper.SavePersonWithPastActivePendingLACCaseStatusToDatabase(DatabaseContext);

            var legalStatus = new CaseStatusValue();
            legalStatus.Option = "legalStatus";
            legalStatus.Value = "V4";

            var placementType = new CaseStatusValue();
            placementType.Option = "placementType";
            placementType.Value = "H5";

            var answers = new List<CaseStatusValue>();
            answers.Add(legalStatus);
            answers.Add(placementType);

            request.StartDate = DateTime.Now;
            request.Answers = answers;
            request.CaseStatusId = caseStatus.Id;

            var response = _caseStatusGateway.UpdateCaseStatus(request);

            response.StartDate.Should().Be(caseStatus.StartDate);

            response.Answers[caseStatusAnswers.Count - 1].StartDate.Should().Be((DateTime) request.StartDate);
            response.Answers[caseStatusAnswers.Count - 2].StartDate.Should().Be((DateTime) request.StartDate);

            response.Answers.Should().ContainEquivalentOf(legalStatus);
            response.Answers.Should().ContainEquivalentOf(placementType);

            response.Answers.Count.Should().Be(caseStatusAnswers.Count);

            response.Notes.Should().Be(request.Notes);
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
