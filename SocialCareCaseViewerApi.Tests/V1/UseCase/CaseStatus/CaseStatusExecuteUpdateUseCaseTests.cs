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
        public void TestWhenRequestedEndDateIsBeforeCaseStatusStartDateInvalidEndDateExceptionThrown()
        {
            _caseStatus = TestHelpers.CreateCaseStatus(resident: _resident, startDate: DateTime.Now.AddDays(1));
            _updateCaseStatusRequest = TestHelpers.CreateUpdateCaseStatusRequest(caseStatusId: _caseStatus.Id, email: _worker.Email, endDate: DateTime.Now.AddDays(-1));

            _mockCaseStatusGateway
                .Setup(x => x.GetCasesStatusByCaseStatusId(_caseStatus.Id))
                .Returns(_caseStatus.ToDomain());

            Action act = () => _caseStatusesUseCase.ExecuteUpdate(_updateCaseStatusRequest);

            act.Should().Throw<InvalidEndDateException>()
                .WithMessage($"requested end date of {_updateCaseStatusRequest.EndDate?.ToString("O")} " +
                             $"is before the start date of {_caseStatus.StartDate:O}");
        }

        [Test]
        public void TestUpdatingACaseStatusReturnsUpdatedCaseStatusResponse()
        {
            var response = _caseStatusesUseCase.ExecuteUpdate(_updateCaseStatusRequest);

            response.Should().BeEquivalentTo(_updatedCaseStatus.ToDomain().ToResponse());
        }
    }
}
