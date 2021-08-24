using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Bson;
using MongoDB.Driver;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Boundary.Response;
using SocialCareCaseViewerApi.V1.Exceptions;
using SocialCareCaseViewerApi.V1.Factories;
using SocialCareCaseViewerApi.V1.Gateways;
using SocialCareCaseViewerApi.V1.Helpers;
using SocialCareCaseViewerApi.V1.Infrastructure;
using SocialCareCaseViewerApi.V1.UseCase.Interfaces;

#nullable enable
namespace SocialCareCaseViewerApi.V1.UseCase
{
    public class FormSubmissionsUseCase : IFormSubmissionsUseCase
    {
        private readonly IDatabaseGateway _databaseGateway;
        private readonly IMongoGateway _mongoGateway;
        private static readonly string _collectionName = MongoConnectionStrings.Map[Collection.ResidentCaseSubmissions];

        private static readonly Dictionary<string, SubmissionState> _stringToSubmissionState = new Dictionary<string, SubmissionState> {
            { "in_progress", SubmissionState.InProgress },
            { "submitted", SubmissionState.Submitted },
            { "approved", SubmissionState.Approved },
            { "discarded", SubmissionState.Discarded },
            { "panel_approved", SubmissionState.PanelApproved }
        };

        public FormSubmissionsUseCase(IDatabaseGateway databaseGateway, IMongoGateway mongoGateway)
        {
            _databaseGateway = databaseGateway;
            _mongoGateway = mongoGateway;
        }

        public (CaseSubmissionResponse, CaseSubmission) ExecutePost(CreateCaseSubmissionRequest request)
        {
            var worker = GetSanitisedWorker(request.CreatedBy);
            var resident = GetSanitisedResident(request.ResidentId);

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

            _mongoGateway.InsertRecord(_collectionName, caseSubmission);

            return (caseSubmission.ToDomain().ToResponse(), caseSubmission);
        }

        public CaseSubmissionResponse? ExecuteGetById(string submissionId)
        {
            var foundSubmission = _mongoGateway.LoadRecordById<CaseSubmission>(_collectionName, ObjectId.Parse(submissionId));

            return foundSubmission?.ToDomain().ToResponse();
        }

        private static FilterDefinition<CaseSubmission> GenerateFilter(QueryCaseSubmissionsRequest request)
        {
            var builder = Builders<CaseSubmission>.Filter;
            var filter = builder.Empty;

            if (request.FormId != null)
            {
                filter &= Builders<CaseSubmission>.Filter.Eq(s => s.FormId, request.FormId);
            }

            if (request.SubmissionStates != null)
            {
                var requestSubmissionStates = new List<SubmissionState>();

                foreach (var submissionState in request.SubmissionStates)
                {
                    if (_stringToSubmissionState.ContainsKey(submissionState.ToLower()))
                    {
                        var state = _stringToSubmissionState[submissionState.ToLower()];
                        requestSubmissionStates.Add(state);
                    }
                }

                filter &= Builders<CaseSubmission>.Filter.In(s => s.SubmissionState, requestSubmissionStates);
            }

            if (request.CreatedAfter != null)
            {
                filter &= Builders<CaseSubmission>.Filter.Gte(s => s.CreatedAt, request.CreatedAfter);
            }

            if (request.CreatedBefore != null)
            {
                filter &= Builders<CaseSubmission>.Filter.Lte(s => s.CreatedAt, request.CreatedBefore);
            }

            if (request.WorkerEmail != null)
            {
                filter &= Builders<CaseSubmission>.Filter.ElemMatch(x => x.Workers, w => w.Email == request.WorkerEmail);
            }

            if (request.PersonID != null)
            {
                filter &= Builders<CaseSubmission>.Filter.ElemMatch(s => s.Residents , p => p.Id == request.PersonID);
            }            

            return filter;
        }

        private static bool CheckIfInvalidRequest(QueryCaseSubmissionsRequest request)
        {
            return request.FormId == null && request.SubmissionStates == null && request.CreatedAfter == null &&
                   request.CreatedBefore == null && request.WorkerEmail == null && request.PersonID == null;
        }

