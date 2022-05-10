using SocialCareCaseViewerApi.V1.Domain;
using SocialCareCaseViewerApi.V1.Gateways.Interfaces;
using SocialCareCaseViewerApi.V1.UseCase.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace SocialCareCaseViewerApi.V1.UseCase
{
    public class VisitsUseCase : IVisitsUseCase
    {
        private readonly IHistoricalDataGateway _historicalDataGateway;

        public VisitsUseCase(IHistoricalDataGateway historicalSocialCareGateway)
        {
            _historicalDataGateway = historicalSocialCareGateway;
        }

        public List<Visit> ExecuteGetByPersonId(long personId)
        {
            return _historicalDataGateway.GetVisitByPersonId(personId).ToList();
        }
    }
}
