using System;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SocialCareCaseViewerApi.Tests.V1.Helpers;
using SocialCareCaseViewerApi.V1.Exceptions;
using SocialCareCaseViewerApi.V1.Factories;
using SocialCareCaseViewerApi.V1.Gateways;
using SocialCareCaseViewerApi.V1.Gateways.Interfaces;
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

        [SetUp]
        public void SetUp()
        {
            _mockCaseStatusGateway = new Mock<ICaseStatusGateway>();
            _mockDatabaseGateway = new Mock<IDatabaseGateway>();
            _caseStatusesUseCase = new CaseStatusesUseCase(_mockCaseStatusGateway.Object, _mockDatabaseGateway.Object);
        }

        [Test]
        public void TestUseCaseMakesAppropriateCallsToOurGateways()
        {
            var resident = TestHelpers.CreatePerson(ageContext: "C");
            var worker = TestHelpers.CreateWorker();
            var caseStatus = TestHelpers.CreateCaseStatus(resident: resident);

            var updateCaseStatusRequest = TestHelpers.CreateUpdateCaseStatusRequest(personId: resident.Id, email: worker.Email);
            var updatedCaseStatus = TestHelpers.CreateCaseStatus(resident.Id, caseStatus.TypeId, caseStatus.Notes,
                caseStatus.StartDate, updateCaseStatusRequest.EndDate, resident: resident);

            _mockCaseStatusGateway
                .Setup(x => x.GetCasesStatusByCaseStatusId(caseStatus.Id))
                .Returns(caseStatus.ToDomain());

            _mockCaseStatusGateway
                .Setup(x => x.UpdateCaseStatus(caseStatus.Id, updateCaseStatusRequest))
                .Returns(updatedCaseStatus.ToDomain());

            _mockDatabaseGateway.Setup(x => x.GetPersonByMosaicId(resident.Id)).Returns(resident);
            _mockDatabaseGateway.Setup(x => x.GetWorkerByEmail(worker.Email)).Returns(worker);

            _caseStatusesUseCase.ExecuteUpdate(caseStatus.Id, updateCaseStatusRequest);

            _mockCaseStatusGateway.Verify(x => x.GetCasesStatusByCaseStatusId(caseStatus.Id), Times.Once);
            _mockCaseStatusGateway.Verify(x => x.UpdateCaseStatus(caseStatus.Id, updateCaseStatusRequest), Times.Once);
            _mockDatabaseGateway.Verify(x => x.GetPersonByMosaicId(resident.Id), Times.Once);
            _mockDatabaseGateway.Verify(x => x.GetWorkerByEmail(worker.Email), Times.Once);
        }

        [Test]
        public void TestWhenResidentNotFoundWithRequestIdPersonNotFoundExceptionThrown()
        {
            const int nonExistentResidentId = 0;
            var caseStatus = TestHelpers.CreateCaseStatus();
            var updateCaseStatusRequest = TestHelpers.CreateUpdateCaseStatusRequest(personId: nonExistentResidentId);

            _mockCaseStatusGateway
                .Setup(x => x.GetCasesStatusByCaseStatusId(caseStatus.Id))
                .Returns(caseStatus.ToDomain());

            _mockDatabaseGateway.Setup(x => x.GetPersonByMosaicId(nonExistentResidentId));

            Action act = () => _caseStatusesUseCase.ExecuteUpdate(caseStatus.Id, updateCaseStatusRequest);

            act.Should().Throw<PersonNotFoundException>()
                .WithMessage($"'personId' with '{nonExistentResidentId}' was not found");
        }

        [Test]
        public void TestWhenResidentAgeContextIsNotChildrensInvalidAgeContextExceptionThrown()
        {
            var resident = TestHelpers.CreatePerson(ageContext: "A");
            var caseStatus = TestHelpers.CreateCaseStatus();
            var updateCaseStatusRequest = TestHelpers.CreateUpdateCaseStatusRequest(personId: resident.Id);

            _mockCaseStatusGateway
                .Setup(x => x.GetCasesStatusByCaseStatusId(caseStatus.Id))
                .Returns(caseStatus.ToDomain());

            _mockDatabaseGateway.Setup(x => x.GetPersonByMosaicId(resident.Id)).Returns(resident);

            Action act = () => _caseStatusesUseCase.ExecuteUpdate(caseStatus.Id, updateCaseStatusRequest);

            act.Should().Throw<InvalidAgeContextException>()
                .WithMessage($"Person with the id {resident.Id} belongs to the wrong AgeContext for this operation");
        }

        [Test]
        public void TestWhenResidentIdFromRequestDoesNotMatchResidentOnCaseStatusThrowCaseStatusDoesNotMatchPersonException()
        {
            var resident = TestHelpers.CreatePerson(ageContext: "C");
            var residentOnCaseStatus = TestHelpers.CreatePerson(ageContext: "C");
            var caseStatus = TestHelpers.CreateCaseStatus(resident: residentOnCaseStatus);
            var updateCaseStatusRequest = TestHelpers.CreateUpdateCaseStatusRequest(personId: resident.Id);

            _mockCaseStatusGateway
                .Setup(x => x.GetCasesStatusByCaseStatusId(caseStatus.Id))
                .Returns(caseStatus.ToDomain());

            _mockDatabaseGateway.Setup(x => x.GetPersonByMosaicId(resident.Id)).Returns(resident);

            Action act = () => _caseStatusesUseCase.ExecuteUpdate(caseStatus.Id, updateCaseStatusRequest);

            act.Should().Throw<CaseStatusDoesNotMatchPersonException>()
                .WithMessage($"Retrieved case status does not match the provided person id of {updateCaseStatusRequest.PersonId}");
        }

        [Test]
        public void TestWhenRequestEditedByWorkerEmailNotFoundWorkerNotFoundExceptionThrown()
        {
            var worker = TestHelpers.CreateWorker();
            var resident = TestHelpers.CreatePerson(ageContext: "C");
            var caseStatus = TestHelpers.CreateCaseStatus(resident: resident);
            var updateCaseStatusRequest = TestHelpers.CreateUpdateCaseStatusRequest(personId: resident.Id, email: worker.Email);

            _mockCaseStatusGateway
                .Setup(x => x.GetCasesStatusByCaseStatusId(caseStatus.Id))
                .Returns(caseStatus.ToDomain());

            _mockDatabaseGateway.Setup(x => x.GetPersonByMosaicId(resident.Id)).Returns(resident);
            _mockDatabaseGateway.Setup(x => x.GetWorkerByEmail(updateCaseStatusRequest.EditedBy));

            Action act = () => _caseStatusesUseCase.ExecuteUpdate(caseStatus.Id, updateCaseStatusRequest);

            act.Should().Throw<WorkerNotFoundException>()
                .WithMessage($"Worker with email `{updateCaseStatusRequest.EditedBy}` was not found");
        }

        [Test]
        public void TestWhenUpdatingCaseStatusAndCaseStatusNotFoundCaseStatusDoesNotExistExceptionThrown()
        {
            var caseStatus = TestHelpers.CreateCaseStatus();
            var updateCaseStatusRequest = TestHelpers.CreateUpdateCaseStatusRequest();

            _mockCaseStatusGateway.Setup(x => x.GetCasesStatusByCaseStatusId(caseStatus.Id));

            Action act = () => _caseStatusesUseCase.ExecuteUpdate(caseStatus.Id, updateCaseStatusRequest);

            act.Should().Throw<CaseStatusDoesNotExistException>()
                .WithMessage($"Case status with {caseStatus.Id} not found");
        }

        [Test]
        public void TestUpdatingACaseStatusReturnsUpdatedCaseStatusResponse()
        {
            var worker = TestHelpers.CreateWorker();
            var resident = TestHelpers.CreatePerson(ageContext: "C");
            var caseStatus = TestHelpers.CreateCaseStatus(resident: resident);
            var updateCaseStatusRequest = TestHelpers.CreateUpdateCaseStatusRequest(personId: resident.Id, email: worker.Email);
            var updatedCaseStatus = TestHelpers.CreateCaseStatus(resident.Id, caseStatus.TypeId, caseStatus.Notes,
                caseStatus.StartDate, updateCaseStatusRequest.EndDate, resident: resident);

            _mockCaseStatusGateway
                .Setup(x => x.GetCasesStatusByCaseStatusId(caseStatus.Id))
                .Returns(caseStatus.ToDomain());

            _mockDatabaseGateway.Setup(x => x.GetPersonByMosaicId(resident.Id)).Returns(resident);
            _mockDatabaseGateway.Setup(x => x.GetWorkerByEmail(updateCaseStatusRequest.EditedBy)).Returns(worker);

            _mockCaseStatusGateway
                .Setup(x => x.UpdateCaseStatus(caseStatus.Id, updateCaseStatusRequest))
                .Returns(updatedCaseStatus.ToDomain());

            var response = _caseStatusesUseCase.ExecuteUpdate(caseStatus.Id, updateCaseStatusRequest);

            response.Should().BeEquivalentTo(updatedCaseStatus.ToDomain().ToResponse());
        }
    }
}