        public IEnumerable<CaseSubmissionResponse>? ExecuteGetByQuery(QueryCaseSubmissionsRequest request)
        {
            if (CheckIfInvalidRequest(request))
            {
                throw new QueryCaseSubmissionsException("Provide at minimum one query parameter");
            }

            var filter = GenerateFilter(request);
            var pagination = new Pagination { Page = request.Page, Size = request.Size };

            var foundSubmission = _mongoGateway.LoadRecordsByFilter(_collectionName, filter, pagination);

            return foundSubmission?.Select(s => s.ToDomain(request.IncludeFormAnswers, request.IncludeEditHistory).ToResponse());
        }

        public CaseSubmissionResponse ExecuteUpdateSubmission(string submissionId, UpdateCaseSubmissionRequest request)
        {
            var worker = GetSanitisedWorker(request.EditedBy);

            var updatedSubmission = _mongoGateway.LoadRecordById<CaseSubmission?>(_collectionName, ObjectId.Parse(submissionId));
            if (updatedSubmission == null)
            {
                throw new GetSubmissionException($"Submission with ID {submissionId} not found");
            }

            UpdateSubmissionState(updatedSubmission, request, worker);
            UpdateResidents(updatedSubmission, request);

            updatedSubmission.EditHistory.Add(new EditHistory<Worker> { Worker = worker, EditTime = DateTime.Now });

            _mongoGateway.UpsertRecord(_collectionName, ObjectId.Parse(submissionId), updatedSubmission);

            return updatedSubmission.ToDomain().ToResponse();
        }

        public CaseSubmissionResponse UpdateAnswers(string submissionId, string stepId, UpdateFormSubmissionAnswersRequest request)
        {
            var worker = GetSanitisedWorker(request.EditedBy);

            var submission = _mongoGateway.LoadRecordById<CaseSubmission>(_collectionName, ObjectId.Parse(submissionId));
            if (submission == null)
            {
                throw new GetSubmissionException($"Submission with ID {submissionId} not found");
            }
            if (submission.SubmissionState != SubmissionState.InProgress)
            {
                throw new UpdateSubmissionException("Cannot update answers, submission state not 'in progress'");
            }

            submission.FormAnswers[stepId] = request.StepAnswers;
            submission.EditHistory.Add(new EditHistory<Worker>
            {
                Worker = worker,
                EditTime = DateTime.Now
            });
            UpdateDateOfEvent(submission, request.DateOfEvent);
            UpdateTitle(submission, request.Title);
            _mongoGateway.UpsertRecord(_collectionName, ObjectId.Parse(submissionId), submission);
            return submission.ToDomain().ToResponse();
        }

        private static void UpdateSubmissionState(CaseSubmission submission, UpdateCaseSubmissionRequest request, Worker worker)
        {
            if (request.SubmissionState == null) return;

            var mapSubmissionStateToResponseString = new Dictionary<SubmissionState, string> {
                { SubmissionState.InProgress, "In progress" },
                { SubmissionState.Submitted, "Submitted" },
                { SubmissionState.Approved, "Approved" },
                { SubmissionState.Discarded, "Discarded" },
                { SubmissionState.PanelApproved, "Panel Approved" }
            };

            if (!_stringToSubmissionState.ContainsKey(request.SubmissionState.ToLower()))
            {
                throw new UpdateSubmissionException($"Invalid submission state supplied {request.SubmissionState}");
            }

            var newSubmissionState = _stringToSubmissionState[request.SubmissionState.ToLower()];

            // We should never hit the default but C# compiler complains if we don't provide a default case
            // https://stackoverflow.com/questions/1098644/switch-statement-without-default-when-dealing-with-enumerations
            switch (newSubmissionState)
            {
                case SubmissionState.Discarded:
                    if (submission.SubmissionState != SubmissionState.InProgress)
                    {
                        throw new UpdateSubmissionException($"Invalid submission state change from {mapSubmissionStateToResponseString[submission.SubmissionState]} to {mapSubmissionStateToResponseString[newSubmissionState]}");
                    }
                    break;
                case SubmissionState.InProgress:
                    if (submission.SubmissionState != SubmissionState.Submitted && submission.SubmissionState != SubmissionState.Approved)
                    {
                        throw new UpdateSubmissionException($"Invalid submission state change from {mapSubmissionStateToResponseString[submission.SubmissionState]} to {mapSubmissionStateToResponseString[newSubmissionState]}");
                    }
                    submission.RejectionReason = request.RejectionReason;
                    break;
                case SubmissionState.Submitted:
                    if (submission.SubmissionState != SubmissionState.InProgress)
                    {
                        throw new UpdateSubmissionException($"Invalid submission state change from {mapSubmissionStateToResponseString[submission.SubmissionState]} to {mapSubmissionStateToResponseString[newSubmissionState]}");
                    }
                    submission.SubmittedAt = DateTime.Now;
                    submission.SubmittedBy = worker;
                    break;
                case SubmissionState.Approved:
                    if (submission.SubmissionState != SubmissionState.Submitted)
                    {
                        throw new UpdateSubmissionException($"Invalid submission state change from {mapSubmissionStateToResponseString[submission.SubmissionState]} to {mapSubmissionStateToResponseString[newSubmissionState]}");
                    }
                    if (submission.CreatedBy.Email.Equals(request.EditedBy, StringComparison.OrdinalIgnoreCase))
                    {
                        throw new UpdateSubmissionException($"Worker with email {request.EditedBy} cannot approve the submission as they created the submission");
                    }
                    submission.ApprovedAt = DateTime.Now;
                    submission.ApprovedBy = worker;
                    break;
                case SubmissionState.PanelApproved:
                    if (submission.SubmissionState != SubmissionState.Approved)
                    {
                        throw new UpdateSubmissionException($"Invalid submission state change from {mapSubmissionStateToResponseString[submission.SubmissionState]} to {mapSubmissionStateToResponseString[newSubmissionState]}");
                    }
                    if (submission.CreatedBy.Email.Equals(request.EditedBy, StringComparison.OrdinalIgnoreCase))
                    {
                        throw new UpdateSubmissionException($"Worker with email {request.EditedBy} cannot panel approve the submission as they created the submission");
                    }
                    submission.PanelApprovedAt = DateTime.Now;
                    submission.PanelApprovedBy = worker;
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(request));
            }

