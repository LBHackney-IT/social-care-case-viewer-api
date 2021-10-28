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
        public void WhenRequestedEndDateIsBeforeCaseStatusStartDateAndTypeIsCINorCPInvalidEndDateExceptionThrown(string type)
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
        public void WhenRequestedEndDateIsOnCaseStatusStartDateAndTypeIsCINorCPItCallsTheGatewayToUpdateTheRecords(string type)
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
        public void WhenTypeIsLACandTheProvidedEndIsBeforeTheCurrentlyActiveAnswersStartDateItThrowsInvalidEndDateException()
        {
            var answers = TestHelpers.CreateCaseStatusAnswers(min: 2, max: 2, startDate: new DateTime(2021, 11, 3));

            _caseStatus = TestHelpers.CreateCaseStatus(resident: _resident, startDate: DateTime.Now.AddDays(1), type: "LAC");
            _caseStatus.Answers.AddRange(answers);

            _updateCaseStatusRequest = TestHelpers.CreateUpdateCaseStatusRequest(caseStatusId: _caseStatus.Id, email: _worker.Email, endDate: new DateTime(2021, 11, 1));

            _mockCaseStatusGateway
                .Setup(x => x.GetCasesStatusByCaseStatusId(_caseStatus.Id))
                .Returns(_caseStatus.ToDomain());

            Action act = () => _caseStatusesUseCase.ExecuteUpdate(_updateCaseStatusRequest);

            act.Should().Throw<InvalidEndDateException>()
                .WithMessage("requested end date is before the start date of the currently active answer");
        }

        [Test]
        public void WhenRequestedEndDateIsOnActiveCaseStatusAnswerStartDateAndTypeIsLACItCallsTheGateway()
        {
            _caseStatus = TestHelpers.CreateCaseStatus(resident: _resident, startDate: new DateTime(2021, 11, 3), type: "LAC");
            _updateCaseStatusRequest = TestHelpers.CreateUpdateCaseStatusRequest(caseStatusId: _caseStatus.Id, email: _worker.Email, endDate: new DateTime(2021, 11, 3));

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
        public void TestUpdatingACaseStatusReturnsUpdatedCaseStatusResponse()
        {
            var response = _caseStatusesUseCase.ExecuteUpdate(_updateCaseStatusRequest);

            response.Should().BeEquivalentTo(_updatedCaseStatus.ToDomain().ToResponse());
        }
    }
}
