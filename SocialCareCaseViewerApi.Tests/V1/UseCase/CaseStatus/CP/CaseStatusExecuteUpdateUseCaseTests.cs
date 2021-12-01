using FluentAssertions;
using Moq;
using NUnit.Framework;
using SocialCareCaseViewerApi.Tests.V1.Helpers;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Exceptions;
using SocialCareCaseViewerApi.V1.Factories;
using SocialCareCaseViewerApi.V1.Gateways.Interfaces;
using SocialCareCaseViewerApi.V1.Infrastructure;
using SocialCareCaseViewerApi.V1.UseCase;
using System;
using System.Collections.Generic;
using DomainCaseStatus = SocialCareCaseViewerApi.V1.Domain.CaseStatus;

#nullable enable
namespace SocialCareCaseViewerApi.Tests.V1.UseCase.CP.CaseStatus
{
    [TestFixture]
    public class CaseStatusExecuteUpdateUseCaseTests
    {
        private Mock<ICaseStatusGateway> _mockCaseStatusGateway = null!;
        private Mock<IDatabaseGateway> _mockDatabaseGateway = null!;
        private CaseStatusesUseCase _caseStatusesUseCase = null!;

        private Person _resident = null!;
        private Worker _worker = null!;
        private SocialCareCaseViewerApi.V1.Infrastructure.CaseStatus _caseStatus = null!;
        private UpdateCaseStatusRequest _updateCaseStatusRequest = null!;
        private SocialCareCaseViewerApi.V1.Infrastructure.CaseStatus _updatedCaseStatus = null!;

        private static readonly object[] _invaliCPAnswers = {
            new List<CaseStatusValue>() { },
            new List<CaseStatusValue>() { new CaseStatusValue() { Option = "", Value = "" } },
            new List<CaseStatusValue>() { new CaseStatusValue() { Option = "", Value = "value" } } ,
            new List<CaseStatusValue>() { new CaseStatusValue() { Option = "option", Value = "" } },
            new List<CaseStatusValue>() { new CaseStatusValue() { Option = "option", Value = " " } }
        };


        private static readonly object[] _invalidCPEpisodeEndingAnswer = {
            new List<CaseStatusValue>() { },
            new List<CaseStatusValue>() { new CaseStatusValue() { Option = "", Value = "" } },
            new List<CaseStatusValue>() { new CaseStatusValue() { Option = "", Value = "value" }},
            new List<CaseStatusValue>() { new CaseStatusValue() { Option = "option", Value = "" }},
            new List<CaseStatusValue>() { new CaseStatusValue() { Option = "option", Value = " " }}
        };

        [SetUp]
        public void SetUp()
        {
            _mockCaseStatusGateway = new Mock<ICaseStatusGateway>();
            _mockDatabaseGateway = new Mock<IDatabaseGateway>();
            _caseStatusesUseCase = new CaseStatusesUseCase(_mockCaseStatusGateway.Object, _mockDatabaseGateway.Object);

            _resident = TestHelpers.CreatePerson(ageContext: "C");
            _worker = TestHelpers.CreateWorker();
            _caseStatus = TestHelpers.CreateCaseStatus(resident: _resident);
            _updateCaseStatusRequest = TestHelpers.CreateUpdateCaseStatusRequest(caseStatusId: _caseStatus.Id, email: _worker.Email);
            _updatedCaseStatus = TestHelpers.CreateCaseStatus(_resident.Id, _caseStatus.Notes,
                _caseStatus.StartDate, _updateCaseStatusRequest.EndDate, resident: _resident);

            _mockCaseStatusGateway
                .Setup(x => x.GetCasesStatusByCaseStatusId(_caseStatus.Id))
                .Returns(_caseStatus.ToDomain());

            _mockCaseStatusGateway
                .Setup(x => x.UpdateCaseStatus(_updateCaseStatusRequest))
                .Returns(_updatedCaseStatus.ToDomain());

            _mockDatabaseGateway.Setup(x => x.GetPersonByMosaicId(_resident.Id)).Returns(_resident);
            _mockDatabaseGateway.Setup(x => x.GetWorkerByEmail(_worker.Email)).Returns(_worker);
        }

        [Test]
        public void WhenTypeIsCPAndProvidedEndIsBeforeTheCurrentlyActiveAnswersStartDateItThrowsInvalidEndDateException()
        {
            _caseStatus = TestHelpers.CreateCaseStatus(resident: _resident, startDate: DateTime.Now.AddDays(1), type: "CP");
            _caseStatus.Answers = new List<CaseStatusAnswer>();
            _caseStatus.Answers.AddRange(TestHelpers.CreateCaseStatusAnswers(min: 1, max: 1, startDate: new DateTime(2021, 11, 3)));

            _updateCaseStatusRequest = TestHelpers.CreateUpdateCaseStatusRequest(caseStatusId: _caseStatus.Id, email: _worker.Email, endDate: new DateTime(2021, 11, 1), min: 1, max: 1);

            _mockCaseStatusGateway
                .Setup(x => x.GetCasesStatusByCaseStatusId(_caseStatus.Id))
                .Returns(_caseStatus.ToDomain());

            _mockCaseStatusGateway.Setup(x => x.UpdateCaseStatus(It.IsAny<UpdateCaseStatusRequest>())).Returns(new DomainCaseStatus());

            Action act = () => _caseStatusesUseCase.ExecuteUpdate(_updateCaseStatusRequest);

            act.Should().Throw<InvalidEndDateException>()
                .WithMessage("requested end date is before the start date of the currently active answer");
        }

