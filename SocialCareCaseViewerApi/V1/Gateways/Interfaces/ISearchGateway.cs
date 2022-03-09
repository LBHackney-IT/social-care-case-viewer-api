using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Boundary.Response;
using System.Collections.Generic;

namespace SocialCareCaseViewerApi.V1.Gateways.Interfaces
{
    public interface ISearchGateway
    {
        public (List<ResidentInformation>, int, int?) GetPersonRecordsBySearchQuery(PersonSearchRequest query);
    }
}
