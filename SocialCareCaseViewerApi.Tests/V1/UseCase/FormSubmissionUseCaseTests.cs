using System;
using System.Collections.Generic;
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

#nullable enable
namespace SocialCareCaseViewerApi.Tests.V1.UseCase
{
    [TestFixture]
    public class FormSubmissionUseCaseTests
    {
        private Mock<IDatabaseGateway> _mockDatabaseGateway = null!;
        private Mock<IMongoGateway> _mockMongoGateway = null!;
        private FormSubmissionsUseCase _formSubmissionsUseCase = null!;
        private Faker _faker = null!;
        private const string CollectionName = "resident-case-submissions";

        private static object[] _invalidSubmissionStateForUpdates =
        {
            SubmissionState.Discarded,
            SubmissionState.Submitted,
            SubmissionState.Approved
        };

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
                caseSubmission.CreatedAt, worker, resident, null, caseSubmission.SubmissionId, request.FormId);
            caseSubmissionResponse.SubmissionId = expectedResponse.SubmissionId.ToString();

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

            caseSubmission.Workers.FirstOrDefault()?.WorkerTeams.Should().BeNull();
            caseSubmission.Workers.FirstOrDefault()?.Allocations.Should().BeNull();
            caseSubmissionResponse.Workers.FirstOrDefault()?.Teams.Should().BeEmpty();
            caseSubmissionResponse.Workers.FirstOrDefault()?.AllocationCount.Should().Be(0);
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
        public void ExecutePostThrowsPersonNotFoundExceptionWhenNoResidentFoundForRequest()
        {
            var request = TestHelpers.CreateCaseSubmissionRequest();
            var worker = TestHelpers.CreateWorker();
            _mockDatabaseGateway.Setup(x => x.GetWorkerByEmail(request.CreatedBy)).Returns(worker);
            _mockDatabaseGateway.Setup(x => x.GetPersonByMosaicId(request.ResidentId));

            Action act = () => _formSubmissionsUseCase.ExecutePost(request);

            act.Should().Throw<PersonNotFoundException>()
                .WithMessage($"Resident not found with id {request.ResidentId}");
        }

