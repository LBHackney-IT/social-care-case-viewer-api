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
namespace SocialCareCaseViewerApi.Tests.V1.UseCase.CaseStatus
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

        private static readonly object[] _invalidLACAnswers = {
            new List<CaseStatusValue>() { },
            new List<CaseStatusValue>() { new CaseStatusValue() { Option = "", Value = "" }, new CaseStatusValue() { Option = "", Value = "" } },
            new List<CaseStatusValue>() { new CaseStatusValue() { Option = "", Value = "value" }, new CaseStatusValue() { Option = "option", Value = "value" } },
            new List<CaseStatusValue>() { new CaseStatusValue() { Option = "option", Value = "" }, new CaseStatusValue() { Option = "option", Value = "value" } },
            new List<CaseStatusValue>() { new CaseStatusValue() { Option = "option", Value = " " }, new CaseStatusValue() { Option = "option", Value = "value" } }
        };

        private static readonly object[] _invalidLACEpisodeEndingAnswer = {
            new List<CaseStatusValue>() { },
            new List<CaseStatusValue>() { new CaseStatusValue() { Option = "", Value = "" } },
            new List<CaseStatusValue>() { new CaseStatusValue() { Option = "", Value = "value" }},
            new List<CaseStatusValue>() { new CaseStatusValue() { Option = "option", Value = "" }},
            new List<CaseStatusValue>() { new CaseStatusValue() { Option = "option", Value = " " }}
        };

        private static readonly object[] _invalidPCAnswers = {
            new List<CaseStatusValue>() { },
            new List<CaseStatusValue>() { new CaseStatusValue() { Option = "", Value = "" } },
            new List<CaseStatusValue>() { new CaseStatusValue() { Option = "", Value = "value" } } ,
            new List<CaseStatusValue>() { new CaseStatusValue() { Option = "option", Value = "" } },
            new List<CaseStatusValue>() { new CaseStatusValue() { Option = "option", Value = " " } }
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
        public void TestUseCaseMakesAppropriateCallsToOurGateways()
        {
            _caseStatusesUseCase.ExecuteUpdate(_updateCaseStatusRequest);

            _mockCaseStatusGateway.Verify(x => x.GetCasesStatusByCaseStatusId(_caseStatus.Id), Times.Once);
            _mockCaseStatusGateway.Verify(x => x.UpdateCaseStatus(_updateCaseStatusRequest), Times.Once);
            _mockDatabaseGateway.Verify(x => x.GetWorkerByEmail(_worker.Email), Times.Once);
        }

        [Test]
        public void TestWhenResidentAgeContextIsNotChildrensInvalidAgeContextExceptionThrown()
        {
            _updateCaseStatusRequest = TestHelpers.CreateUpdateCaseStatusRequest();
            _caseStatus.Person.AgeContext = "A";

            _mockCaseStatusGateway.Setup(x => x.GetCasesStatusByCaseStatusId(It.IsAny<long>())).Returns(_caseStatus.ToDomain);

            Action act = () => _caseStatusesUseCase.ExecuteUpdate(_updateCaseStatusRequest);

            act.Should().Throw<InvalidAgeContextException>()
                .WithMessage($"Person with the id {_resident.Id} belongs to the wrong AgeContext for this operation");
        }

        [Test]
        public void TestWhenRequestEditedByWorkerEmailNotFoundWorkerNotFoundExceptionThrown()
        {
            _mockDatabaseGateway.Setup(x => x.GetWorkerByEmail(_updateCaseStatusRequest.EditedBy));

            Action act = () => _caseStatusesUseCase.ExecuteUpdate(_updateCaseStatusRequest);

            act.Should().Throw<WorkerNotFoundException>()
                .WithMessage($"Worker with email `{_updateCaseStatusRequest.EditedBy}` was not found");
        }

        [Test]
        public void TestWhenUpdatingCaseStatusAndCaseStatusNotFoundCaseStatusDoesNotExistExceptionThrown()
        {
            _mockCaseStatusGateway.Setup(x => x.GetCasesStatusByCaseStatusId(_caseStatus.Id));

            Action act = () => _caseStatusesUseCase.ExecuteUpdate(_updateCaseStatusRequest);

            act.Should().Throw<CaseStatusDoesNotExistException>()
                .WithMessage($"Case status with {_caseStatus.Id} not found");
        }

        [Test]
        [TestCase("CP")]
        [TestCase("CIN")]
        public void WhenTypeIsCPorCINAndRequestedEndDateIsBeforeCaseStatusStartDateInvalidEndDateExceptionThrown(string type)
        {
            _caseStatus = TestHelpers.CreateCaseStatus(resident: _resident, startDate: new DateTime(2021, 11, 3), type: type);
            _updateCaseStatusRequest = TestHelpers.CreateUpdateCaseStatusRequest(caseStatusId: _caseStatus.Id, email: _worker.Email, endDate: new DateTime(2021, 11, 1));

            _mockCaseStatusGateway
                .Setup(x => x.GetCasesStatusByCaseStatusId(_caseStatus.Id))
                .Returns(_caseStatus.ToDomain());

            Action act = () => _caseStatusesUseCase.ExecuteUpdate(_updateCaseStatusRequest);

            act.Should().Throw<InvalidEndDateException>()
                .WithMessage($"requested end date of {_updateCaseStatusRequest.EndDate?.ToString("O")} " +
                             $"is before the start date of {_caseStatus.StartDate:O}");
        }

        [Test]
        [TestCase("CP")]
        [TestCase("CIN")]
        public void WhenTypeIsCINorCPItAndRequestedEndDateIsOnCaseStatusStartDateCallsTheGatewayToUpdateTheRecords(string type)
        {
            _caseStatus = TestHelpers.CreateCaseStatus(resident: _resident, startDate: new DateTime(2021, 11, 3), type: type);
            _updateCaseStatusRequest = TestHelpers.CreateUpdateCaseStatusRequest(caseStatusId: _caseStatus.Id, email: _worker.Email, endDate: new DateTime(2021, 11, 3));

            _mockCaseStatusGateway
                .Setup(x => x.GetCasesStatusByCaseStatusId(_caseStatus.Id))
                .Returns(_caseStatus.ToDomain());
            _mockCaseStatusGateway.Setup(x => x.UpdateCaseStatus(_updateCaseStatusRequest)).Returns(new DomainCaseStatus());

            _caseStatusesUseCase.ExecuteUpdate(_updateCaseStatusRequest);

            _mockCaseStatusGateway.Verify(x => x.UpdateCaseStatus(_updateCaseStatusRequest));
        }

        [Test]
        public void WhenTypeIsLACandThereAreNoScheduledAnswersAndProvidedEndIsBeforeTheCurrentlyActiveAnswersStartDateItThrowsInvalidEndDateException()
        {
            _caseStatus = TestHelpers.CreateCaseStatus(resident: _resident, startDate: DateTime.Now.AddDays(1), type: "LAC");
            _caseStatus.Answers = new List<CaseStatusAnswer>();
            _caseStatus.Answers.AddRange(TestHelpers.CreateCaseStatusAnswers(min: 2, max: 2, startDate: new DateTime(2021, 11, 3)));

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
        public void WhenTypeIsLACandTheProvidedEndDateIsValidAndThereAreScheduledAnswersItCallsTheGateway()
        {
            var activeAnswers = TestHelpers.CreateCaseStatusAnswers(min: 2, max: 2, startDate: new DateTime(2000, 01, 11));
            var scheduledAnswers = TestHelpers.CreateCaseStatusAnswers(min: 2, max: 2, startDate: new DateTime(2040, 02, 01));

            _caseStatus = TestHelpers.CreateCaseStatus(resident: _resident, startDate: new DateTime(200, 01, 11), type: "LAC");
            _caseStatus.Answers = new List<CaseStatusAnswer>();
            _caseStatus.Answers.AddRange(activeAnswers);
            _caseStatus.Answers.AddRange(scheduledAnswers);

            _updateCaseStatusRequest = TestHelpers.CreateUpdateCaseStatusRequest(caseStatusId: _caseStatus.Id, email: _worker.Email, endDate: new DateTime(200, 01, 11), min: 1, max: 1);

            _mockCaseStatusGateway.Setup(x => x.GetCasesStatusByCaseStatusId(_caseStatus.Id)).Returns(_caseStatus.ToDomain());
            _mockCaseStatusGateway.Setup(x => x.UpdateCaseStatus(It.IsAny<UpdateCaseStatusRequest>())).Returns(new DomainCaseStatus());

            _caseStatusesUseCase.ExecuteUpdate(_updateCaseStatusRequest);

            _mockCaseStatusGateway.Verify(x => x.UpdateCaseStatus(_updateCaseStatusRequest));
        }

        [Test]
        public void WhenTypeIsLACAndRequestedEndDateIsOnActiveCaseStatusAnswerStartDateItCallsTheGateway()
        {
            _caseStatus = TestHelpers.CreateCaseStatus(resident: _resident, startDate: new DateTime(2021, 11, 3), type: "LAC");
            _updateCaseStatusRequest = TestHelpers.CreateUpdateCaseStatusRequest(caseStatusId: _caseStatus.Id, email: _worker.Email, endDate: new DateTime(2021, 11, 3), min: 1, max: 1);

            _mockCaseStatusGateway
                .Setup(x => x.GetCasesStatusByCaseStatusId(_caseStatus.Id))
                .Returns(_caseStatus.ToDomain());
            _mockCaseStatusGateway.Setup(x => x.UpdateCaseStatus(_updateCaseStatusRequest)).Returns(new DomainCaseStatus());

            _caseStatusesUseCase.ExecuteUpdate(_updateCaseStatusRequest);

            _mockCaseStatusGateway.Verify(x => x.UpdateCaseStatus(_updateCaseStatusRequest));
        }

        //when end date not provided
        //CIN
        [Test]
        public void WhenTypeIsCINandEndDateIsNotProvidedAndStartDateIsIntheFutureItThrowsInvalidStartDateException()
        {
            _updateCaseStatusRequest.StartDate = DateTime.Now.AddDays(1);
            _updateCaseStatusRequest.EndDate = null;
            _caseStatus.Type = "CIN";

            _mockCaseStatusGateway.Setup(x => x.GetCasesStatusByCaseStatusId(_caseStatus.Id)).Returns(_caseStatus.ToDomain());

            Action act = () => _caseStatusesUseCase.ExecuteUpdate(_updateCaseStatusRequest);

            act.Should().Throw<InvalidStartDateException>()
                .WithMessage("Invalid start date. It cannot be in the future for CIN, CP or LAC.");
        }

        //CP
        [Test]
        public void WhenTypeIsCPandEndDateIsNotProvidedAndStartDateIsInteFutureItThrowsInvalidStartDateException()
        {
            _updateCaseStatusRequest.StartDate = DateTime.Now.AddDays(1);
            _updateCaseStatusRequest.EndDate = null;
            _caseStatus.Type = "CP";

            _mockCaseStatusGateway.Setup(x => x.GetCasesStatusByCaseStatusId(_caseStatus.Id)).Returns(_caseStatus.ToDomain());

            Action act = () => _caseStatusesUseCase.ExecuteUpdate(_updateCaseStatusRequest);

            act.Should().Throw<InvalidStartDateException>()
                .WithMessage("Invalid start date. It cannot be in the future for CIN, CP or LAC.");
        }

        [Test]
        public void WhenTypeIsCPandEndDateIsNotProvidedAndStartDateIsNotProvidedtThrowsInvalidStartDateException()
        {
            _updateCaseStatusRequest.StartDate = null;
            _updateCaseStatusRequest.EndDate = null;
            _caseStatus.Type = "CP";

            _mockCaseStatusGateway.Setup(x => x.GetCasesStatusByCaseStatusId(_caseStatus.Id)).Returns(_caseStatus.ToDomain());

            Action act = () => _caseStatusesUseCase.ExecuteUpdate(_updateCaseStatusRequest);

            act.Should().Throw<InvalidStartDateException>()
                .WithMessage("You must provide a valid date for CP");
        }

        [Test]
        public void WhenTypeIsCPandEndDateIsNotProvidedAndDefaultStartDateIsProvidedtThrowsInvalidStartDateException()
        {
            _updateCaseStatusRequest.StartDate = DateTime.MinValue;
            _updateCaseStatusRequest.EndDate = null;
            _caseStatus.Type = "CP";

            _mockCaseStatusGateway.Setup(x => x.GetCasesStatusByCaseStatusId(_caseStatus.Id)).Returns(_caseStatus.ToDomain());

            Action act = () => _caseStatusesUseCase.ExecuteUpdate(_updateCaseStatusRequest);

            act.Should().Throw<InvalidStartDateException>()
                .WithMessage("You must provide a valid date for CP");
        }

        [Test]
        [TestCaseSource(nameof(_invalidPCAnswers))]
        public void WhenTypeIsPCAndProvidedAnswerIsNotValidItThrowsInvalidUpdateRequestException(List<CaseStatusValue> answers)
        {
            _updateCaseStatusRequest.Answers = answers;
            _updateCaseStatusRequest.EndDate = null;
            _updateCaseStatusRequest.StartDate = DateTime.Today.AddDays(-10);

            _caseStatus.Type = "CP";

            _mockCaseStatusGateway.Setup(x => x.GetCasesStatusByCaseStatusId(_caseStatus.Id)).Returns(_caseStatus.ToDomain());

            Action act = () => _caseStatusesUseCase.ExecuteUpdate(_updateCaseStatusRequest);

            act.Should().Throw<InvalidCaseStatusUpdateRequestException>().WithMessage("Invalid PC answer");
        }


        //LAC
        [Test]
        public void WhenTypeIsLACandEndDateIsNotProvidedAndStartDateIsInteFutureItThrowsInvalidStartDateException()
        {
            _updateCaseStatusRequest.StartDate = DateTime.Now.AddDays(1);
            _updateCaseStatusRequest.EndDate = null;
            _caseStatus.Type = "LAC";

            _mockCaseStatusGateway.Setup(x => x.GetCasesStatusByCaseStatusId(_caseStatus.Id)).Returns(_caseStatus.ToDomain());

            Action act = () => _caseStatusesUseCase.ExecuteUpdate(_updateCaseStatusRequest);

            act.Should().Throw<InvalidStartDateException>()
                .WithMessage("Invalid start date. It cannot be in the future for CIN, CP or LAC.");
        }

        [Test]
        [TestCaseSource(nameof(_invalidLACAnswers))]
        public void WhenTypeIsLACAndProvidedAnswersAreNotValidItThrowsInvalidUpdateRequestException(List<CaseStatusValue> answers)
        {
            _updateCaseStatusRequest.Answers = answers;

            _updateCaseStatusRequest.EndDate = null;
            _caseStatus.Type = "LAC";

            _mockCaseStatusGateway.Setup(x => x.GetCasesStatusByCaseStatusId(_caseStatus.Id)).Returns(_caseStatus.ToDomain());

            Action act = () => _caseStatusesUseCase.ExecuteUpdate(_updateCaseStatusRequest);

            act.Should().Throw<InvalidCaseStatusUpdateRequestException>().WithMessage("Invalid LAC answers");
        }

        [Test]
        [TestCaseSource(nameof(_invalidLACEpisodeEndingAnswer))]
        public void WhenTypeIsLACandEndDateIsProvidedAndEpisodeEndingAnswersIsNotValidItThrowsInvalidCaseStatusUpdateRequestException(List<CaseStatusValue> answers)
        {
            _updateCaseStatusRequest.EndDate = DateTime.Today.Date;
            _updateCaseStatusRequest.Answers = answers;
            _caseStatus.Type = "LAC";

            _mockCaseStatusGateway.Setup(x => x.GetCasesStatusByCaseStatusId(_caseStatus.Id)).Returns(_caseStatus.ToDomain());

            Action act = () => _caseStatusesUseCase.ExecuteUpdate(_updateCaseStatusRequest);

            act.Should().Throw<InvalidCaseStatusUpdateRequestException>().WithMessage("Invalid LAC episode ending answer");
        }

        [Test]
        public void TestUpdatingACaseStatusReturnsUpdatedCaseStatusResponse()
        {
            var response = _caseStatusesUseCase.ExecuteUpdate(_updateCaseStatusRequest);

            response.Should().BeEquivalentTo(_updatedCaseStatus.ToDomain().ToResponse());
        }

    }
}