            submission.SubmissionState = newSubmissionState;
        }

        private void UpdateResidents(CaseSubmission caseSubmission, UpdateCaseSubmissionRequest request)
        {
            if (request.Residents == null) return;

            if (request.Residents.Count == 0)
            {
                throw new UpdateSubmissionException("A submission must be against at least one resident");
            }

            if (caseSubmission.SubmissionState != SubmissionState.InProgress)
            {
                throw new UpdateSubmissionException("Cannot update residents for submission, submission state not 'in progress'");
            }

            var residentsWithoutDuplicates = new HashSet<long>(request.Residents);

            var newResident = residentsWithoutDuplicates.Select(GetSanitisedResident).ToList();

            caseSubmission.Residents = newResident;
        }

        private static void UpdateDateOfEvent(CaseSubmission caseSubmission, DateTime? dateOfEvent)
        {
            if (dateOfEvent == null) return;

            caseSubmission.DateOfEvent = dateOfEvent;
        }

        private static void UpdateTitle(CaseSubmission caseSubmission, string? title)
        {
            if (title == null) return;

            caseSubmission.Title = title;
        }

        private Worker GetSanitisedWorker(string workerEmail)
        {
            var worker = _databaseGateway.GetWorkerByEmail(workerEmail);
            if (worker == null)
            {
                throw new WorkerNotFoundException($"Worker with email {workerEmail} not found");
            }
            SanitiseWorker(worker);
            return worker;
        }

        private Person GetSanitisedResident(long residentId)
        {
            var resident = _databaseGateway.GetPersonDetailsById(residentId);
            if (resident == null)
            {
                throw new PersonNotFoundException($"Resident not found with ID {residentId}");
            }
            SanitiseResident(resident);
            return resident;
        }

        private static void SanitiseWorker(Worker workerToSanitise)
        {
            workerToSanitise.WorkerTeams = null;
            workerToSanitise.Allocations = null;
        }

        private static void SanitiseResident(Person residentToSanitise)
        {
            for (var index = 0; index < residentToSanitise.Addresses?.Count; index++)
            {
                var address = residentToSanitise.Addresses[index];
                address.Person = null;
            }

            for (var index = 0; index < residentToSanitise.PhoneNumbers?.Count; index++)
            {
                var number = residentToSanitise.PhoneNumbers[index];
                number.Person = null;
            }

            for (var index = 0; index < residentToSanitise.OtherNames?.Count; index++)
            {
                var name = residentToSanitise.OtherNames[index];
                name.Person = null;
            }

            for (var index = 0; index < residentToSanitise.Allocations?.Count; index++)
            {
                var allocation = residentToSanitise.Allocations[index];
                allocation.Person = null;
                allocation.Team = null;
            }
        }
    }
}