        [Test]
        public void ExecuteGetByIdSuccessfully()
        {
            var submissionResponse = TestHelpers.CreateCaseSubmission();
            _mockMongoGateway.Setup(x => x.LoadRecordById<CaseSubmission>(It.IsAny<string>(), submissionResponse.SubmissionId)).Returns(submissionResponse);

            var response = _formSubmissionsUseCase.ExecuteGetById(submissionResponse.SubmissionId.ToString());

            _mockMongoGateway.Verify(x => x.LoadRecordById<CaseSubmission>(It.IsAny<string>(), submissionResponse.SubmissionId), Times.Once);
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
        public void ExecuteListBySubmissionStatusSuccessfully()
        {
            var firstSubmission = TestHelpers.CreateCaseSubmission(SubmissionState.InProgress);
            var secondSubmission = TestHelpers.CreateCaseSubmission(SubmissionState.InProgress);

            var submissionResponse = new List<CaseSubmission> { firstSubmission, secondSubmission };

            _mockMongoGateway
                .Setup(x => x.LoadMultipleRecordsByProperty<CaseSubmission, SubmissionState>(It.IsAny<string>(), "SubmissionState", SubmissionState.InProgress))
                .Returns(submissionResponse);

            var response = _formSubmissionsUseCase.ExecuteListBySubmissionStatus(SubmissionState.InProgress);

            _mockMongoGateway
                .Verify(x => x.LoadMultipleRecordsByProperty<CaseSubmission, SubmissionState>(It.IsAny<string>(), "SubmissionState", SubmissionState.InProgress), Times.Once);

            response.Should().BeEquivalentTo(submissionResponse.Select(x => x.ToDomain().ToResponse()));
        }

        [Test]
        public void ExecuteListBySubmissionStatusShouldReturnAnEmptyListIfNoMatchesWereFound()
        {
            var response = _formSubmissionsUseCase.ExecuteListBySubmissionStatus(SubmissionState.Submitted);

            _mockMongoGateway
                .Verify(x => x.LoadMultipleRecordsByProperty<CaseSubmission, SubmissionState>(It.IsAny<string>(), "SubmissionState", SubmissionState.Submitted), Times.Once);

            response.Should().BeEmpty();
        }

        [Test]
        public void ExecuteUpdateSubmissionToInProgressWithRejectionSuccessfullyUpdatesSubmissionStateAndRejectionMessage()
        {
            const string rejectionReason = "rejected";
            var request = TestHelpers.UpdateCaseSubmissionRequest(submissionState: "in_progress", rejectionReason: rejectionReason);
            var createdSubmission = TestHelpers.CreateCaseSubmission(SubmissionState.Submitted);
            var worker = TestHelpers.CreateWorker();
            _mockDatabaseGateway.Setup(x => x.GetWorkerByEmail(request.EditedBy)).Returns(worker);
            _mockMongoGateway.Setup(x => x.LoadRecordById<CaseSubmission>(CollectionName, ObjectId.Parse(createdSubmission.SubmissionId.ToString()))).Returns(createdSubmission);

            var response = _formSubmissionsUseCase.ExecuteUpdateSubmission(createdSubmission.SubmissionId.ToString(), request);

            _mockMongoGateway.Verify(x => x.UpsertRecord(CollectionName, ObjectId.Parse(createdSubmission.SubmissionId.ToString()), createdSubmission), Times.Once);
            createdSubmission.SubmissionState.Should().Be(SubmissionState.InProgress);
            response.Should().BeEquivalentTo(createdSubmission.ToDomain().ToResponse());
            response.RejectionReason.Should().Be(rejectionReason);
        }

        [Test]
        public void ExecuteUpdateSubmissionToSubmittedSuccessfullyUpdatesSubmissionState()
        {
            var request = TestHelpers.UpdateCaseSubmissionRequest(submissionState: "submitted");
            var createdSubmission = TestHelpers.CreateCaseSubmission(SubmissionState.InProgress);
            var worker = TestHelpers.CreateWorker();
            _mockDatabaseGateway.Setup(x => x.GetWorkerByEmail(request.EditedBy)).Returns(worker);
            _mockMongoGateway.Setup(x => x.LoadRecordById<CaseSubmission>(CollectionName, ObjectId.Parse(createdSubmission.SubmissionId.ToString()))).Returns(createdSubmission);

            var response = _formSubmissionsUseCase.ExecuteUpdateSubmission(createdSubmission.SubmissionId.ToString(), request);

            _mockMongoGateway.Verify(x => x.UpsertRecord(CollectionName, ObjectId.Parse(createdSubmission.SubmissionId.ToString()), createdSubmission), Times.Once);
            createdSubmission.SubmissionState.Should().Be(SubmissionState.Submitted);
            response.Should().BeEquivalentTo(createdSubmission.ToDomain().ToResponse());
            response.SubmittedBy.Should().BeEquivalentTo(worker.ToDomain(false).ToResponse());
        }

        [Test]
        public void ExecuteUpdateSubmissionToApprovedSuccessfullyUpdatesSubmissionState()
        {
            var request = TestHelpers.UpdateCaseSubmissionRequest(submissionState: "approved");
            var createdSubmission = TestHelpers.CreateCaseSubmission(SubmissionState.Submitted);
            var worker = TestHelpers.CreateWorker();
            _mockDatabaseGateway.Setup(x => x.GetWorkerByEmail(request.EditedBy)).Returns(worker);
            _mockMongoGateway.Setup(x => x.LoadRecordById<CaseSubmission>(CollectionName, ObjectId.Parse(createdSubmission.SubmissionId.ToString()))).Returns(createdSubmission);

            var response = _formSubmissionsUseCase.ExecuteUpdateSubmission(createdSubmission.SubmissionId.ToString(), request);

            _mockMongoGateway.Verify(x => x.UpsertRecord(CollectionName, ObjectId.Parse(createdSubmission.SubmissionId.ToString()), createdSubmission), Times.Once);
            createdSubmission.SubmissionState.Should().Be(SubmissionState.Approved);
            response.Should().BeEquivalentTo(createdSubmission.ToDomain().ToResponse());
            response.ApprovedBy.Should().BeEquivalentTo(worker.ToDomain(false).ToResponse());
        }

        [Test]
        public void ExecuteUpdateSubmissionThrowsUpdateSubmissionExceptionIfInvalidSubmissionState()
        {
            var request = TestHelpers.UpdateCaseSubmissionRequest(submissionState: "invalid-state");
            var createdSubmission = TestHelpers.CreateCaseSubmission();
            var worker = TestHelpers.CreateWorker();
            _mockDatabaseGateway.Setup(x => x.GetWorkerByEmail(request.EditedBy)).Returns(worker);
            _mockMongoGateway.Setup(x => x.LoadRecordById<CaseSubmission>(CollectionName, ObjectId.Parse(createdSubmission.SubmissionId.ToString()))).Returns(createdSubmission);

            Action act = () => _formSubmissionsUseCase.ExecuteUpdateSubmission(createdSubmission.SubmissionId.ToString(), request);

            act.Should().Throw<UpdateSubmissionException>()
                .WithMessage($"Invalid submission state supplied {request.SubmissionState}");
        }

        [Test]
        public void ExecuteUpdateSubmissionDoesNotChangeSubmissionStateWhenNullStateProvided()
        {
            var request = TestHelpers.UpdateCaseSubmissionRequest(submissionState: null);
            var createdSubmission = TestHelpers.CreateCaseSubmission(SubmissionState.InProgress);
            var worker = TestHelpers.CreateWorker();
            _mockDatabaseGateway.Setup(x => x.GetWorkerByEmail(request.EditedBy)).Returns(worker);
            _mockMongoGateway.Setup(x => x.LoadRecordById<CaseSubmission>(CollectionName, ObjectId.Parse(createdSubmission.SubmissionId.ToString()))).Returns(createdSubmission);

            _formSubmissionsUseCase.ExecuteUpdateSubmission(createdSubmission.SubmissionId.ToString(), request);

            _mockMongoGateway.Verify(x => x.UpsertRecord(CollectionName, ObjectId.Parse(createdSubmission.SubmissionId.ToString()), createdSubmission), Times.Once);
            createdSubmission.SubmissionState.Should().Be(SubmissionState.InProgress);
        }

        [Test]
        public void ExecuteUpdateSubmissionSetsTheWorkersTeamsAndAllocationsToNull()
        {
            var request = TestHelpers.UpdateCaseSubmissionRequest();
            var createdSubmission = TestHelpers.CreateCaseSubmission(SubmissionState.InProgress);
            var worker = TestHelpers.CreateWorker();
            _mockDatabaseGateway.Setup(x => x.GetWorkerByEmail(request.EditedBy)).Returns(worker);
            _mockMongoGateway.Setup(x => x.LoadRecordById<CaseSubmission>(CollectionName, ObjectId.Parse(createdSubmission.SubmissionId.ToString()))).Returns(createdSubmission);

            _formSubmissionsUseCase.ExecuteUpdateSubmission(createdSubmission.SubmissionId.ToString(), request);

            worker.WorkerTeams.Should().BeNull();
            worker.Allocations.Should().BeNull();
        }

        [Test]
        public void ExecuteUpdateSubmissionSuccessfullyUpdatesARecordsResidents()
        {
            var resident = TestHelpers.CreatePerson();
            var request = TestHelpers.UpdateCaseSubmissionRequest(residents: new List<long> { resident.Id });
            var createdSubmission = TestHelpers.CreateCaseSubmission(SubmissionState.InProgress);
            var worker = TestHelpers.CreateWorker();
            _mockDatabaseGateway.Setup(x => x.GetPersonDetailsById(resident.Id)).Returns(resident);
            _mockDatabaseGateway.Setup(x => x.GetWorkerByEmail(request.EditedBy)).Returns(worker);
            _mockMongoGateway.Setup(x => x.LoadRecordById<CaseSubmission>(CollectionName, ObjectId.Parse(createdSubmission.SubmissionId.ToString()))).Returns(createdSubmission);

            var response = _formSubmissionsUseCase.ExecuteUpdateSubmission(createdSubmission.SubmissionId.ToString(), request);

            _mockMongoGateway.Verify(x => x.UpsertRecord(CollectionName, ObjectId.Parse(createdSubmission.SubmissionId.ToString()), createdSubmission), Times.Once);
            createdSubmission.Residents.Should().BeEquivalentTo(new List<SocialCareCaseViewerApi.V1.Infrastructure.Person> { resident });
            response.Should().BeEquivalentTo(createdSubmission.ToDomain().ToResponse());
        }

        [Test]
        public void ExecuteUpdateSubmissionRemovesAnyDuplicateResidents()
        {
            var resident = TestHelpers.CreatePerson();
            var request = TestHelpers.UpdateCaseSubmissionRequest(residents: new List<long> { resident.Id, resident.Id });
            var createdSubmission = TestHelpers.CreateCaseSubmission(SubmissionState.InProgress);
            var worker = TestHelpers.CreateWorker();
            _mockDatabaseGateway.Setup(x => x.GetPersonDetailsById(resident.Id)).Returns(resident);
            _mockDatabaseGateway.Setup(x => x.GetWorkerByEmail(request.EditedBy)).Returns(worker);
            _mockMongoGateway.Setup(x => x.LoadRecordById<CaseSubmission>(CollectionName, ObjectId.Parse(createdSubmission.SubmissionId.ToString()))).Returns(createdSubmission);

            var response = _formSubmissionsUseCase.ExecuteUpdateSubmission(createdSubmission.SubmissionId.ToString(), request);

            _mockMongoGateway.Verify(x => x.UpsertRecord(CollectionName, ObjectId.Parse(createdSubmission.SubmissionId.ToString()), createdSubmission), Times.Once);
            createdSubmission.Residents.Should().BeEquivalentTo(new List<SocialCareCaseViewerApi.V1.Infrastructure.Person> { resident });
            response.Should().BeEquivalentTo(createdSubmission.ToDomain().ToResponse());
        }

        [Test]
        public void ExecuteUpdateSubmissionThrowsUpdateSubmissionExceptionIfEmptyListOfResidentsProvided()
        {
            var request = TestHelpers.UpdateCaseSubmissionRequest(residents: new List<long>());
            var createdSubmission = TestHelpers.CreateCaseSubmission();
            var worker = TestHelpers.CreateWorker();
            _mockDatabaseGateway.Setup(x => x.GetWorkerByEmail(request.EditedBy)).Returns(worker);
            _mockMongoGateway.Setup(x => x.LoadRecordById<CaseSubmission>(CollectionName, ObjectId.Parse(createdSubmission.SubmissionId.ToString()))).Returns(createdSubmission);

            Action act = () => _formSubmissionsUseCase.ExecuteUpdateSubmission(createdSubmission.SubmissionId.ToString(), request);

            act.Should().Throw<UpdateSubmissionException>()
                .WithMessage("A submission must be against at least one resident");
        }

        [TestCaseSource(nameof(_invalidSubmissionStateForUpdates))]
        public void UpdatingResidentsThrowsUpdateSubmissionExceptionWhenSubmissionIsNotInProgress(SubmissionState submissionState)
        {
            var resident = TestHelpers.CreatePerson();
            var request = TestHelpers.UpdateCaseSubmissionRequest(residents: new List<long> { resident.Id });
            var createdSubmission = TestHelpers.CreateCaseSubmission(submissionState);
            var worker = TestHelpers.CreateWorker();
            _mockDatabaseGateway.Setup(x => x.GetPersonByMosaicId(resident.Id)).Returns(resident);
            _mockDatabaseGateway.Setup(x => x.GetWorkerByEmail(request.EditedBy)).Returns(worker);
            _mockMongoGateway
                .Setup(x => x.LoadRecordById<CaseSubmission>(CollectionName, ObjectId.Parse(createdSubmission.SubmissionId.ToString())))
                .Returns(createdSubmission);

            Action act = () => _formSubmissionsUseCase.ExecuteUpdateSubmission(createdSubmission.SubmissionId.ToString(), request);

            act.Should().Throw<UpdateSubmissionException>().WithMessage("Cannot update residents for submission, submission state not 'in progress'");
        }

        [Test]
        public void ExecuteUpdateSubmissionDoesNotChangeResidentsIfNoListIsPassed()
        {
            var resident = TestHelpers.CreatePerson();
            var request = TestHelpers.UpdateCaseSubmissionRequest();
            var createdSubmission = TestHelpers.CreateCaseSubmission(resident: resident);
            var worker = TestHelpers.CreateWorker();
            _mockDatabaseGateway.Setup(x => x.GetWorkerByEmail(request.EditedBy)).Returns(worker);
            _mockMongoGateway.Setup(x => x.LoadRecordById<CaseSubmission>(CollectionName, ObjectId.Parse(createdSubmission.SubmissionId.ToString()))).Returns(createdSubmission);

            _formSubmissionsUseCase.ExecuteUpdateSubmission(createdSubmission.SubmissionId.ToString(), request);

            _mockMongoGateway.Verify(x => x.UpsertRecord(CollectionName, ObjectId.Parse(createdSubmission.SubmissionId.ToString()), createdSubmission), Times.Once);
            createdSubmission.Residents.Should().BeEquivalentTo(new List<SocialCareCaseViewerApi.V1.Infrastructure.Person> { resident });
        }

        [Test]
        public void ExecuteUpdateSubmissionThrowsPersonNotFoundExceptionWhenInvalidResidentsFound()
        {
            var request = TestHelpers.UpdateCaseSubmissionRequest(residents: new List<long> { 0L });
            var createdSubmission = TestHelpers.CreateCaseSubmission(SubmissionState.InProgress);
            var worker = TestHelpers.CreateWorker();
            _mockDatabaseGateway.Setup(x => x.GetPersonByMosaicId(0L));
            _mockDatabaseGateway.Setup(x => x.GetWorkerByEmail(request.EditedBy)).Returns(worker);
            _mockMongoGateway.Setup(x => x.LoadRecordById<CaseSubmission>(CollectionName, ObjectId.Parse(createdSubmission.SubmissionId.ToString()))).Returns(createdSubmission);

            Action act = () => _formSubmissionsUseCase.ExecuteUpdateSubmission(createdSubmission.SubmissionId.ToString(), request);

            act.Should().Throw<PersonNotFoundException>()
                .WithMessage("Resident not found with ID 0");
        }

        private static object[] _allSubmissionChangePermutations =
        {
            new object[] { SubmissionState.Discarded, SubmissionState.Discarded, false },
            new object[] { SubmissionState.Discarded, SubmissionState.InProgress, false },
            new object[] { SubmissionState.Discarded, SubmissionState.Submitted, false },
            new object[] { SubmissionState.Discarded, SubmissionState.Approved, false },

            new object[] { SubmissionState.InProgress, SubmissionState.Discarded, true },
            new object[] { SubmissionState.InProgress, SubmissionState.InProgress, false },
            new object[] { SubmissionState.InProgress, SubmissionState.Submitted, true },
            new object[] { SubmissionState.InProgress, SubmissionState.Approved, false },

            new object[] { SubmissionState.Submitted, SubmissionState.Discarded, false },
            new object[] { SubmissionState.Submitted, SubmissionState.InProgress, true },
            new object[] { SubmissionState.Submitted, SubmissionState.Submitted, false },
            new object[] { SubmissionState.Submitted, SubmissionState.Approved, true },

            new object[] { SubmissionState.Approved, SubmissionState.Discarded, false },
            new object[] { SubmissionState.Approved, SubmissionState.InProgress, false },
            new object[] { SubmissionState.Approved, SubmissionState.Submitted, false },
            new object[] { SubmissionState.Approved, SubmissionState.Approved, false },
        };

        [TestCaseSource(nameof(_allSubmissionChangePermutations))]
        public void ExecuteUpdateSubmissionOnlyAllowsValidSubmissionStateChanges(SubmissionState startingState, SubmissionState desiredStated, bool allowed)
        {
            var mapSubmissionStateToRequestString = new Dictionary<SubmissionState, string> {
                { SubmissionState.InProgress, "in_progress" },
                { SubmissionState.Submitted, "Submitted" },
                { SubmissionState.Approved, "Approved" },
                { SubmissionState.Discarded, "Discarded" }
            };

            var mapSubmissionStateToResponseString = new Dictionary<SubmissionState, string> {
                { SubmissionState.InProgress, "In progress" },
                { SubmissionState.Submitted, "Submitted" },
                { SubmissionState.Approved, "Approved" },
                { SubmissionState.Discarded, "Discarded" }
            };

            var resident = TestHelpers.CreatePerson();
            var worker = TestHelpers.CreateWorker();
            var createdSubmission = TestHelpers.CreateCaseSubmission(startingState);
            var request = TestHelpers.UpdateCaseSubmissionRequest(submissionState: mapSubmissionStateToRequestString[desiredStated]);
            _mockDatabaseGateway.Setup(x => x.GetPersonByMosaicId(resident.Id)).Returns(resident);
            _mockDatabaseGateway.Setup(x => x.GetWorkerByEmail(request.EditedBy)).Returns(worker);
            _mockMongoGateway.Setup(x => x.LoadRecordById<CaseSubmission>(CollectionName, ObjectId.Parse(createdSubmission.SubmissionId.ToString()))).Returns(createdSubmission);

            if (allowed)
            {
                var response = _formSubmissionsUseCase.ExecuteUpdateSubmission(createdSubmission.SubmissionId.ToString(), request);

                _mockMongoGateway.Verify(x => x.UpsertRecord(CollectionName, ObjectId.Parse(createdSubmission.SubmissionId.ToString()), createdSubmission), Times.Once);
                response.Should().BeEquivalentTo(createdSubmission.ToDomain().ToResponse());
                response.SubmissionState.Should().Be(mapSubmissionStateToResponseString[desiredStated]);
            }
            else
            {
                Action act = () => _formSubmissionsUseCase.ExecuteUpdateSubmission(createdSubmission.SubmissionId.ToString(), request);

                act.Should().Throw<UpdateSubmissionException>()
                    .WithMessage($"Invalid submission state change from {mapSubmissionStateToResponseString[startingState]} to {mapSubmissionStateToResponseString[desiredStated]}");
            }
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
                .Setup(x => x.LoadRecordById<CaseSubmission>(CollectionName, ObjectId.Parse(createdSubmission.SubmissionId.ToString())))
                .Returns(createdSubmission);
            _mockMongoGateway.Setup(x =>
                x.UpsertRecord(It.IsAny<string>(), It.IsAny<ObjectId>(), It.IsAny<CaseSubmission>()));

            var response = _formSubmissionsUseCase.UpdateAnswers(createdSubmission.SubmissionId.ToString(), stepId, request);

            response.FormAnswers[stepId].Should().BeEquivalentTo(request.StepAnswers);
            response.EditHistory.LastOrDefault()?.Worker.Should().BeEquivalentTo(worker.ToDomain(false).ToResponse());
            _mockDatabaseGateway.Verify(x => x.GetWorkerByEmail(request.EditedBy), Times.Once);
            _mockMongoGateway.Verify(x => x.LoadRecordById<CaseSubmission>(CollectionName, createdSubmission.SubmissionId), Times.Once);
            _mockMongoGateway.Verify(x => x.UpsertRecord(CollectionName, createdSubmission.SubmissionId, It.IsAny<CaseSubmission>()), Times.Once);
        }

        [TestCaseSource(nameof(_invalidSubmissionStateForUpdates))]
        public void UpdateAnswersThrowsUpdateSubmissionExceptionWhenSubmissionIsNotInProgress(SubmissionState submissionState)
        {
            var request = TestHelpers.CreateUpdateFormSubmissionAnswersRequest();
            var createdSubmission = TestHelpers.CreateCaseSubmission(submissionState);
            var worker = TestHelpers.CreateWorker();
            const string stepId = "1";
            _mockDatabaseGateway.Setup(x => x.GetWorkerByEmail(request.EditedBy)).Returns(worker);
            _mockMongoGateway
                .Setup(x => x.LoadRecordById<CaseSubmission>(CollectionName, ObjectId.Parse(createdSubmission.SubmissionId.ToString())))
                .Returns(createdSubmission);

            Action act = () => _formSubmissionsUseCase.UpdateAnswers(createdSubmission.SubmissionId.ToString(), stepId, request);

            act.Should().Throw<UpdateSubmissionException>().WithMessage("Cannot update answers, submission state not 'in progress'");
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
                .Setup(x => x.LoadRecordById<CaseSubmission>(CollectionName, createdSubmission.SubmissionId))
                .Returns(createdSubmission);
            _mockMongoGateway.Setup(x =>
                x.UpsertRecord(It.IsAny<string>(), It.IsAny<ObjectId>(), It.IsAny<CaseSubmission>()));

            var response = _formSubmissionsUseCase.UpdateAnswers(createdSubmission.SubmissionId.ToString(), stepId, request);

            worker.WorkerTeams.Should().BeNull();
            worker.Allocations.Should().BeNull();
            response.Workers.FirstOrDefault()?.Teams.Should().BeEmpty();
            response.Workers.FirstOrDefault()?.AllocationCount.Should().Be(0);
        }

        [Test]
        public void ExecuteUpdateSubmissionThrowsWorkerNotFoundExceptionWhenNoWorkerFoundFromRequest()
        {
            var request = TestHelpers.UpdateCaseSubmissionRequest();
            var createdSubmission = TestHelpers.CreateCaseSubmission();
            _mockDatabaseGateway.Setup(x => x.GetWorkerByEmail(request.EditedBy));

            Action act = () => _formSubmissionsUseCase.ExecuteUpdateSubmission(createdSubmission.SubmissionId.ToString(), request);

            act.Should().Throw<WorkerNotFoundException>().WithMessage($"Worker with email {request.EditedBy} not found");
        }

        [Test]
        public void ExecuteUpdateSubmissionThrowsGetSubmissionExceptionWhenNoSubmissionFoundFromRequest()
        {
            var request = TestHelpers.UpdateCaseSubmissionRequest();
            var createdSubmission = TestHelpers.CreateCaseSubmission();
            var worker = TestHelpers.CreateWorker();
            _mockDatabaseGateway.Setup(x => x.GetWorkerByEmail(request.EditedBy)).Returns(worker);
            _mockMongoGateway.Setup(x => x.LoadRecordById<CaseSubmission>(CollectionName, createdSubmission.SubmissionId));

            Action act = () => _formSubmissionsUseCase.ExecuteUpdateSubmission(createdSubmission.SubmissionId.ToString(), request);

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
            _mockMongoGateway.Setup(x => x.LoadRecordById<CaseSubmission>(CollectionName, createdSubmission.SubmissionId));

            Action act = () => _formSubmissionsUseCase.UpdateAnswers(createdSubmission.SubmissionId.ToString(), stepId, request);

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

            Action act = () => _formSubmissionsUseCase.UpdateAnswers(createdSubmission.SubmissionId.ToString(), stepId, request);

            act.Should().Throw<WorkerNotFoundException>()
                .WithMessage($"Worker with email {request.EditedBy} not found");
        }

        [Test]
        public void ExecuteUpdateSubmissionUpdatesSubmissionsTagsIfTheRequestIncludesTags()
        {
            var tags = new List<string> { "tag one", "tag two" };

            var resident = TestHelpers.CreatePerson();
            var request = TestHelpers.UpdateCaseSubmissionRequest(tags: tags);
            var createdSubmission = TestHelpers.CreateCaseSubmission(SubmissionState.InProgress);
            var worker = TestHelpers.CreateWorker();

            _mockDatabaseGateway.Setup(x => x.GetPersonDetailsById(resident.Id)).Returns(resident);
            _mockDatabaseGateway.Setup(x => x.GetWorkerByEmail(request.EditedBy)).Returns(worker);
            _mockMongoGateway.Setup(x => x.LoadRecordById<CaseSubmission>(CollectionName, ObjectId.Parse(createdSubmission.SubmissionId.ToString()))).Returns(createdSubmission);

            var response = _formSubmissionsUseCase.ExecuteUpdateSubmission(createdSubmission.SubmissionId.ToString(), request);

            _mockMongoGateway.Verify(x => x.UpsertRecord(CollectionName, ObjectId.Parse(createdSubmission.SubmissionId.ToString()), createdSubmission), Times.Once);
            response.Should().BeEquivalentTo(createdSubmission.ToDomain().ToResponse());
            response.Tags.Should().BeEquivalentTo(tags);
        }

        [Test]
        public void ExecuteUpdateSubmissionToApprovedThrowsUpdateSubmissionExceptionIfApproverIsAlsoWorkerOnTheSubmission()
        {
            var resident = TestHelpers.CreatePerson();
            var worker = TestHelpers.CreateWorker();
            var request = TestHelpers.UpdateCaseSubmissionRequest(updatedBy: worker.Email, submissionState: "approved");
            var createdSubmission = TestHelpers.CreateCaseSubmission(worker: worker, submissionState: SubmissionState.InProgress);

            _mockDatabaseGateway.Setup(x => x.GetPersonDetailsById(resident.Id)).Returns(resident);
            _mockDatabaseGateway.Setup(x => x.GetWorkerByEmail(request.EditedBy)).Returns(worker);
            _mockMongoGateway.Setup(x => x.LoadRecordById<CaseSubmission>(CollectionName, ObjectId.Parse(createdSubmission.SubmissionId.ToString()))).Returns(createdSubmission);

            Action act = () => _formSubmissionsUseCase.ExecuteUpdateSubmission(createdSubmission.SubmissionId.ToString(), request);

            act.Should().Throw<UpdateSubmissionException>()
                .WithMessage($"Worker with email {request.EditedBy} cannot approve the submission as they have worked on the submission");
        }
    }
}

