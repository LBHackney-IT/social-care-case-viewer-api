using System;
using System.Collections.Generic;
using MongoDB.Bson;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Boundary.Response;
using SocialCareCaseViewerApi.V1.Exceptions;
using SocialCareCaseViewerApi.V1.Factories;
using SocialCareCaseViewerApi.V1.Gateways;
using SocialCareCaseViewerApi.V1.Infrastructure;
using SocialCareCaseViewerApi.V1.UseCase.Interfaces;

#nullable enable
namespace SocialCareCaseViewerApi.V1.UseCase
{
    public class FormSubmissionsUseCase : IFormSubmissionsUseCase
    {
        private readonly IDatabaseGateway _databaseGateway;
        private readonly IMongoGateway _mongoGateway;
        private const string CollectionName = "resident-case-submissions";

        public FormSubmissionsUseCase(IDatabaseGateway databaseGateway, IMongoGateway mongoGateway)
        {
            _databaseGateway = databaseGateway;
            _mongoGateway = mongoGateway;
        }

        public (CaseSubmissionResponse, CaseSubmission) ExecutePost(CreateCaseSubmissionRequest request)
        {
            var worker = _databaseGateway.GetWorkerByEmail(request.CreatedBy);
            if (worker == null)
            {
                throw new WorkerNotFoundException($"Worker with email {request.CreatedBy} not found");
            }
            worker.WorkerTeams = null;
            worker.Allocations = null;

            var resident = _databaseGateway.GetPersonByMosaicId(request.ResidentId);
            if (resident == null)
            {
                throw new PersonNotFoundException($"Person with id {request.ResidentId} not found");
            }

            var dateTimeNow = DateTime.Now;

            var caseSubmission = new CaseSubmission
            {
                FormId = request.FormId,
                Residents = new List<Person> { resident },
                Workers = new List<Worker> { worker },
                CreatedAt = dateTimeNow,
                CreatedBy = worker,
                SubmissionState = SubmissionState.InProgress,
                EditHistory = new List<EditHistory<Worker>> { new EditHistory<Worker> { Worker = worker, EditTime = dateTimeNow } },
                FormAnswers = new Dictionary<string, string>()
            };

            _mongoGateway.InsertRecord(CollectionName, caseSubmission);

            return (caseSubmission.ToDomain().ToResponse(), caseSubmission);
        }

        public CaseSubmissionResponse? ExecuteGetById(string submissionId)
        {
            var foundSubmission = _mongoGateway.LoadRecordById<CaseSubmission>(CollectionName, ObjectId.Parse(submissionId));

            return foundSubmission?.ToDomain().ToResponse();
        }

        public void ExecuteFinishSubmission(string submissionId, FinishCaseSubmissionRequest request)
        {
            var worker = _databaseGateway.GetWorkerByEmail(request.CreatedBy);
            if (worker == null)
            {
                throw new WorkerNotFoundException($"Worker with email {request.CreatedBy} not found");
            }
            worker.WorkerTeams = null;
            worker.Allocations = null;

            var updateSubmission = _mongoGateway.LoadRecordById<CaseSubmission>(CollectionName, ObjectId.Parse(submissionId));
            if (updateSubmission == null)
            {
                throw new GetSubmissionException($"Submission with ID {submissionId} not found");
            }

            updateSubmission.SubmissionState = SubmissionState.Submitted;
            updateSubmission.EditHistory.Add(new EditHistory<Worker> { Worker = worker, EditTime = DateTime.Now });

            _mongoGateway.UpsertRecord(CollectionName, ObjectId.Parse(submissionId), updateSubmission);
        }


        public CaseSubmissionResponse UpdateAnswers(string submissionId, string stepId, UpdateFormSubmissionAnswersRequest request)
        {
            var worker = _databaseGateway.GetWorkerByEmail(request.EditedBy);
            if (worker == null)
            {
                throw new WorkerNotFoundException($"Worker with email {request.EditedBy} not found");
            }
            worker.WorkerTeams = null;
            worker.Allocations = null;

            var submission = _mongoGateway.LoadRecordById<CaseSubmission>(CollectionName, ObjectId.Parse(submissionId));
            if (submission == null)
            {
                throw new GetSubmissionException($"Submission with ID {submissionId} not found");
            }

            submission.FormAnswers[stepId] = request.StepAnswers;
            submission.EditHistory.Add(new EditHistory<Worker>
            {
                Worker = worker,
                EditTime = DateTime.Now
            });
            _mongoGateway.UpsertRecord(CollectionName, ObjectId.Parse(submissionId), submission);
            return submission.ToDomain().ToResponse();
        }


    }
}
