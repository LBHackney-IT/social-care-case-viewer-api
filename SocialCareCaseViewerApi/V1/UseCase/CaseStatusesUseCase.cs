using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Boundary.Response;
using SocialCareCaseViewerApi.V1.Exceptions;
using SocialCareCaseViewerApi.V1.Factories;
using SocialCareCaseViewerApi.V1.Gateways.Interfaces;
using SocialCareCaseViewerApi.V1.UseCase.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using CaseStatus = SocialCareCaseViewerApi.V1.Domain.CaseStatus;

#nullable enable
namespace SocialCareCaseViewerApi.V1.UseCase
{
    public class CaseStatusesUseCase : ICaseStatusesUseCase
    {
        private readonly ICaseStatusGateway _caseStatusGateway;
        private readonly IDatabaseGateway _databaseGateway;

        public CaseStatusesUseCase(ICaseStatusGateway caseStatusGateway, IDatabaseGateway databaseGateway)
        {
            _caseStatusGateway = caseStatusGateway;
            _databaseGateway = databaseGateway;
        }

        public List<CaseStatusResponse> ExecuteGet(ListCaseStatusesRequest request)
        {
            var person = _databaseGateway.GetPersonByMosaicId(request.PersonId);

            if (person == null)
            {
                throw new GetCaseStatusesException("Person not found");
            }

            if (!request.IncludeClosedCases)
            {
                return _caseStatusGateway.GetActiveCaseStatusesByPersonId(request.PersonId).Select(caseStatus => caseStatus.ToResponse()).ToList();
            }

            return _caseStatusGateway.GetCaseStatusesByPersonId(request.PersonId).Select(caseStatus => caseStatus.ToResponse()).ToList();
        }

        public CaseStatus ExecutePost(CreateCaseStatusRequest request)
        {
            var person = _databaseGateway.GetPersonByMosaicId(request.PersonId);
            if (person == null) throw new PersonNotFoundException($"'personId' with '{request.PersonId}' was not found.");

            if (person.AgeContext.ToLower() != "c")
            {
                throw new InvalidAgeContextException(
                    $"Person with the id {person.Id} belongs to the wrong AgeContext for this operation");
            }

            var worker = _databaseGateway.GetWorkerByEmail(request.CreatedBy);
            var workerDoesNotExist = worker == null;
            if (workerDoesNotExist) throw new WorkerNotFoundException($"'createdBy' with '{request.CreatedBy}' was not found as a worker.");

            var personActiveCaseStatuses = _caseStatusGateway.GetActiveCaseStatusesByPersonId(request.PersonId);
            var personCaseStatusAlreadyExists = personActiveCaseStatuses?.Count > 0;
            if (personCaseStatusAlreadyExists) throw new CaseStatusAlreadyExistsException("Active case status already exists for this person.");

            var overlappingClosedCaseStatuses = _caseStatusGateway.GetClosedCaseStatusesByPersonIdAndDate(request.PersonId, request.StartDate);
            if (overlappingClosedCaseStatuses?.Count > 0) throw new InvalidStartDateException("Invalid start date.");

            return _caseStatusGateway.CreateCaseStatus(request);
        }

        public CaseStatusResponse ExecuteUpdate(UpdateCaseStatusRequest request)
        {
            var caseStatus = _caseStatusGateway.GetCasesStatusByCaseStatusId(request.CaseStatusId);

            ExecuteUpdateValidation(request, caseStatus);

            var updatedCaseStatus = _caseStatusGateway.UpdateCaseStatus(request);

            return updatedCaseStatus.ToResponse();
        }

