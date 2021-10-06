using SocialCareCaseViewerApi.V1.Boundary.Response;
using SocialCareCaseViewerApi.V1.Exceptions;
using SocialCareCaseViewerApi.V1.Factories;
using SocialCareCaseViewerApi.V1.Gateways.Interfaces;
using SocialCareCaseViewerApi.V1.UseCase.Interfaces;

namespace SocialCareCaseViewerApi.V1.UseCase
{
    public class RelationshipsUseCase : IRelationshipsUseCase
    {
        private readonly IDatabaseGateway _databaseGateway;

        public RelationshipsUseCase(IDatabaseGateway databaseGateway)
        {
            _databaseGateway = databaseGateway;
        }

        public ListRelationshipsResponse ExecuteGet(long personId)
        {
            var person = _databaseGateway.GetPersonWithPersonalRelationshipsByPersonId(personId);

            if (person == null)
                throw new GetRelationshipsException("Person not found");

            var response = new ListRelationshipsResponse() { PersonId = personId };

            if (person.PersonalRelationships != null)
                response.PersonalRelationships = person.PersonalRelationships.ToResponse();

            return response;
        }
    }
}