        [Test]
        public void WhenTypeIsCPandTheProvidedEndDateIsValidAndThereAreScheduledAnswersItCallsTheGateway()
        {
            var activeAnswers = TestHelpers.CreateCaseStatusAnswers(min: 1, max: 1, startDate: new DateTime(2000, 01, 11), endDate: new DateTime(2040, 02, 01));
            var scheduledAnswers = TestHelpers.CreateCaseStatusAnswers(min: 1, max: 1, startDate: new DateTime(2040, 02, 01));

            _caseStatus = TestHelpers.CreateCaseStatus(resident: _resident, startDate: new DateTime(2000, 01, 11), type: "CP");
            _caseStatus.Answers = new List<CaseStatusAnswer>();
            _caseStatus.Answers.AddRange(activeAnswers);
            _caseStatus.Answers.AddRange(scheduledAnswers);

            _updateCaseStatusRequest = TestHelpers.CreateUpdateCaseStatusRequest(caseStatusId: _caseStatus.Id, email: _worker.Email, endDate: new DateTime(2000, 01, 11), min: 1, max: 1);

            _mockCaseStatusGateway.Setup(x => x.GetCasesStatusByCaseStatusId(_caseStatus.Id)).Returns(_caseStatus.ToDomain());
            _mockCaseStatusGateway.Setup(x => x.UpdateCaseStatus(It.IsAny<UpdateCaseStatusRequest>())).Returns(new DomainCaseStatus());

            _caseStatusesUseCase.ExecuteUpdate(_updateCaseStatusRequest);

            _mockCaseStatusGateway.Verify(x => x.UpdateCaseStatus(_updateCaseStatusRequest));
        }

        [Test]
        public void WhenTypeIsCPAndRequestedEndDateIsOnActiveCaseStatusAnswerStartDateItCallsTheGateway()
        {
            _caseStatus = TestHelpers.CreateCaseStatus(resident: _resident, startDate: new DateTime(2021, 11, 3), type: "CP");
            _updateCaseStatusRequest = TestHelpers.CreateUpdateCaseStatusRequest(caseStatusId: _caseStatus.Id, email: _worker.Email, endDate: new DateTime(2021, 11, 3), min: 1, max: 1);

            _mockCaseStatusGateway
                .Setup(x => x.GetCasesStatusByCaseStatusId(_caseStatus.Id))
                .Returns(_caseStatus.ToDomain());
            _mockCaseStatusGateway.Setup(x => x.UpdateCaseStatus(_updateCaseStatusRequest)).Returns(new DomainCaseStatus());

            _caseStatusesUseCase.ExecuteUpdate(_updateCaseStatusRequest);

            _mockCaseStatusGateway.Verify(x => x.UpdateCaseStatus(_updateCaseStatusRequest));
        }

        [Test]
        public void WhenTypeIsCPandEndDateIsNotProvidedAndStartDateIsIntheFutureItThrowsInvalidStartDateException()
        {
            _updateCaseStatusRequest.StartDate = DateTime.Now.AddDays(1);
            _updateCaseStatusRequest.EndDate = null;
            _caseStatus.Type = "CP";

            _mockCaseStatusGateway.Setup(x => x.GetCasesStatusByCaseStatusId(_caseStatus.Id)).Returns(_caseStatus.ToDomain());

            Action act = () => _caseStatusesUseCase.ExecuteUpdate(_updateCaseStatusRequest);

            act.Should().Throw<InvalidStartDateException>()
                .WithMessage("Invalid start date. It cannot be in the future.");
        }

        [Test]
        [TestCaseSource(nameof(_invaliCPAnswers))]
        public void WhenTypeIsCPAndProvidedAnswersAreNotValidItThrowsInvalidUpdateRequestException(List<CaseStatusValue> answers)
        {
            _updateCaseStatusRequest.Answers = answers;

            _updateCaseStatusRequest.EndDate = null;
            _caseStatus.Type = "CP";

            _mockCaseStatusGateway.Setup(x => x.GetCasesStatusByCaseStatusId(_caseStatus.Id)).Returns(_caseStatus.ToDomain());

            Action act = () => _caseStatusesUseCase.ExecuteUpdate(_updateCaseStatusRequest);

            act.Should().Throw<InvalidCaseStatusUpdateRequestException>().WithMessage("Invalid CP answer");
        }

        [Test]
        [TestCaseSource(nameof(_invalidCPEpisodeEndingAnswer))]
        public void WhenTypeIsCPandEndDateIsProvidedAndEpisodeEndingAnswersIsNotValidItThrowsInvalidCaseStatusUpdateRequestException(List<CaseStatusValue> answers)
        {
            _updateCaseStatusRequest.EndDate = DateTime.Today.Date;
            _updateCaseStatusRequest.Answers = answers;
            _caseStatus.Type = "CP";

            _mockCaseStatusGateway.Setup(x => x.GetCasesStatusByCaseStatusId(_caseStatus.Id)).Returns(_caseStatus.ToDomain());

            Action act = () => _caseStatusesUseCase.ExecuteUpdate(_updateCaseStatusRequest);

            act.Should().Throw<InvalidCaseStatusUpdateRequestException>().WithMessage("Invalid CP episode ending answer");
        }
    }
}
