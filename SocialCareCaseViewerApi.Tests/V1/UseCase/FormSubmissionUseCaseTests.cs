using System;
using System.Linq;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SocialCareCaseViewerApi.Tests.V1.Helpers;
using SocialCareCaseViewerApi.V1.Exceptions;
using SocialCareCaseViewerApi.V1.Factories;
using SocialCareCaseViewerApi.V1.Gateways;
using SocialCareCaseViewerApi.V1.Infrastructure;
using SocialCareCaseViewerApi.V1.UseCase;

namespace SocialCareCaseViewerApi.Tests.V1.UseCase
{
    [TestFixture]
    public class FormSubmissionUseCaseTests
    {
        private Mock<IDatabaseGateway> _mockDatabaseGateway;
        private Mock<IMongoGateway> _mockMongoGateway;
        private FormSubmissionsUseCase _formSubmissionsUseCase;
        private const string CollectionName = "resident-case-submissions";

        [SetUp]
        public void SetUp()
        {
            _mockDatabaseGateway = new Mock<IDatabaseGateway>();
            _mockMongoGateway = new Mock<IMongoGateway>();
            _formSubmissionsUseCase = new FormSubmissionsUseCase(_mockDatabaseGateway.Object, _mockMongoGateway.Object);
        }

        [Test]
        public void ExecutePostSuccessfully()
        {
            var request = TestHelpers.CreateCaseSubmissionRequest();
            var worker = TestHelpers.CreateWorker();
            var resident = TestHelpers.CreatePerson();

            _mockDatabaseGateway.Setup(x => x.GetWorkerByEmail(request.CreatedBy)).Returns(worker);
            _mockDatabaseGateway.Setup(x => x.GetPersonByMosaicId(request.ResidentId)).Returns(resident);
            _mockMongoGateway.Setup(x => x.InsertRecord(It.IsAny<string>(), It.IsAny<CaseSubmission>()));

            var (caseSubmissionResponse, caseSubmission) = _formSubmissionsUseCase.ExecutePost(request);
            var expectedResponse = TestHelpers.CreateCaseSubmission(SubmissionState.InProgress,
                caseSubmission.CreatedAt, worker, resident, caseSubmission.SubmissionId, request.FormId);

            caseSubmissionResponse.Should().BeEquivalentTo(expectedResponse.ToDomain().ToResponse());
            _mockDatabaseGateway.Verify(x => x.GetWorkerByEmail(request.CreatedBy), Times.Once);
            _mockDatabaseGateway.Verify(x => x.GetPersonByMosaicId(request.ResidentId), Times.Once);
            _mockMongoGateway.Verify(x => x.InsertRecord(CollectionName, caseSubmission), Times.Once);
        }

        [Test]
        public void ExecutePostThrowsAnErrorWhenNoWorkerFoundForRequest()
        {
            var request = TestHelpers.CreateCaseSubmissionRequest();
            _mockDatabaseGateway.Setup(x => x.GetWorkerByEmail(request.CreatedBy));

            Action act = () => _formSubmissionsUseCase.ExecutePost(request);

            act.Should().Throw<WorkerNotFoundException>()
                .WithMessage($"Worker with email {request.CreatedBy} not found");
        }

        [Test]
        public void ExecutePostThrowsAnErrorWhenNoResidentFoundForRequest()
        {
            var request = TestHelpers.CreateCaseSubmissionRequest();
            var worker = TestHelpers.CreateWorker();
            _mockDatabaseGateway.Setup(x => x.GetWorkerByEmail(request.CreatedBy)).Returns(worker);
            _mockDatabaseGateway.Setup(x => x.GetPersonByMosaicId(request.ResidentId));

            Action act = () => _formSubmissionsUseCase.ExecutePost(request);

            act.Should().Throw<PersonNotFoundException>()
                .WithMessage($"Person with id {request.ResidentId} not found");
        }

        [Test]
        public void ExecuteGetByIdSuccessfully()
        {
            var submissionResponse = TestHelpers.CreateCaseSubmission();
            _mockMongoGateway.Setup(x => x.LoadRecordById<CaseSubmission>(It.IsAny<string>(), submissionResponse.SubmissionId)).Returns(submissionResponse);

            var response = _formSubmissionsUseCase.ExecuteGetById(submissionResponse.SubmissionId);

            _mockMongoGateway.Verify(x => x.LoadRecordById<CaseSubmission>(It.IsAny<string>(), submissionResponse.SubmissionId), Times.Once);
            response.Should().BeEquivalentTo(submissionResponse.ToDomain().ToResponse());
        }

        [Test]
        public void ExecuteGetByIdShouldReturnNullIfNoCaseIsFound()
        {
            var nonExistentSubmissionId = new Guid();

            var response = _formSubmissionsUseCase.ExecuteGetById(nonExistentSubmissionId);

            _mockMongoGateway.Verify(x => x.LoadRecordById<CaseSubmission>(It.IsAny<string>(), nonExistentSubmissionId), Times.Once);
            response.Should().BeNull();
        }

