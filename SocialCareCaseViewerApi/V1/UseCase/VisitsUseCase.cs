using SocialCareCaseViewerApi.V1.Domain;
using SocialCareCaseViewerApi.V1.Gateways.Interfaces;
using SocialCareCaseViewerApi.V1.UseCase.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace SocialCareCaseViewerApi.V1.UseCase
{
    public class VisitsUseCase : IVisitsUseCase
    {
        private readonly IHistoricalSocialCareGateway _historicalSocialCareGateway;

        public VisitsUseCase(IHistoricalSocialCareGateway historicalSocialCareGateway)
        {
            _historicalSocialCareGateway = historicalSocialCareGateway;
        }

        public List<Visit> ExecuteGetByPersonId(long id)
        {
            return _historicalSocialCareGateway.GetVisitInformationByPersonId(id).ToList();
        }
    }
}
