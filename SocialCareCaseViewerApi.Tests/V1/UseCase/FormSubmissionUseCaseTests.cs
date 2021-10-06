using System;
using System.Collections.Generic;
using System.Linq;
using Bogus;
using FluentAssertions;
using MongoDB.Bson;
using MongoDB.Driver;
using Moq;
using NUnit.Framework;
using SocialCareCaseViewerApi.Tests.V1.Helpers;
using SocialCareCaseViewerApi.V1.Boundary.Response;
using SocialCareCaseViewerApi.V1.Exceptions;
using SocialCareCaseViewerApi.V1.Factories;
using SocialCareCaseViewerApi.V1.Gateways;
using SocialCareCaseViewerApi.V1.Gateways.Interfaces;
using SocialCareCaseViewerApi.V1.Helpers;
using SocialCareCaseViewerApi.V1.Infrastructure;
using SocialCareCaseViewerApi.V1.UseCase;
using Person = SocialCareCaseViewerApi.V1.Infrastructure.Person;

#nullable enable
namespace SocialCareCaseViewerApi.Tests.V1.UseCase
{
    [TestFixture]
    public class FormSubmissionUseCaseTests
    {
        private Mock<IDatabaseGateway> _mockDatabaseGateway = null!;
        private Mock<IMongoGateway> _mockMongoGateway = null!;
        private FormSubmissionsUseCase _formSubmissionsUseCase = null!;
        private readonly Faker _faker = new Faker();
        private const string CollectionName = "resident-case-submissions";

        private static object[] _invalidSubmissionStateForUpdates =
        {
            SubmissionState.Discarded,
            SubmissionState.Submitted,
            SubmissionState.Approved,
            SubmissionState.PanelApproved
        };

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
            var workers = new List<Worker> { TestHelpers.CreateWorker() };
            var residents = new List<Person> { TestHelpers.CreatePerson() };

            _mockDatabaseGateway.Setup(x => x.GetWorkerByEmail(request.CreatedBy)).Returns(workers[0]);
            _mockDatabaseGateway.Setup(x => x.GetPersonDetailsById(request.ResidentId)).Returns(residents[0]);
            _mockMongoGateway.Setup(x => x.InsertRecord(It.IsAny<string>(), It.IsAny<CaseSubmission>()));

