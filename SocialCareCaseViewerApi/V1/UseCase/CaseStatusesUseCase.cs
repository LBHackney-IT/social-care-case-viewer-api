using System.Collections.Generic;
using System.Linq;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Boundary.Response;
using SocialCareCaseViewerApi.V1.Exceptions;
using SocialCareCaseViewerApi.V1.Factories;
using SocialCareCaseViewerApi.V1.Gateways;
using SocialCareCaseViewerApi.V1.Gateways.Interfaces;
using SocialCareCaseViewerApi.V1.UseCase.Interfaces;
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

        public GetCaseStatusFieldsResponse ExecuteGetFields(GetCaseStatusFieldsRequest request)
        {
            var caseStatusType = _caseStatusGateway.GetCaseStatusTypeWithFields(request.Type);

            if (caseStatusType == null)
            {
                throw new CaseStatusNotFoundException();
            }

            return new GetCaseStatusFieldsResponse
            {
                Description = caseStatusType.Description,
                Name = caseStatusType.Name,
                Fields = caseStatusType.Fields.ToResponse()
            };
        }

        public List<CaseStatusResponse> ExecuteGet(long personId)
        {
            var person = _databaseGateway.GetPersonByMosaicId(personId);

            if (person == null)
            {
                throw new GetCaseStatusesException("Person not found");
            }

            var caseStatuses = _caseStatusGateway.GetCaseStatusesByPersonId(personId);

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

            var type = _caseStatusGateway.GetCaseStatusTypeWithFields(request.Type);
            var typeDoesNotExist = type == null;
            if (typeDoesNotExist) throw new CaseStatusTypeNotFoundException($"'type' with '{request.Type}' was not found.");

            var worker = _databaseGateway.GetWorkerByEmail(request.CreatedBy);
            var workerDoesNotExist = worker == null;
            if (workerDoesNotExist) throw new WorkerNotFoundException($"'createdBy' with '{request.CreatedBy}' was not found as a worker.");

            // check if case status exists for the period
            var personCaseStatus = _caseStatusGateway.GetCaseStatusesByPersonIdDate(request.PersonId, request.StartDate);

            var personCaseStatusAlreadyExists = personCaseStatus != null;
            if (personCaseStatusAlreadyExists) throw new CaseStatusAlreadyExistsException($"Case Status already exists for the period.");

            return _caseStatusGateway.CreateCaseStatus(request);
        }

        public CaseStatusResponse ExecuteUpdate(long caseStatusId, UpdateCaseStatusRequest request)
        {
            var caseStatus = _caseStatusGateway.GetCasesStatusByCaseStatusId(caseStatusId);

            ExecuteUpdateValidation(caseStatusId, request, caseStatus);

            var updatedCaseStatus = _caseStatusGateway.UpdateCaseStatus(caseStatusId, request);

            return updatedCaseStatus.ToResponse();
        }

        private void ExecuteUpdateValidation(long caseStatusId, UpdateCaseStatusRequest request, CaseStatus? caseStatus)
        {
            if (caseStatus == null)
            {
                throw new CaseStatusDoesNotExistException($"Case status with {caseStatusId} not found");
            }

            var person = _databaseGateway.GetPersonByMosaicId(request.PersonId);
            if (person == null)
            {
                throw new PersonNotFoundException($"'personId' with '{request.PersonId}' was not found");
            }
            if (person.AgeContext.ToLower() != "c")
            {
                throw new InvalidAgeContextException(
                    $"Person with the id {person.Id} belongs to the wrong AgeContext for this operation");
            }

            if (caseStatus.Resident.Id != request.PersonId)
            {
                throw new CaseStatusDoesNotMatchPersonException(
                    $"Retrieved case status does not match the provided person id of {request.PersonId}");
            }

            var worker = _databaseGateway.GetWorkerByEmail(request.EditedBy);
            if (worker == null)
            {
                throw new WorkerNotFoundException($"Worker with email `{request.EditedBy}` was not found");
            }
        }
    }
}
