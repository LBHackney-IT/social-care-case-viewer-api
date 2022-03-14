using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Boundary.Response;
using SocialCareCaseViewerApi.V1.Gateways.Interfaces;
using SocialCareCaseViewerApi.V1.UseCase.Interfaces;

#nullable enable
namespace SocialCareCaseViewerApi.V1.UseCase
{
    public class SearchUseCase : ISearchUseCase
    {
        private readonly ISearchGateway _searchGateway;

        public SearchUseCase(ISearchGateway searchGateway)
        {
            _searchGateway = searchGateway;
        }

        public ResidentInformationList GetResidentsByQuery(PersonSearchRequest query)
        {
            if (string.IsNullOrEmpty(query.DateOfBirth)
                && string.IsNullOrEmpty(query.Name)
                && string.IsNullOrEmpty(query.PersonId)
                && string.IsNullOrEmpty(query.Postcode))
            {
                return new ResidentInformationList();
            }

            var (results, totalCount, nextCursor) = _searchGateway.GetPersonRecordsBySearchQuery(query);

            return new ResidentInformationList()
            {
                Residents = results,
                TotalCount = totalCount,
                NextCursor = nextCursor.ToString()
            };
        }
    }
}
