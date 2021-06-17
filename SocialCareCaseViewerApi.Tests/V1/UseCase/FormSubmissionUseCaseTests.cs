using System;
using System.Linq;
using Bogus;
using FluentAssertions;
using MongoDB.Bson;
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
        private Faker _faker;
        private const string CollectionName = "resident-case-submissions";

        [SetUp]
        public void SetUp()
        {
            _faker = new Faker();
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
            _mockDatabaseGateway.Setup(x => x.GetPersonDetailsById(request.ResidentId)).Returns(resident);
            _mockMongoGateway.Setup(x => x.InsertRecord(It.IsAny<string>(), It.IsAny<CaseSubmission>()));

            var (caseSubmissionResponse, caseSubmission) = _formSubmissionsUseCase.ExecutePost(request);
            var expectedResponse = TestHelpers.CreateCaseSubmission(SubmissionState.InProgress,
                caseSubmission.CreatedAt, worker, resident, caseSubmission.SubmissionId, request.FormId);
            caseSubmissionResponse.SubmissionId = expectedResponse.SubmissionId ?? "0";

            caseSubmissionResponse.Should().BeEquivalentTo(expectedResponse.ToDomain().ToResponse());
            _mockDatabaseGateway.Verify(x => x.GetWorkerByEmail(request.CreatedBy), Times.Once);
            _mockDatabaseGateway.Verify(x => x.GetPersonDetailsById(request.ResidentId), Times.Once);
            _mockMongoGateway.Verify(x => x.InsertRecord(CollectionName, caseSubmission), Times.Once);
        }

        [Test]
        public void ExecutePostSetsTheWorkersTeamsAndAllocationsToNull()
        {
            var request = TestHelpers.CreateCaseSubmissionRequest();
            var worker = TestHelpers.CreateWorker();
            var resident = TestHelpers.CreatePerson();

            _mockDatabaseGateway.Setup(x => x.GetWorkerByEmail(request.CreatedBy)).Returns(worker);
            _mockDatabaseGateway.Setup(x => x.GetPersonDetailsById(request.ResidentId)).Returns(resident);
            _mockMongoGateway.Setup(x => x.InsertRecord(It.IsAny<string>(), It.IsAny<CaseSubmission>()));

            var (caseSubmissionResponse, caseSubmission) = _formSubmissionsUseCase.ExecutePost(request);

            caseSubmission?.Workers.FirstOrDefault()?.WorkerTeams.Should().BeNull();
            caseSubmission?.Workers.FirstOrDefault()?.Allocations.Should().BeNull();
            caseSubmissionResponse?.Workers.FirstOrDefault()?.Teams.Should().BeEmpty();
            caseSubmissionResponse?.Workers.FirstOrDefault()?.AllocationCount.Should().Be(0);
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
            _mockMongoGateway.Setup(x => x.LoadRecordById<CaseSubmission>(It.IsAny<string>(), ObjectId.Parse(submissionResponse.SubmissionId))).Returns(submissionResponse);

            var response = _formSubmissionsUseCase.ExecuteGetById(submissionResponse.SubmissionId ?? "");

            _mockMongoGateway.Verify(x => x.LoadRecordById<CaseSubmission>(It.IsAny<string>(), ObjectId.Parse(submissionResponse.SubmissionId)), Times.Once);
            response.Should().BeEquivalentTo(submissionResponse.ToDomain().ToResponse());
        }

        [Test]
        public void ExecuteGetByIdShouldReturnNullIfNoCaseIsFound()
        {
            var objectId = _faker.Random.String2(24, "0123456789acbdef");
            var response = _formSubmissionsUseCase.ExecuteGetById(objectId);

            _mockMongoGateway.Verify(x => x.LoadRecordById<CaseSubmission>(It.IsAny<string>(), ObjectId.Parse(objectId)), Times.Once);
            response.Should().BeNull();
        }

        [Test]
        public void ExecuteFinishSubmissionSuccessfullyCompletesASubmission()
        {
            var request = TestHelpers.FinishCaseSubmissionRequest();
            var createdSubmission = TestHelpers.CreateCaseSubmission(SubmissionState.InProgress);
            var worker = TestHelpers.CreateWorker();
            _mockDatabaseGateway.Setup(x => x.GetWorkerByEmail(request.CreatedBy)).Returns(worker);
            _mockMongoGateway.Setup(x => x.LoadRecordById<CaseSubmission>(CollectionName, ObjectId.Parse(createdSubmission.SubmissionId))).Returns(createdSubmission);

            _formSubmissionsUseCase.ExecuteFinishSubmission(createdSubmission.SubmissionId ?? "", request);
            _mockMongoGateway.Verify(x => x.UpsertRecord(CollectionName, ObjectId.Parse(createdSubmission.SubmissionId), It.IsAny<CaseSubmission>()), Times.Once);
        }

        [Test]
        public void ExecuteFinishSubmissionSetsTheWorkersTeamsAndAllocationsToNull()
        {
            var request = TestHelpers.FinishCaseSubmissionRequest();
            var createdSubmission = TestHelpers.CreateCaseSubmission(SubmissionState.InProgress);
            var worker = TestHelpers.CreateWorker();
            _mockDatabaseGateway.Setup(x => x.GetWorkerByEmail(request.CreatedBy)).Returns(worker);
            _mockMongoGateway.Setup(x => x.LoadRecordById<CaseSubmission>(CollectionName, ObjectId.Parse(createdSubmission.SubmissionId))).Returns(createdSubmission);

            _formSubmissionsUseCase.ExecuteFinishSubmission(createdSubmission.SubmissionId ?? "", request);

            worker.WorkerTeams.Should().BeNull();
            worker.Allocations.Should().BeNull();
        }

        [Test]
        public void UpdateAnswersSuccessfullyChangesSubmissionAnswers()
        {
            var request = TestHelpers.CreateUpdateFormSubmissionAnswersRequest();
            var createdSubmission = TestHelpers.CreateCaseSubmission(SubmissionState.InProgress);
            var worker = TestHelpers.CreateWorker();
            const string stepId = "1";
            _mockDatabaseGateway.Setup(x => x.GetWorkerByEmail(request.EditedBy)).Returns(worker);
            _mockMongoGateway
                .Setup(x => x.LoadRecordById<CaseSubmission>(CollectionName, ObjectId.Parse(createdSubmission.SubmissionId)))
                .Returns(createdSubmission);
            _mockMongoGateway.Setup(x =>
                x.UpsertRecord(It.IsAny<string>(), It.IsAny<ObjectId>(), It.IsAny<CaseSubmission>()));

            var response = _formSubmissionsUseCase.UpdateAnswers(createdSubmission.SubmissionId ?? "", stepId, request);

            response.FormAnswers[stepId].Should().BeEquivalentTo(request.StepAnswers);
            response.EditHistory.LastOrDefault()?.Worker.Should().BeEquivalentTo(worker.ToDomain(false).ToResponse());
            _mockDatabaseGateway.Verify(x => x.GetWorkerByEmail(request.EditedBy), Times.Once);
            _mockMongoGateway.Verify(x => x.LoadRecordById<CaseSubmission>(CollectionName, ObjectId.Parse(createdSubmission.SubmissionId)), Times.Once);
            _mockMongoGateway.Verify(x => x.UpsertRecord(CollectionName, ObjectId.Parse(createdSubmission.SubmissionId), It.IsAny<CaseSubmission>()), Times.Once);
        }

        [Test]
        public void UpdateAnswersSetsTheWorkersTeamsAndAllocationsToNull()
        {
            var request = TestHelpers.CreateUpdateFormSubmissionAnswersRequest();
            var worker = TestHelpers.CreateWorker();
            var createdSubmission = TestHelpers.CreateCaseSubmission(SubmissionState.InProgress, null, worker);
            const string stepId = "1";
            _mockDatabaseGateway.Setup(x => x.GetWorkerByEmail(request.EditedBy)).Returns(worker);
            _mockMongoGateway
                .Setup(x => x.LoadRecordById<CaseSubmission>(CollectionName, ObjectId.Parse(createdSubmission.SubmissionId)))
                .Returns(createdSubmission);
            _mockMongoGateway.Setup(x =>
                x.UpsertRecord(It.IsAny<string>(), It.IsAny<ObjectId>(), It.IsAny<CaseSubmission>()));

            var response = _formSubmissionsUseCase.UpdateAnswers(createdSubmission.SubmissionId ?? "", stepId, request);

            worker.WorkerTeams.Should().BeNull();
            worker.Allocations.Should().BeNull();
            response.Workers.FirstOrDefault()?.Teams.Should().BeEmpty();
            response.Workers.FirstOrDefault()?.AllocationCount.Should().Be(0);
        }

        [Test]
        public void ExecuteFinishSubmissionThrowsWorkerNotFoundExceptionWhenNoWorkerFoundFromRequest()
        {
            var request = TestHelpers.FinishCaseSubmissionRequest();
            var createdSubmission = TestHelpers.CreateCaseSubmission();
            _mockDatabaseGateway.Setup(x => x.GetWorkerByEmail(request.CreatedBy));

            Action act = () => _formSubmissionsUseCase.ExecuteFinishSubmission(createdSubmission.SubmissionId ?? "", request);

            act.Should().Throw<WorkerNotFoundException>().WithMessage($"Worker with email {request.CreatedBy} not found");
        }

        [Test]
        public void ExecuteFinishSubmissionThrowsGetSubmissionExceptionWhenNoSubmissionFoundFromRequest()
        {
            var request = TestHelpers.FinishCaseSubmissionRequest();
            var createdSubmission = TestHelpers.CreateCaseSubmission();
            var worker = TestHelpers.CreateWorker();
            _mockDatabaseGateway.Setup(x => x.GetWorkerByEmail(request.CreatedBy)).Returns(worker);
            _mockMongoGateway.Setup(x => x.LoadRecordById<CaseSubmission>(CollectionName, ObjectId.Parse(createdSubmission.SubmissionId)));

            Action act = () => _formSubmissionsUseCase.ExecuteFinishSubmission(createdSubmission.SubmissionId ?? "", request);

            act.Should().Throw<GetSubmissionException>().WithMessage($"Submission with ID {createdSubmission.SubmissionId} not found");
        }

        [Test]
        public void UpdateAnswersThrowsGetSubmissionExceptionWhenNoSubmissionFoundFromRequest()
        {
            var request = TestHelpers.CreateUpdateFormSubmissionAnswersRequest();
            var createdSubmission = TestHelpers.CreateCaseSubmission();
            var worker = TestHelpers.CreateWorker();
            const string stepId = "1";
            _mockDatabaseGateway.Setup(x => x.GetWorkerByEmail(request.EditedBy)).Returns(worker);
            _mockMongoGateway.Setup(x => x.LoadRecordById<CaseSubmission>(CollectionName, ObjectId.Parse(createdSubmission.SubmissionId)));

            Action act = () => _formSubmissionsUseCase.UpdateAnswers(createdSubmission.SubmissionId ?? "", stepId, request);

            act.Should().Throw<GetSubmissionException>()
                .WithMessage($"Submission with ID {createdSubmission.SubmissionId} not found");
        }

        [Test]
        public void UpdateAnswersThrowsWorkerNotFoundExceptionWhenNoWorkerFoundFromRequest()
        {
            var request = TestHelpers.CreateUpdateFormSubmissionAnswersRequest();
            var createdSubmission = TestHelpers.CreateCaseSubmission();
            const string stepId = "1";
            _mockDatabaseGateway.Setup(x => x.GetWorkerByEmail(request.EditedBy));

            Action act = () => _formSubmissionsUseCase.UpdateAnswers(createdSubmission.SubmissionId ?? "", stepId, request);

            act.Should().Throw<WorkerNotFoundException>()
                .WithMessage($"Worker with email {request.EditedBy} not found");
        }
    }
}