        [Test]
        public void ExecuteFinishSubmissionSuccessfullyCompletesASubmission()
        {
            var request = TestHelpers.FinishCaseSubmissionRequest();
            var createdSubmission = TestHelpers.CreateCaseSubmission(SubmissionState.InProgress);
            var worker = TestHelpers.CreateWorker();
            _mockDatabaseGateway.Setup(x => x.GetWorkerByEmail(request.CreatedBy)).Returns(worker);
            _mockMongoGateway.Setup(x => x.LoadRecordById<CaseSubmission>(CollectionName, createdSubmission.SubmissionId)).Returns(createdSubmission);

            _formSubmissionsUseCase.ExecuteFinishSubmission(createdSubmission.SubmissionId, request);
            _mockMongoGateway.Verify(x => x.UpsertRecord(CollectionName, createdSubmission.SubmissionId, It.IsAny<CaseSubmission>()), Times.Once);
        }

        public void UpdateAnswersSuccessfullyChangesSubmissionAnswers()
        {
            var request = TestHelpers.CreateUpdateFormSubmissionAnswersRequest();
            var createdSubmission = TestHelpers.CreateCaseSubmission(SubmissionState.InProgress);
            var worker = TestHelpers.CreateWorker();
            const string stepId = "1";
            _mockDatabaseGateway.Setup(x => x.GetWorkerByEmail(request.EditedBy)).Returns(worker);
            _mockMongoGateway
                .Setup(x => x.LoadRecordById<CaseSubmission>(CollectionName, createdSubmission.SubmissionId))
                .Returns(createdSubmission);
            _mockMongoGateway.Setup(x =>
                x.UpsertRecord(It.IsAny<string>(), It.IsAny<Guid>(), It.IsAny<CaseSubmission>()));

            var response = _formSubmissionsUseCase.UpdateAnswers(createdSubmission.SubmissionId, stepId, request);

            response.Should().NotBeNull();
            response.FormAnswers[stepId].Should().BeEquivalentTo(request.StepAnswers);
            response.EditHistory.LastOrDefault()?.Worker.Should().BeEquivalentTo(worker.ToDomain(false).ToResponse());

            _mockDatabaseGateway.Verify(x => x.GetWorkerByEmail(request.EditedBy), Times.Once);
            _mockMongoGateway.Verify(x => x.LoadRecordById<CaseSubmission>(CollectionName, createdSubmission.SubmissionId), Times.Once);
            _mockMongoGateway.Verify(x => x.UpsertRecord(CollectionName, createdSubmission.SubmissionId, It.IsAny<CaseSubmission>()), Times.Once);
        }

        [Test]
        public void ExecuteFinishSubmissionThrowsWorkerNotFoundExecptionWhenNoWorkerFoundFromRequest()
        {
            var request = TestHelpers.FinishCaseSubmissionRequest();
            var createdSubmission = TestHelpers.CreateCaseSubmission();
            _mockDatabaseGateway.Setup(x => x.GetWorkerByEmail(request.CreatedBy));

            Action act = () => _formSubmissionsUseCase.ExecuteFinishSubmission(createdSubmission.SubmissionId, request);

            act.Should().Throw<WorkerNotFoundException>().WithMessage($"Worker with email {request.CreatedBy} not found");
        }

        [Test]
        public void ExecuteFinishSubmissionThrowsGetSubmissionExecptionWhenNoSubmissionFoundFromRequest()
        {
            var request = TestHelpers.FinishCaseSubmissionRequest();
            var createdSubmission = TestHelpers.CreateCaseSubmission();
            var worker = TestHelpers.CreateWorker();
            _mockDatabaseGateway.Setup(x => x.GetWorkerByEmail(request.CreatedBy)).Returns(worker);
            _mockMongoGateway.Setup(x => x.LoadRecordById<CaseSubmission>(CollectionName, createdSubmission.SubmissionId));

            Action act = () => _formSubmissionsUseCase.ExecuteFinishSubmission(createdSubmission.SubmissionId, request);

            act.Should().Throw<GetSubmissionException>().WithMessage($"Submission with ID {createdSubmission.SubmissionId.ToString()} not found");
        }

        [Test]
        public void UpdateAnswersThrowsGetSubmissionExceptionWhenNoSubmissionFoundFromRequest()
        {
            var request = TestHelpers.CreateUpdateFormSubmissionAnswersRequest();
            var createdSubmission = TestHelpers.CreateCaseSubmission();
            var worker = TestHelpers.CreateWorker();
            const string stepId = "1";
            _mockDatabaseGateway.Setup(x => x.GetWorkerByEmail(request.EditedBy)).Returns(worker);
            _mockMongoGateway.Setup(x => x.LoadRecordById<CaseSubmission>(CollectionName, createdSubmission.SubmissionId));

            Action act = () => _formSubmissionsUseCase.UpdateAnswers(createdSubmission.SubmissionId, stepId, request);

            act.Should().Throw<GetSubmissionException>()
                .WithMessage($"Submission with ID {createdSubmission.SubmissionId.ToString()} not found");
        }

        [Test]
        public void UpdateAnswersThrowsWorkerNotFoundExceptionWhenNoWorkerFoundFromRequest()
        {
            var request = TestHelpers.CreateUpdateFormSubmissionAnswersRequest();
            var createdSubmission = TestHelpers.CreateCaseSubmission();
            const string stepId = "1";
            _mockDatabaseGateway.Setup(x => x.GetWorkerByEmail(request.EditedBy));

            Action act = () => _formSubmissionsUseCase.UpdateAnswers(createdSubmission.SubmissionId, stepId, request);

            act.Should().Throw<WorkerNotFoundException>()
                .WithMessage($"Worker with email {request.EditedBy} not found");
        }
    }
}