        public CaseStatusResponse ExecutePostCaseStatusAnswer(CreateCaseStatusAnswerRequest request)
        {
            var caseStatus = _caseStatusGateway.GetCasesStatusByCaseStatusId(request.CaseStatusId);

            var caseStatusType = caseStatus?.Type.ToLower();

            if (caseStatus == null)
            {
                throw new CaseStatusDoesNotExistException($"Case status with {request.CaseStatusId} not found");
            }

            if (caseStatusType != "lac" && caseStatusType != "cp")
            {
                throw new InvalidCaseStatusTypeException("Answers can only be added to LAC or CP statuses");
            }

            if (caseStatusType == "cp" && request.Answers?.Count != 1)
            {
                throw new InvalidCaseStatusAnswersRequestException("CP must have only one answer");
            }

            if (caseStatusType == "lac" && request.Answers?.Count != 2)
            {
                throw new InvalidCaseStatusAnswersRequestException("LAC must have only two answers");
            }

            var worker = _databaseGateway.GetWorkerByEmail(request.CreatedBy);

            if (worker == null)
            {
                throw new WorkerNotFoundException($"Worker with email `{request.CreatedBy}` was not found");
            }

            //check for overlapping dates
            var activeAnswers = caseStatus.Answers.Where(x => x.DiscardedAt == null && (x.EndDate == null || x.EndDate > DateTime.Today));

            switch (caseStatusType)
            {
                case "lac":
                    if (activeAnswers.Count() == 2 && activeAnswers.FirstOrDefault().StartDate >= request.StartDate)
                    {
                        throw new InvalidCaseStatusAnswersStartDateException($"Start date cannot be before the current active date for {caseStatusType.ToUpper()}");
                    }
                    break;
                case "cp":
                    if (activeAnswers.Count() == 1 && activeAnswers.FirstOrDefault().StartDate >= request.StartDate)
                    {
                        throw new InvalidCaseStatusAnswersStartDateException($"Start date cannot be before the current active date for {caseStatusType.ToUpper()}");
                    }
                    break;
            }

            return _caseStatusGateway.ReplaceCaseStatusAnswers(request).ToResponse();
        }

        private void ExecuteUpdateValidation(UpdateCaseStatusRequest request, CaseStatus? caseStatus)
        {
            if (caseStatus == null)
            {
                throw new CaseStatusDoesNotExistException($"Case status with {request.CaseStatusId} not found");
            }

            if (caseStatus.Person.AgeContext.ToLower() != "c")
            {
                throw new InvalidAgeContextException($"Person with the id {caseStatus.Person.Id} belongs to the wrong AgeContext for this operation");
            }

            var worker = _databaseGateway.GetWorkerByEmail(request.EditedBy);
            if (worker == null)
            {
                throw new WorkerNotFoundException($"Worker with email `{request.EditedBy}` was not found");
            }
            var caseStatusType = caseStatus.Type.ToLower();

            //end date validation
            if (request.EndDate != null)
            {
                switch (caseStatusType)
                {
                    case "cp":
                    case "lac":
                        var activeAnswerCountToMatch = caseStatusType == "cp" ? 1 : 2;

                        var activeAnswers = caseStatus.Answers.Where(x => x.DiscardedAt == null && (x.EndDate == null || x.EndDate > DateTime.Today));

                        if (activeAnswers.Count() == activeAnswerCountToMatch && activeAnswers.FirstOrDefault().StartDate > request.EndDate)
                        {
                            throw new InvalidEndDateException("requested end date is before the start date of the currently active answer");
                        }
                        if (request?.Answers?.Count != 1
                            || request.Answers.Any(x => string.IsNullOrWhiteSpace(x.Option))
                            || request.Answers.Any(x => string.IsNullOrWhiteSpace(x.Value)))
                        {
                            throw new InvalidCaseStatusUpdateRequestException($"Invalid {caseStatus.Type.ToUpper()} episode ending answer");
                        }
                        break;
                    case "cin":
                        if (request.EndDate < caseStatus.StartDate)
                        {
                            throw new InvalidEndDateException($"requested end date of {request.EndDate?.ToString("O")} " + $"is before the start date of {caseStatus.StartDate:O}");
                        }
                        break;
                }
            }

            //when end date is not provided
            if (request.EndDate == null)
            {
                if (request.StartDate > DateTime.Today)
                {
                    throw new InvalidStartDateException("Invalid start date. It cannot be in the future.");
                }

                switch (caseStatusType)
                {
                    case "cp":
                        if (request?.Answers?.Count != 1
                            || request.Answers.Any(x => string.IsNullOrWhiteSpace(x.Option))
                            || request.Answers.Any(x => string.IsNullOrWhiteSpace(x.Value)))
                        {
                            throw new InvalidCaseStatusUpdateRequestException("Invalid CP answer");
                        }
                        break;
                    case "lac":
                        if (request?.Answers?.Count != 2
                            || request.Answers.Any(x => string.IsNullOrWhiteSpace(x.Option))
                            || request.Answers.Any(x => string.IsNullOrWhiteSpace(x.Value)))
                        {
                            throw new InvalidCaseStatusUpdateRequestException("Invalid LAC answers");
                        }
                        break;
                }
            }
        }
    }
}
