using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Boundary.Response;
using SocialCareCaseViewerApi.V1.Exceptions;
using SocialCareCaseViewerApi.V1.Factories;
using SocialCareCaseViewerApi.V1.Gateways;
using SocialCareCaseViewerApi.V1.UseCase.Interfaces;

namespace SocialCareCaseViewerApi.V1.UseCase
{
    public class CaseStatusesUseCase : ICaseStatusesUseCase
    {
        private IDatabaseGateway _databaseGateway;

        public CaseStatusesUseCase(IDatabaseGateway databaseGateway)
        {
            _databaseGateway = databaseGateway;
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

        public void ExecutePost(CreateCaseStatusRequest request)
        {
            var persons = _databaseGateway.GetPersonByMosaicId(request.PersonId);
            if (persons == null) throw new PersonNotFoundException($"'personId' with '{request.PersonId}' was not found.");

            var type = _databaseGateway.GetCaseStatusTypeWithFields(request.Type);
            request.TypeId = type.Id;

            var typeDoesNotExist = type == null;
            if (typeDoesNotExist) throw new CaseStatusTypeNotFoundException($"'type' with '{request.Type}' was not found.");

            var worker = _databaseGateway.GetWorkerByEmail(request.CreatedBy);
            var workerDoesNotExist = worker == null;
            if (workerDoesNotExist) throw new WorkerNotFoundException($"'createdBy' with '{request.CreatedBy}' was not found as a worker.");

            // add check if case status exists for the period

            // var personCaseStatus = _databaseGateway.GetCaseStatusesByPersonId(request.PersonId);

            // var personalRelationships = personWithPersonalRelationships.PersonalRelationships;
            // var personCaseStatusAlreadyExists = personCaseStatus.Find(pr => pr.OtherPersonId == request.OtherPersonId && pr.Type.Description == request.Type) != null;
            // if (personalRelationshipAlreadyExists) throw new PersonalRelationshipAlreadyExistsException($"Personal relationship with 'type' of '{request.Type}' already exists.");

            _databaseGateway.CreateCaseStatus(request);

            // _databaseGateway.CreatePersonalRelationship(new CreatePersonalRelationshipRequest()
            // {
            //     PersonId = request.OtherPersonId,
            //     OtherPersonId = request.PersonId,
            //     TypeId = type.InverseTypeId,
            //     IsMainCarer = null,
            //     IsInformalCarer = null,
            //     Details = null,
            //     CreatedBy = request.CreatedBy
            // });
        }
    }
}
