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
    }
}
