using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Boundary.Response;
using SocialCareCaseViewerApi.V1.Infrastructure;
using SocialCareCaseViewerApi.V1.Exceptions;
using SocialCareCaseViewerApi.V1.Factories;
using SocialCareCaseViewerApi.V1.Gateways;
using SocialCareCaseViewerApi.V1.UseCase.Interfaces;

namespace SocialCareCaseViewerApi.V1.UseCase
{
    public class CaseStatusesUseCase : ICaseStatusesUseCase
    {
        private readonly IDatabaseGateway _databaseGateway;

        public CaseStatusesUseCase(IDatabaseGateway databaseGateway)
        {
            _databaseGateway = databaseGateway;
        }

        public GetCaseStatusFieldsResponse ExecuteGetFields(GetCaseStatusFieldsRequest request)
        {
            var caseStatusType = _databaseGateway.GetCaseStatusTypeWithFields(request.Type);

            if (caseStatusType == null)
            {
                throw new CaseStatusNotFoundException();
            }

            return new GetCaseStatusFieldsResponse
            {
                Description = caseStatusType.Description,
                Name = caseStatusType.Name,
                Fields = caseStatusType?.Fields.ToResponse()
            };
        }

        public ListCaseStatusesResponse ExecuteGet(long personId)
        {
            var person = _databaseGateway.GetPersonByMosaicId(personId);

            if (person == null)
            {
                throw new GetCaseStatusesException("Person not found");
            }

            var caseStatus = _databaseGateway.GetCaseStatusesByPersonId(personId);

            var response = new ListCaseStatusesResponse() { PersonId = personId, CaseStatuses = caseStatus.ToResponse() };

            return response;
        }

        public CaseStatus ExecutePost(CreateCaseStatusRequest request)
        {
            var person = _databaseGateway.GetPersonByMosaicId(request.PersonId);
            if (person == null) throw new PersonNotFoundException($"'personId' with '{request.PersonId}' was not found.");

            var type = _databaseGateway.GetCaseStatusTypeWithFields(request.Type);
            var typeDoesNotExist = type == null;
            if (typeDoesNotExist) throw new CaseStatusTypeNotFoundException($"'type' with '{request.Type}' was not found.");

            var worker = _databaseGateway.GetWorkerByEmail(request.CreatedBy);
            var workerDoesNotExist = worker == null;
            if (workerDoesNotExist) throw new WorkerNotFoundException($"'createdBy' with '{request.CreatedBy}' was not found as a worker.");

            // check if case status exists for the period
            var personCaseStatus = _databaseGateway.GetCaseStatusesByPersonIdDate(request.PersonId, request.StartDate);

            var personCaseStatusAlreadyExists = personCaseStatus != null;
            if (personCaseStatusAlreadyExists) throw new CaseStatusAlreadyExistsException($"Case Status already exists for the period.");

            return _databaseGateway.CreateCaseStatus(request);
        }

        public CaseStatus ExecuteUpdate(long caseStatusId, UpdateCaseStatusRequest request)
        {
            var person = _databaseGateway.GetPersonByMosaicId(caseStatusId);
            if (person == null)
            {
                throw new PersonNotFoundException($"'personId' with '{caseStatusId}' was not found.");
            }

            var worker = _databaseGateway.GetWorkerByEmail(request.EditedBy);
            if (worker == null)
            {
                throw new WorkerNotFoundException($"Worker with email `{request.EditedBy}` was not found");
            }

            var caseStatus = _databaseGateway.GetCasesStatusByCaseStatusId(caseStatusId);
            if (caseStatus == null)
            {
                throw new CaseStatusDoesNotExistException($"Case status with {caseStatusId} not found");
            }

            if (caseStatus.Person.Id != request.PersonId)
            {
                throw new CaseStatusDoesNotMatchPersonException(
                    $"Retrieved case status does not match the provided person id of {request.PersonId}");
            }

            // apply updates

            // save changes

            return new CaseStatus();
        }
    }
}
