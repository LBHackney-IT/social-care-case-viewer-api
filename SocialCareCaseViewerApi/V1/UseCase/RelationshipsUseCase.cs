using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Boundary.Response;
using SocialCareCaseViewerApi.V1.Exceptions;
using SocialCareCaseViewerApi.V1.Factories;
using SocialCareCaseViewerApi.V1.Gateways;
using SocialCareCaseViewerApi.V1.Infrastructure;
using SocialCareCaseViewerApi.V1.UseCase.Interfaces;
using System.Collections.Generic;

namespace SocialCareCaseViewerApi.V1.UseCase
{
    public class RelationshipsUseCase : IRelationshipsUseCase
    {
        private ISocialCarePlatformAPIGateway _socialCarePlatformAPIGateway;
        private IDatabaseGateway _databaseGateway;

        public RelationshipsUseCase(ISocialCarePlatformAPIGateway socialCarePlatformAPIGateway, IDatabaseGateway databaseGateway)
        {
            _socialCarePlatformAPIGateway = socialCarePlatformAPIGateway;
            _databaseGateway = databaseGateway;
        }

        public ListRelationshipsV1Response ExecuteGet(ListRelationshipsV1Request request)
        {
            var person = _databaseGateway.GetPersonByMosaicId(request.PersonId);

            if (person == null)
                throw new GetRelationshipsException("Person not found");

            var relationships = _socialCarePlatformAPIGateway.GetRelationshipsByPersonId(request.PersonId);

            List<long> personIds = new List<long>();
            List<Person> personRecords = new List<Person>();

            if (relationships != null)
            {
                if (relationships.PersonalRelationships?.Children?.Count > 0) personIds.AddRange(relationships.PersonalRelationships.Children);
                if (relationships.PersonalRelationships?.Other?.Count > 0) personIds.AddRange(relationships.PersonalRelationships.Other);
                if (relationships.PersonalRelationships?.Parents?.Count > 0) personIds.AddRange(relationships.PersonalRelationships.Parents);
                if (relationships.PersonalRelationships?.Siblings?.Count > 0) personIds.AddRange(relationships.PersonalRelationships.Siblings);

                if (personIds.Count > 0)
                    personRecords = _databaseGateway.GetPersonsByListOfIds(personIds);
            }

            return ResponseFactory.ToResponse(personRecords, relationships, personIds, request.PersonId);
        }
    }
}
