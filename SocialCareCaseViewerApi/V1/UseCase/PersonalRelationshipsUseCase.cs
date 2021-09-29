using System.Collections.Generic;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Exceptions;
using SocialCareCaseViewerApi.V1.Gateways.Interfaces;
using SocialCareCaseViewerApi.V1.UseCase.Interfaces;

namespace SocialCareCaseViewerApi.V1.UseCase
{
    public class PersonalRelationshipsUseCase : IPersonalRelationshipsUseCase
    {
        private readonly IDatabaseGateway _databaseGateway;

        public PersonalRelationshipsUseCase(IDatabaseGateway databaseGateway)
        {
            _databaseGateway = databaseGateway;
        }

        public void ExecuteDelete(long id)
        {
            var relationship = _databaseGateway.GetPersonalRelationshipById(id);

            var relationshipDoesNotExist = relationship == null;
            if (relationshipDoesNotExist) throw new PersonalRelationshipNotFoundException($"'relationshipId' with '{id}' was not found.");

            _databaseGateway.DeleteRelationship(relationship.Id);
        }

        public void ExecutePost(CreatePersonalRelationshipRequest request)
        {
            var persons = _databaseGateway.GetPersonsByListOfIds(new List<long>() { request.PersonId, request.OtherPersonId });

            var personDoesNotExist = persons.Find(person => person.Id == request.PersonId) == null;
            if (personDoesNotExist) throw new PersonNotFoundException($"'personId' with '{request.PersonId}' was not found.");

            var otherPersonDoesNotExist = persons.Find(person => person.Id == request.OtherPersonId) == null;
            if (otherPersonDoesNotExist) throw new PersonNotFoundException($"'otherPersonId' with '{request.OtherPersonId}' was not found.");

            var type = _databaseGateway.GetPersonalRelationshipTypeByDescription(request.Type);

            var typeDoesNotExist = type == null;
            if (typeDoesNotExist) throw new PersonalRelationshipTypeNotFoundException($"'type' with '{request.Type}' was not found.");

            var worker = _databaseGateway.GetWorkerByEmail(request.CreatedBy);
            var workerDoesNotExist = worker == null;
            if (workerDoesNotExist) throw new WorkerNotFoundException($"'createdBy' with '{request.CreatedBy}' was not found as a worker.");

            var personWithPersonalRelationships = _databaseGateway.GetPersonWithPersonalRelationshipsByPersonId(request.PersonId);

            var personalRelationships = personWithPersonalRelationships.PersonalRelationships;
            var personalRelationshipAlreadyExists = personalRelationships.Find(pr => pr.OtherPersonId == request.OtherPersonId && pr.Type.Description == request.Type) != null;
            if (personalRelationshipAlreadyExists) throw new PersonalRelationshipAlreadyExistsException($"Personal relationship with 'type' of '{request.Type}' already exists.");

            request.TypeId = type.Id;

            _databaseGateway.CreatePersonalRelationship(request);

            _databaseGateway.CreatePersonalRelationship(new CreatePersonalRelationshipRequest()
            {
                PersonId = request.OtherPersonId,
                OtherPersonId = request.PersonId,
                TypeId = type.InverseTypeId,
                IsMainCarer = null,
                IsInformalCarer = null,
                Details = null,
                CreatedBy = request.CreatedBy
            });
        }
    }
}
