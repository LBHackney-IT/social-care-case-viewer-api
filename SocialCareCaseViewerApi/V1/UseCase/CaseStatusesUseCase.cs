using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Boundary.Response;
using SocialCareCaseViewerApi.V1.Exceptions;
using SocialCareCaseViewerApi.V1.Factories;
using SocialCareCaseViewerApi.V1.Gateways.Interfaces;
using SocialCareCaseViewerApi.V1.UseCase.Interfaces;
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

        public List<CaseStatusResponse> ExecuteGet(long personId)
        {
            var person = _databaseGateway.GetPersonByMosaicId(personId);

            if (person == null)
            {
                throw new GetCaseStatusesException("Person not found");
            }

            var caseStatuses = _caseStatusGateway.GetActiveCaseStatusesByPersonId(personId);

            return caseStatuses.Select(caseStatus => caseStatus.ToResponse()).ToList();
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
            if (overlappingClosedCaseStatuses != null) throw new InvalidStartDateException("Invalid start date.");

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
            var worker = _databaseGateway.GetWorkerByEmail(request.CreatedBy);

            if (worker == null)
            {
                throw new WorkerNotFoundException($"Worker with email `{request.CreatedBy}` was not found");
            }

            var caseStatus = _caseStatusGateway.GetCasesStatusByCaseStatusId(request.CaseStatusId);

            if (caseStatus == null)
            {
                throw new CaseStatusDoesNotExistException($"Case status with {request.CaseStatusId} not found");
            }

            if (caseStatus.Type.ToLower() != "lac")
            {
                throw new InvalidCaseStatusTypeException("Answers can only be added to LAC statuses");
            }

            return _caseStatusGateway.CreateCaseStatusAnswer(request).ToResponse();
        }

        private void ExecuteUpdateValidation(UpdateCaseStatusRequest request, CaseStatus? caseStatus)
        {
            if (caseStatus == null)
            {
                throw new CaseStatusDoesNotExistException($"Case status with {request.CaseStatusId} not found");
            }

            if (request.EndDate != null && request.EndDate < caseStatus.StartDate)
            {
                throw new InvalidEndDateException($"requested end date of {request.EndDate?.ToString("O")} " +
                                                  $"is before the start date of {caseStatus.StartDate:O}");
            }

            if (caseStatus.Person.AgeContext.ToLower() != "c")
            {
                throw new InvalidAgeContextException(
                    $"Person with the id {caseStatus.Person.Id} belongs to the wrong AgeContext for this operation");
            }

            var worker = _databaseGateway.GetWorkerByEmail(request.EditedBy);
            if (worker == null)
            {
                throw new WorkerNotFoundException($"Worker with email `{request.EditedBy}` was not found");
            }
        }
    }
}
