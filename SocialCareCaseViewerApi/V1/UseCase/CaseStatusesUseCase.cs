using System.Collections.Generic;
using System.Linq;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Boundary.Response;
using SocialCareCaseViewerApi.V1.Infrastructure;
using SocialCareCaseViewerApi.V1.Exceptions;
using SocialCareCaseViewerApi.V1.Factories;
using SocialCareCaseViewerApi.V1.Gateways;
using SocialCareCaseViewerApi.V1.UseCase.Interfaces;
using CaseStatus = SocialCareCaseViewerApi.V1.Domain.CaseStatus;

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
                Fields = caseStatusType?.Fields.ToResponse()
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
    }
}
