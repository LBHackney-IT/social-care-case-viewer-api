using SocialCareCaseViewerApi.V1.Domain;
using SocialCareCaseViewerApi.V1.Gateways.Interfaces;
using SocialCareCaseViewerApi.V1.UseCase.Interfaces;

#nullable enable
namespace SocialCareCaseViewerApi.V1.UseCase
{
    public class GetVisitByVisitIdUseCase : IGetVisitByVisitIdUseCase
    {
        private readonly IHistoricalSocialCareGateway _historicalSocialCareGateway;

        public GetVisitByVisitIdUseCase(IHistoricalSocialCareGateway historicalSocialCareGateway)
        {
            _historicalSocialCareGateway = historicalSocialCareGateway;
        }

        public Visit? Execute(long id)
        {
            return _historicalSocialCareGateway.GetVisitInformationByVisitId(id);
        }
    }
}