            var (caseSubmissionResponse, caseSubmission) = _formSubmissionsUseCase.ExecutePost(request);
            var expectedResponse = TestHelpers.CreateCaseSubmission(SubmissionState.InProgress,
                caseSubmission.CreatedAt, workers, residents, null, caseSubmission.SubmissionId, request.FormId);
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
        public void ExecuteUpdateSubmissionToInProgressFromSubmittedWithRejectionSuccessfullyUpdatesSubmissionStateAndRejectionMessage()
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
        public void ExecuteUpdateSubmissionToInProgressFromApprovedWithRejectionSuccessfullyUpdatesSubmissionStateAndRejectionMessage()
        {
            const string rejectionReason = "rejected";
            var request = TestHelpers.UpdateCaseSubmissionRequest(submissionState: "in_progress", rejectionReason: rejectionReason);
            var createdSubmission = TestHelpers.CreateCaseSubmission(SubmissionState.Approved);
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
        public void ExecuteUpdateSubmissionToPanelApprovedSuccessfullyUpdatesSubmissionState()
        {
            var request = TestHelpers.UpdateCaseSubmissionRequest(submissionState: "panel_approved");
            var createdSubmission = TestHelpers.CreateCaseSubmission(SubmissionState.Approved);
            var worker = TestHelpers.CreateWorker();
            _mockDatabaseGateway.Setup(x => x.GetWorkerByEmail(request.EditedBy)).Returns(worker);
            _mockMongoGateway.Setup(x => x.LoadRecordById<CaseSubmission>(CollectionName, ObjectId.Parse(createdSubmission.SubmissionId.ToString()))).Returns(createdSubmission);

            var response = _formSubmissionsUseCase.ExecuteUpdateSubmission(createdSubmission.SubmissionId.ToString(), request);

            _mockMongoGateway.Verify(x => x.UpsertRecord(CollectionName, ObjectId.Parse(createdSubmission.SubmissionId.ToString()), createdSubmission), Times.Once);
            createdSubmission.SubmissionState.Should().Be(SubmissionState.PanelApproved);
            response.Should().BeEquivalentTo(createdSubmission.ToDomain().ToResponse());
            response.PanelApprovedBy.Should().BeEquivalentTo(worker.ToDomain(false).ToResponse());
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
            createdSubmission.Residents.Should().BeEquivalentTo(new List<Person> { resident });
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
            createdSubmission.Residents.Should().BeEquivalentTo(new List<Person> { resident });
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
            var residents = new List<Person> { TestHelpers.CreatePerson() };
            var request = TestHelpers.UpdateCaseSubmissionRequest();
            var createdSubmission = TestHelpers.CreateCaseSubmission(residents: residents);
            var worker = TestHelpers.CreateWorker();
            _mockDatabaseGateway.Setup(x => x.GetWorkerByEmail(request.EditedBy)).Returns(worker);
            _mockMongoGateway.Setup(x => x.LoadRecordById<CaseSubmission>(CollectionName, ObjectId.Parse(createdSubmission.SubmissionId.ToString()))).Returns(createdSubmission);

            _formSubmissionsUseCase.ExecuteUpdateSubmission(createdSubmission.SubmissionId.ToString(), request);

            _mockMongoGateway.Verify(x => x.UpsertRecord(CollectionName, ObjectId.Parse(createdSubmission.SubmissionId.ToString()), createdSubmission), Times.Once);
            createdSubmission.Residents.Should().BeEquivalentTo(residents);
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
            new object[] { SubmissionState.Discarded, SubmissionState.PanelApproved, false },

            new object[] { SubmissionState.InProgress, SubmissionState.Discarded, true },
            new object[] { SubmissionState.InProgress, SubmissionState.InProgress, false },
            new object[] { SubmissionState.InProgress, SubmissionState.Submitted, true },
            new object[] { SubmissionState.InProgress, SubmissionState.Approved, false },
            new object[] { SubmissionState.InProgress, SubmissionState.PanelApproved, false },

            new object[] { SubmissionState.Submitted, SubmissionState.Discarded, false },
            new object[] { SubmissionState.Submitted, SubmissionState.InProgress, true },
            new object[] { SubmissionState.Submitted, SubmissionState.Submitted, false },
            new object[] { SubmissionState.Submitted, SubmissionState.Approved, true },
            new object[] { SubmissionState.Submitted, SubmissionState.PanelApproved, false },

            new object[] { SubmissionState.Approved, SubmissionState.Discarded, false },
            new object[] { SubmissionState.Approved, SubmissionState.InProgress, true },
            new object[] { SubmissionState.Approved, SubmissionState.Submitted, false },
            new object[] { SubmissionState.Approved, SubmissionState.Approved, false },
            new object[] { SubmissionState.Approved, SubmissionState.PanelApproved, true },

            new object[] { SubmissionState.PanelApproved, SubmissionState.Discarded, false },
            new object[] { SubmissionState.PanelApproved, SubmissionState.InProgress, false },
            new object[] { SubmissionState.PanelApproved, SubmissionState.Submitted, false },
            new object[] { SubmissionState.PanelApproved, SubmissionState.Approved, false },
            new object[] { SubmissionState.PanelApproved, SubmissionState.PanelApproved, false },
        };

        [TestCaseSource(nameof(_allSubmissionChangePermutations))]
        public void ExecuteUpdateSubmissionOnlyAllowsValidSubmissionStateChanges(SubmissionState startingState, SubmissionState desiredStated, bool allowed)
        {
            var mapSubmissionStateToRequestString = new Dictionary<SubmissionState, string> {
                { SubmissionState.InProgress, "in_progress" },
                { SubmissionState.Submitted, "Submitted" },
                { SubmissionState.Approved, "Approved" },
                { SubmissionState.Discarded, "Discarded" },
                { SubmissionState.PanelApproved, "panel_approved" }
            };

            var mapSubmissionStateToResponseString = new Dictionary<SubmissionState, string> {
                { SubmissionState.InProgress, "In progress" },
                { SubmissionState.Submitted, "Submitted" },
                { SubmissionState.Approved, "Approved" },
                { SubmissionState.Discarded, "Discarded" },
                { SubmissionState.PanelApproved, "Panel Approved" }
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

            response.FormAnswers?[stepId].Should().BeEquivalentTo(request.StepAnswers);
            response.EditHistory?.LastOrDefault()?.Worker.Should().BeEquivalentTo(worker.ToDomain(false).ToResponse());
            _mockDatabaseGateway.Verify(x => x.GetWorkerByEmail(request.EditedBy), Times.Once);
            _mockMongoGateway.Verify(x => x.LoadRecordById<CaseSubmission>(CollectionName, createdSubmission.SubmissionId), Times.Once);
            _mockMongoGateway.Verify(x => x.UpsertRecord(CollectionName, createdSubmission.SubmissionId, It.IsAny<CaseSubmission>()), Times.Once);
        }

        [Test]
        public void UpdateAnswersSuccessfullyUpdatesDateOfEventWhenProvided()
        {
            var dateOfEvent = new DateTime(2021, 07, 29);
            var request = TestHelpers.CreateUpdateFormSubmissionAnswersRequest(dateOfEvent: dateOfEvent);
            var createdSubmission = TestHelpers.CreateCaseSubmission(SubmissionState.InProgress);
            var worker = TestHelpers.CreateWorker();
            _mockDatabaseGateway.Setup(x => x.GetWorkerByEmail(request.EditedBy)).Returns(worker);
            _mockMongoGateway
                .Setup(x => x.LoadRecordById<CaseSubmission>(CollectionName, ObjectId.Parse(createdSubmission.SubmissionId.ToString())))
                .Returns(createdSubmission);
            _mockMongoGateway.Setup(x =>
                x.UpsertRecord(It.IsAny<string>(), It.IsAny<ObjectId>(), It.IsAny<CaseSubmission>()));

            var response = _formSubmissionsUseCase.UpdateAnswers(createdSubmission.SubmissionId.ToString(), "", request);

            response.DateOfEvent.Should().Be(dateOfEvent.ToString("O"));
        }

        [Test]
        public void UpdateAnswersSuccessfullyUpdatesTitleFromNullWhenProvided()
        {
            var title = _faker.Random.String2(100);
            var request = TestHelpers.CreateUpdateFormSubmissionAnswersRequest(title: title);
            var createdSubmission = TestHelpers.CreateCaseSubmission(SubmissionState.InProgress, title: null);

            createdSubmission.Title.Should().BeNull();

            var worker = TestHelpers.CreateWorker();
            _mockDatabaseGateway.Setup(x => x.GetWorkerByEmail(request.EditedBy)).Returns(worker);
            _mockMongoGateway
                .Setup(x => x.LoadRecordById<CaseSubmission>(CollectionName, ObjectId.Parse(createdSubmission.SubmissionId.ToString())))
                .Returns(createdSubmission);
            _mockMongoGateway.Setup(x =>
                x.UpsertRecord(It.IsAny<string>(), It.IsAny<ObjectId>(), It.IsAny<CaseSubmission>()));

            var response = _formSubmissionsUseCase.UpdateAnswers(createdSubmission.SubmissionId.ToString(), "", request);

            response.Title.Should().Be(title);
        }

        [Test]
        public void UpdateAnswersSuccessfullyUpdatesTitleFromPreviousTitleWhenProvided()
        {
            var title1 = _faker.Random.String2(100);
            var title2 = _faker.Random.String2(100);
            var request = TestHelpers.CreateUpdateFormSubmissionAnswersRequest(title: title2);
            var createdSubmission = TestHelpers.CreateCaseSubmission(SubmissionState.InProgress, title: title1);

            createdSubmission.Title.Should().Be(title1);

            var worker = TestHelpers.CreateWorker();
            _mockDatabaseGateway.Setup(x => x.GetWorkerByEmail(request.EditedBy)).Returns(worker);
            _mockMongoGateway
                .Setup(x => x.LoadRecordById<CaseSubmission>(CollectionName, ObjectId.Parse(createdSubmission.SubmissionId.ToString())))
                .Returns(createdSubmission);
            _mockMongoGateway.Setup(x =>
                x.UpsertRecord(It.IsAny<string>(), It.IsAny<ObjectId>(), It.IsAny<CaseSubmission>()));

            var response = _formSubmissionsUseCase.UpdateAnswers(createdSubmission.SubmissionId.ToString(), "", request);

            response.Title.Should().Be(title2);
        }

        [Test]
        public void UpdateAnswersDoesNotUpdateTitleWhenNullValueProvided()
        {
            var title = _faker.Random.String2(100);
            var request = TestHelpers.CreateUpdateFormSubmissionAnswersRequest(title: null);
            var createdSubmission = TestHelpers.CreateCaseSubmission(SubmissionState.InProgress, title: title);

            createdSubmission.Title.Should().Be(title);

            var worker = TestHelpers.CreateWorker();
            _mockDatabaseGateway.Setup(x => x.GetWorkerByEmail(request.EditedBy)).Returns(worker);
            _mockMongoGateway
                .Setup(x => x.LoadRecordById<CaseSubmission>(CollectionName, ObjectId.Parse(createdSubmission.SubmissionId.ToString())))
                .Returns(createdSubmission);
            _mockMongoGateway.Setup(x =>
                x.UpsertRecord(It.IsAny<string>(), It.IsAny<ObjectId>(), It.IsAny<CaseSubmission>()));

            var response = _formSubmissionsUseCase.UpdateAnswers(createdSubmission.SubmissionId.ToString(), "", request);

            response.Title.Should().Be(title);
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
            var workers = new List<Worker> { TestHelpers.CreateWorker() };
            var createdSubmission = TestHelpers.CreateCaseSubmission(SubmissionState.InProgress, null, workers);
            const string stepId = "1";
            _mockDatabaseGateway.Setup(x => x.GetWorkerByEmail(request.EditedBy)).Returns(workers[0]);
            _mockMongoGateway
                .Setup(x => x.LoadRecordById<CaseSubmission>(CollectionName, createdSubmission.SubmissionId))
                .Returns(createdSubmission);
            _mockMongoGateway.Setup(x =>
                x.UpsertRecord(It.IsAny<string>(), It.IsAny<ObjectId>(), It.IsAny<CaseSubmission>()));

            var response = _formSubmissionsUseCase.UpdateAnswers(createdSubmission.SubmissionId.ToString(), stepId, request);

            workers[0].WorkerTeams.Should().BeNull();
            workers[0].Allocations.Should().BeNull();
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
        public void ExecuteUpdateSubmissionToApprovedThrowsUpdateSubmissionExceptionIfApproverIsAlsoWorkerOnTheSubmission()
        {
            var resident = TestHelpers.CreatePerson();
            var workers = new List<Worker>() { TestHelpers.CreateWorker() };
            var request = TestHelpers.UpdateCaseSubmissionRequest(updatedBy: workers[0].Email, submissionState: "approved");
            var createdSubmission = TestHelpers.CreateCaseSubmission(workers: workers, submissionState: SubmissionState.Submitted);

            _mockDatabaseGateway.Setup(x => x.GetPersonDetailsById(resident.Id)).Returns(resident);
            _mockDatabaseGateway.Setup(x => x.GetWorkerByEmail(request.EditedBy)).Returns(workers[0]);
            _mockMongoGateway.Setup(x => x.LoadRecordById<CaseSubmission>(CollectionName, ObjectId.Parse(createdSubmission.SubmissionId.ToString()))).Returns(createdSubmission);

            Action act = () => _formSubmissionsUseCase.ExecuteUpdateSubmission(createdSubmission.SubmissionId.ToString(), request);

            act.Should().Throw<UpdateSubmissionException>()
                .WithMessage($"Worker with email {request.EditedBy} cannot approve the submission as they created the submission");
        }

        [Test]
        public void ExecuteGetByQueryThrowsQueryCaseSubmissionsExceptionIfNoParametersFound()
        {
            var request = TestHelpers.CreateQueryCaseSubmissions();

            Action act = () => _formSubmissionsUseCase.ExecuteGetByQuery(request);

            act.Should().Throw<QueryCaseSubmissionsException>()
                .WithMessage("Provide at minimum one query parameter");
        }

        [Test]
        public void ExecuteGetByQueryOnlyGetsFormsWithQueriedFormId()
        {
            const string testFormId = "foo";
            var request = TestHelpers.CreateQueryCaseSubmissions(formId: testFormId);

            var builder = Builders<CaseSubmission>.Filter;
            var filter = builder.Empty;
            filter &= Builders<CaseSubmission>.Filter.Eq(s => s.FormId, testFormId);

            var expectedJsonFilter = filter.RenderToJson();
            var pagination = new Pagination { Page = request.Page, Size = request.Size };

            _mockMongoGateway.Setup(m =>
                m.LoadRecordsByFilter(It.IsAny<string>(), It.IsAny<FilterDefinition<CaseSubmission>>(), pagination));

            _formSubmissionsUseCase.ExecuteGetByQuery(request);

            _mockMongoGateway.Verify(x =>
                x.LoadRecordsByFilter(MongoConnectionStrings.Map[Collection.ResidentCaseSubmissions], It.Is<FilterDefinition<CaseSubmission>>(innerFilter => innerFilter.RenderToJson().Equals(expectedJsonFilter)), It.IsAny<Pagination>()), Times.Once);
        }

        [Test]
        public void ExecuteGetByQueryOnlyGetsFormsWithQueriedSubmissionState()
        {
            var submissionStates = new List<SubmissionState>() { SubmissionState.InProgress };
            var submissionStatesRequest = new List<string>() { "in_progress" };
            var request = TestHelpers.CreateQueryCaseSubmissions(submissionStates: submissionStatesRequest);

            var builder = Builders<CaseSubmission>.Filter;
            var filter = builder.Empty;
            filter &= Builders<CaseSubmission>.Filter.In(s => s.SubmissionState, submissionStates);

            var expectedJsonFilter = filter.RenderToJson();
            var pagination = new Pagination { Page = request.Page, Size = request.Size };

            _mockMongoGateway.Setup(m =>
                m.LoadRecordsByFilter(It.IsAny<string>(), It.IsAny<FilterDefinition<CaseSubmission>>(), pagination));

            _formSubmissionsUseCase.ExecuteGetByQuery(request);

            _mockMongoGateway.Verify(x =>
                x.LoadRecordsByFilter(MongoConnectionStrings.Map[Collection.ResidentCaseSubmissions], It.Is<FilterDefinition<CaseSubmission>>(innerFilter => innerFilter.RenderToJson().Equals(expectedJsonFilter)), It.IsAny<Pagination>()), Times.Once);
        }

        [Test]
        public void ExecuteGetByQueryOnlyGetsFormsAfterCreatedAtQueriedDate()
        {
            var createdAfterTime = DateTime.Now;
            var request = TestHelpers.CreateQueryCaseSubmissions(createdAfter: createdAfterTime);

            var builder = Builders<CaseSubmission>.Filter;
            var filter = builder.Empty;
            filter &= Builders<CaseSubmission>.Filter.Gte(s => s.CreatedAt, createdAfterTime);

            var expectedJsonFilter = filter.RenderToJson();
            var pagination = new Pagination { Page = request.Page, Size = request.Size };

            _mockMongoGateway.Setup(m =>
                m.LoadRecordsByFilter(It.IsAny<string>(), It.IsAny<FilterDefinition<CaseSubmission>>(), pagination));

            _formSubmissionsUseCase.ExecuteGetByQuery(request);

            _mockMongoGateway.Verify(x =>
                x.LoadRecordsByFilter(MongoConnectionStrings.Map[Collection.ResidentCaseSubmissions], It.Is<FilterDefinition<CaseSubmission>>(innerFilter => innerFilter.RenderToJson().Equals(expectedJsonFilter)), It.IsAny<Pagination>()), Times.Once);
        }

        [Test]
        public void ExecuteGetByQueryOnlyGetsFormsBeforeCreatedAtQueriedDate()
        {
            var createdBeforeTime = DateTime.Now;
            var request = TestHelpers.CreateQueryCaseSubmissions(createdBefore: createdBeforeTime);

            var builder = Builders<CaseSubmission>.Filter;
            var filter = builder.Empty;
            filter &= Builders<CaseSubmission>.Filter.Lte(s => s.CreatedAt, createdBeforeTime);

            var expectedJsonFilter = filter.RenderToJson();
            var pagination = new Pagination { Page = request.Page, Size = request.Size };

            _mockMongoGateway.Setup(m =>
                m.LoadRecordsByFilter(It.IsAny<string>(), It.IsAny<FilterDefinition<CaseSubmission>>(), pagination));

            _formSubmissionsUseCase.ExecuteGetByQuery(request);

            _mockMongoGateway.Verify(x =>
                x.LoadRecordsByFilter(MongoConnectionStrings.Map[Collection.ResidentCaseSubmissions], It.Is<FilterDefinition<CaseSubmission>>(innerFilter => innerFilter.RenderToJson().Equals(expectedJsonFilter)), It.IsAny<Pagination>()), Times.Once);
        }

        [Test]
        public void ExecuteGetByQueryOnlyGetsFormsWithQueriedWorkerEmail()
        {
            var request = TestHelpers.CreateQueryCaseSubmissions(workerEmail: "foo@hackney.gov.uk");

            var builder = Builders<CaseSubmission>.Filter;
            var filter = builder.Empty;
            filter &= Builders<CaseSubmission>.Filter.ElemMatch(x => x.Workers, w => w.Email == request.WorkerEmail);

            var expectedJsonFilter = filter.RenderToJson();
            var pagination = new Pagination { Page = request.Page, Size = request.Size };

            _mockMongoGateway.Setup(m =>
                m.LoadRecordsByFilter(It.IsAny<string>(), It.IsAny<FilterDefinition<CaseSubmission>>(), pagination));

            _formSubmissionsUseCase.ExecuteGetByQuery(request);

            _mockMongoGateway.Verify(x =>
                x.LoadRecordsByFilter(MongoConnectionStrings.Map[Collection.ResidentCaseSubmissions], It.Is<FilterDefinition<CaseSubmission>>(innerFilter => innerFilter.RenderToJson().Equals(expectedJsonFilter)), It.IsAny<Pagination>()), Times.Once);
        }

        [Test]
        public void ExecuteGetByQueryOnlyGetsFormsWithResidentsMatchingAgeContext()
        {
            var request = TestHelpers.CreateQueryCaseSubmissions(ageContext: "A");

            var builder = Builders<CaseSubmission>.Filter;
            var filter = builder.Empty;
            filter &= Builders<CaseSubmission>.Filter.ElemMatch(x => x.Residents,
                r => r.AgeContext.ToUpper() == (request.AgeContext != null ? request.AgeContext.ToUpper() : null));

            var expectedJsonFilter = filter.RenderToJson();
            var pagination = new Pagination { Page = request.Page, Size = request.Size };

            _mockMongoGateway.Setup(m =>
                m.LoadRecordsByFilter(It.IsAny<string>(), It.IsAny<FilterDefinition<CaseSubmission>>(), pagination));

            _formSubmissionsUseCase.ExecuteGetByQuery(request);

            _mockMongoGateway.Verify(x =>
                x.LoadRecordsByFilter(MongoConnectionStrings.Map[Collection.ResidentCaseSubmissions], It.Is<FilterDefinition<CaseSubmission>>(innerFilter => innerFilter.RenderToJson().Equals(expectedJsonFilter)), It.IsAny<Pagination>()), Times.Once);
        }

        [Test]
        public void ExecuteGetByQueryOnlyGetsFormsWithQueriedPersonId()
        {
            var request = TestHelpers.CreateQueryCaseSubmissions(personID: 3);

            var builder = Builders<CaseSubmission>.Filter;
            var filter = builder.Empty;
            filter &= Builders<CaseSubmission>.Filter.ElemMatch(x => x.Residents, p => p.Id == request.PersonID);

            var expectedJsonFilter = filter.RenderToJson();
            var pagination = new Pagination { Page = request.Page, Size = request.Size };

            _mockMongoGateway.Setup(m =>
                m.LoadRecordsByFilter(It.IsAny<string>(), It.IsAny<FilterDefinition<CaseSubmission>>(), pagination));

            _formSubmissionsUseCase.ExecuteGetByQuery(request);

            _mockMongoGateway.Verify(x =>
                x.LoadRecordsByFilter(MongoConnectionStrings.Map[Collection.ResidentCaseSubmissions], It.Is<FilterDefinition<CaseSubmission>>(innerFilter => innerFilter.RenderToJson().Equals(expectedJsonFilter)), It.IsAny<Pagination>()), Times.Once);
        }


        [Test]
        public void ExecuteGetByQueryGetsFormsUsingAllQueryOptions()
        {
            const string testFormId = "foo";
            var submissionStates = new List<SubmissionState>() { SubmissionState.InProgress };
            var submissionStatesRequest = new List<string>() { "in_progress" };
            var createdBefore = DateTime.Now.AddDays(1);
            var createdAfter = DateTime.Now.AddDays(-1);
            var request = TestHelpers.CreateQueryCaseSubmissions(testFormId, submissionStatesRequest, createdBefore, createdAfter);

            var builder = Builders<CaseSubmission>.Filter;
            var filter = builder.Empty;
            filter &= Builders<CaseSubmission>.Filter.Eq(s => s.FormId, testFormId);
            filter &= Builders<CaseSubmission>.Filter.In(s => s.SubmissionState, submissionStates);
            filter &= Builders<CaseSubmission>.Filter.Gte(s => s.CreatedAt, createdAfter);
            filter &= Builders<CaseSubmission>.Filter.Lte(s => s.CreatedAt, createdBefore);

            var expectedJsonFilter = filter.RenderToJson();
            var pagination = new Pagination { Page = request.Page, Size = request.Size };

            _mockMongoGateway.Setup(m =>
                m.LoadRecordsByFilter(It.IsAny<string>(), It.IsAny<FilterDefinition<CaseSubmission>>(), pagination));

            _formSubmissionsUseCase.ExecuteGetByQuery(request);

            _mockMongoGateway.Verify(x =>
                x.LoadRecordsByFilter(MongoConnectionStrings.Map[Collection.ResidentCaseSubmissions], It.Is<FilterDefinition<CaseSubmission>>(innerFilter => innerFilter.RenderToJson().Equals(expectedJsonFilter)), It.IsAny<Pagination>()), Times.Once);
        }

        [Test]
        public void ExecuteGetByQueryStripsEditHistoryIfIncludeEditHistoryIsFalse()
        {
            var request = TestHelpers.CreateQueryCaseSubmissions("foo", includeEditHistory: false, pruneUnfinished: false);
            var caseSubmissions = new List<CaseSubmission> { TestHelpers.CreateCaseSubmission() };

            caseSubmissions.First()?.EditHistory.Count.Should().Be(1);

            _mockMongoGateway.Setup(m =>
                    m.LoadRecordsByFilter(It.IsAny<string>(), It.IsAny<FilterDefinition<CaseSubmission>>(), It.IsAny<Pagination>()))
                .Returns((caseSubmissions, 1));

            var response = _formSubmissionsUseCase.ExecuteGetByQuery(request);

            response.Items.First()?.EditHistory.Should().BeNull();
        }

        [Test]
        public void ExecuteGetByQueryContainsEditHistoryIfIncludeEditHistoryIsTrue()
        {
            var request = TestHelpers.CreateQueryCaseSubmissions("foo", includeEditHistory: true, pruneUnfinished: false);
            var caseSubmissions = new List<CaseSubmission> { TestHelpers.CreateCaseSubmission() };

            caseSubmissions.First()?.EditHistory.Count.Should().Be(1);

            _mockMongoGateway.Setup(m =>
                    m.LoadRecordsByFilter(It.IsAny<string>(), It.IsAny<FilterDefinition<CaseSubmission>>(), It.IsAny<Pagination>()))
                .Returns((caseSubmissions, 1));

            var response = _formSubmissionsUseCase.ExecuteGetByQuery(request);

            response.Items.First()?.EditHistory?.Count.Should().Be(1);
        }

        [Test]
        public void ExecuteGetByQueryContainsMinimalResponseIfPruneUnfinishedIsTrue()
        {
            var request = TestHelpers.CreateQueryCaseSubmissions("foo", pruneUnfinished: true);
            var caseSubmissions = new List<CaseSubmission> { TestHelpers.CreateCaseSubmission() };

            _mockMongoGateway.Setup(m =>
                    m.LoadRecordsByFilter(It.IsAny<string>(), It.IsAny<FilterDefinition<CaseSubmission>>(), It.IsAny<Pagination>()))
                .Returns((caseSubmissions, 1));

            var response = _formSubmissionsUseCase.ExecuteGetByQuery(request);
            var responseItem = response.Items.First();

            responseItem.ApprovedAt.Should().BeNull();
            responseItem.ApprovedBy.Should().BeNull();
            responseItem.DateOfEvent.Should().BeNull();
            responseItem.EditHistory.Should().BeNull();
            responseItem.FormAnswers.Should().BeNull();
            responseItem.PanelApprovedAt.Should().BeNull();
            responseItem.PanelApprovedBy.Should().BeNull();
            responseItem.RejectionReason.Should().BeNull();
            responseItem.SubmittedAt.Should().BeNull();
            responseItem.SubmittedBy.Should().BeNull();

            responseItem.Residents.First().Should().BeEquivalentTo(new Person
            {
                Id = caseSubmissions.First().Residents.First().Id,
                FirstName = caseSubmissions.First().Residents.First().FirstName,
                LastName = caseSubmissions.First().Residents.First().LastName,
                AgeContext = caseSubmissions.First().Residents.First().AgeContext,
                Restricted = caseSubmissions.First().Residents.First().Restricted
            });

            responseItem.Workers.First().Email.Should().Be(caseSubmissions.First().Workers.First().Email);

            responseItem.CreatedAt.Should().Be(caseSubmissions.First().CreatedAt.ToString("O"));
        }

        [Test]
        public void ExecuteGetByQueryStripsFormAnswersIfIncludeFormAnswersIsFalse()
        {
            var request = TestHelpers.CreateQueryCaseSubmissions("foo", includeFormAnswers: false);
            var caseSubmissions = new List<CaseSubmission> { TestHelpers.CreateCaseSubmission() };

            caseSubmissions.First()?.FormAnswers.Count.Should().Be(1);

            _mockMongoGateway.Setup(m =>
                    m.LoadRecordsByFilter(It.IsAny<string>(), It.IsAny<FilterDefinition<CaseSubmission>>(), It.IsAny<Pagination>()))
                .Returns((caseSubmissions, 1));

            var response = _formSubmissionsUseCase.ExecuteGetByQuery(request);

            response.Items.First()?.FormAnswers.Should().BeNull();
        }

        [Test]
        public void ExecuteGetByQueryContainsFormAnswersIfIncludeFormAnswersIsTrue()
        {
            var request = TestHelpers.CreateQueryCaseSubmissions("foo", includeFormAnswers: true, pruneUnfinished: false);
            var caseSubmissions = new List<CaseSubmission> { TestHelpers.CreateCaseSubmission() };

            caseSubmissions.First()?.FormAnswers.Count.Should().Be(1);

            _mockMongoGateway.Setup(m =>
                    m.LoadRecordsByFilter(It.IsAny<string>(), It.IsAny<FilterDefinition<CaseSubmission>>(), It.IsAny<Pagination>()))
                .Returns((caseSubmissions, 1));

            var response = _formSubmissionsUseCase.ExecuteGetByQuery(request);

            response.Items.First()?.FormAnswers?.Count.Should().Be(1);
        }

        [Test]
        public void ExecuteGetByQueryReturnsAPaginatedListAndTheTotalCount()
        {
            var request = TestHelpers.CreateQueryCaseSubmissions("foo");
            var paginatedCaseSubmissions = new List<CaseSubmission> { TestHelpers.CreateCaseSubmission() };
            const long totalCount = 20;

            _mockMongoGateway.Setup(m =>
                    m.LoadRecordsByFilter(It.IsAny<string>(), It.IsAny<FilterDefinition<CaseSubmission>>(), It.IsAny<Pagination>()))
                .Returns((paginatedCaseSubmissions, totalCount));

            var response = _formSubmissionsUseCase.ExecuteGetByQuery(request);

            response.Should().BeEquivalentTo(new Paginated<CaseSubmissionResponse>
            {
                Items = paginatedCaseSubmissions.Select(x => x.ToDomain().ToResponse()).ToList(),
                Count = totalCount
            });
        }
    }
}

