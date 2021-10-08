using System;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SocialCareCaseViewerApi.Tests.V1.Helpers;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Exceptions;
using SocialCareCaseViewerApi.V1.Factories;
using SocialCareCaseViewerApi.V1.Gateways;
using SocialCareCaseViewerApi.V1.Gateways.Interfaces;
using SocialCareCaseViewerApi.V1.Infrastructure;
using SocialCareCaseViewerApi.V1.UseCase;

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
            _updateCaseStatusRequest = TestHelpers.CreateUpdateCaseStatusRequest(personId: _resident.Id, email: _worker.Email);
            _updatedCaseStatus = TestHelpers.CreateCaseStatus(_resident.Id, _caseStatus.TypeId, _caseStatus.Notes,
                _caseStatus.StartDate, _updateCaseStatusRequest.EndDate, resident: _resident);

            _mockCaseStatusGateway
                .Setup(x => x.GetCasesStatusByCaseStatusId(_caseStatus.Id))
                .Returns(_caseStatus.ToDomain());

            _mockCaseStatusGateway
                .Setup(x => x.UpdateCaseStatus(_caseStatus.Id, _updateCaseStatusRequest))
                .Returns(_updatedCaseStatus.ToDomain());

            _mockDatabaseGateway.Setup(x => x.GetPersonByMosaicId(_resident.Id)).Returns(_resident);
            _mockDatabaseGateway.Setup(x => x.GetWorkerByEmail(_worker.Email)).Returns(_worker);
        }

        [Test]
        public void TestUseCaseMakesAppropriateCallsToOurGateways()
        {
            _caseStatusesUseCase.ExecuteUpdate(_caseStatus.Id, _updateCaseStatusRequest);

            _mockCaseStatusGateway.Verify(x => x.GetCasesStatusByCaseStatusId(_caseStatus.Id), Times.Once);
            _mockCaseStatusGateway.Verify(x => x.UpdateCaseStatus(_caseStatus.Id, _updateCaseStatusRequest), Times.Once);
            _mockDatabaseGateway.Verify(x => x.GetPersonByMosaicId(_resident.Id), Times.Once);
            _mockDatabaseGateway.Verify(x => x.GetWorkerByEmail(_worker.Email), Times.Once);
        }

        [Test]
        public void TestWhenResidentNotFoundWithRequestIdPersonNotFoundExceptionThrown()
        {
            const int nonExistentResidentId = 0;
            _updateCaseStatusRequest = TestHelpers.CreateUpdateCaseStatusRequest(personId: nonExistentResidentId);

            _mockDatabaseGateway.Setup(x => x.GetPersonByMosaicId(nonExistentResidentId));

            Action act = () => _caseStatusesUseCase.ExecuteUpdate(_caseStatus.Id, _updateCaseStatusRequest);

            act.Should().Throw<PersonNotFoundException>()
                .WithMessage($"'personId' with '{nonExistentResidentId}' was not found");
        }

        [Test]
        public void TestWhenResidentAgeContextIsNotChildrensInvalidAgeContextExceptionThrown()
        {
            _resident = TestHelpers.CreatePerson(ageContext: "A");
            _updateCaseStatusRequest = TestHelpers.CreateUpdateCaseStatusRequest(personId: _resident.Id);

            _mockDatabaseGateway.Setup(x => x.GetPersonByMosaicId(_resident.Id)).Returns(_resident);

            Action act = () => _caseStatusesUseCase.ExecuteUpdate(_caseStatus.Id, _updateCaseStatusRequest);

            act.Should().Throw<InvalidAgeContextException>()
                .WithMessage($"Person with the id {_resident.Id} belongs to the wrong AgeContext for this operation");
        }

        [Test]
        public void TestWhenResidentIdFromRequestDoesNotMatchResidentOnCaseStatusThrowCaseStatusDoesNotMatchPersonException()
        {
            var residentOnCaseStatus = TestHelpers.CreatePerson(ageContext: "C");
            _caseStatus = TestHelpers.CreateCaseStatus(resident: residentOnCaseStatus);

            _mockCaseStatusGateway
                .Setup(x => x.GetCasesStatusByCaseStatusId(_caseStatus.Id))
                .Returns(_caseStatus.ToDomain());

            Action act = () => _caseStatusesUseCase.ExecuteUpdate(_caseStatus.Id, _updateCaseStatusRequest);

            act.Should().Throw<CaseStatusDoesNotMatchPersonException>()
                .WithMessage($"Retrieved case status does not match the provided person id of {_updateCaseStatusRequest.PersonId}");
        }

        [Test]
        public void TestWhenRequestEditedByWorkerEmailNotFoundWorkerNotFoundExceptionThrown()
        {
            _mockDatabaseGateway.Setup(x => x.GetWorkerByEmail(_updateCaseStatusRequest.EditedBy));

            Action act = () => _caseStatusesUseCase.ExecuteUpdate(_caseStatus.Id, _updateCaseStatusRequest);

            act.Should().Throw<WorkerNotFoundException>()
                .WithMessage($"Worker with email `{_updateCaseStatusRequest.EditedBy}` was not found");
        }

        [Test]
        public void TestWhenUpdatingCaseStatusAndCaseStatusNotFoundCaseStatusDoesNotExistExceptionThrown()
        {
            _mockCaseStatusGateway.Setup(x => x.GetCasesStatusByCaseStatusId(_caseStatus.Id));

            Action act = () => _caseStatusesUseCase.ExecuteUpdate(_caseStatus.Id, _updateCaseStatusRequest);

            act.Should().Throw<CaseStatusDoesNotExistException>()
                .WithMessage($"Case status with {_caseStatus.Id} not found");
        }

        [Test]
        public void TestWhenRequestedEndDateIsBeforeCaseStatusStartDateInvalidEndDateExceptionThrown()
        {
            _caseStatus = TestHelpers.CreateCaseStatus(resident: _resident, startDate: DateTime.Now.AddDays(1));
            _updateCaseStatusRequest = TestHelpers.CreateUpdateCaseStatusRequest(personId: _resident.Id, email: _worker.Email, endDate: DateTime.Now.AddDays(-1));

            _mockCaseStatusGateway
                .Setup(x => x.GetCasesStatusByCaseStatusId(_caseStatus.Id))
                .Returns(_caseStatus.ToDomain());

            Action act = () => _caseStatusesUseCase.ExecuteUpdate(_caseStatus.Id, _updateCaseStatusRequest);

            act.Should().Throw<InvalidEndDateException>()
                .WithMessage($"requested end date of {_updateCaseStatusRequest.EndDate?.ToString("O")} " +
                             $"is before the start date of {_caseStatus.StartDate:O}");
        }

        [Test]
        public void TestUpdatingACaseStatusReturnsUpdatedCaseStatusResponse()
        {
            var response = _caseStatusesUseCase.ExecuteUpdate(_caseStatus.Id, _updateCaseStatusRequest);

            response.Should().BeEquivalentTo(_updatedCaseStatus.ToDomain().ToResponse());
        }
    }
}
