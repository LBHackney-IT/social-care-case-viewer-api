using SocialCareCaseViewerApi.V1.Boundary.Response;
using SocialCareCaseViewerApi.V1.Exceptions;
using SocialCareCaseViewerApi.V1.Gateways;
using SocialCareCaseViewerApi.V1.UseCase.Interfaces;

namespace SocialCareCaseViewerApi.V1.UseCase
{
    public class CaseStatusUseCase : ICaseStatusesUseCase
    {
        private readonly IDatabaseGateway _databaseGateway;
        private readonly ICaseStatusGateway _caseStatusGateway;

        public CaseStatusUseCase(IDatabaseGateway databaseGateway, ICaseStatusGateway caseStatusGateway)
        {
            _databaseGateway = databaseGateway;
            _caseStatusGateway = caseStatusGateway;
        }

        public ListCaseStatusesResponse ExecuteGet(long personId)
        {
            var person = _databaseGateway.GetPersonByMosaicId(personId) ??
                         throw new GetCaseStatusesException("Person not found");

            var caseStatus = _caseStatusGateway.GetCaseStatusesByPersonId(person.Id);

            return new ListCaseStatusesResponse { PersonId = personId, CaseStatuses = caseStatus };
        }
    }
}
