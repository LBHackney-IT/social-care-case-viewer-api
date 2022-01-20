using SocialCareCaseViewerApi.V1.Domain;
using SocialCareCaseViewerApi.V1.Gateways.Interfaces;
using SocialCareCaseViewerApi.V1.UseCase.Interfaces;

#nullable enable
namespace SocialCareCaseViewerApi.V1.UseCase
{
    public class GetVisitByVisitIdUseCase : IGetVisitByVisitIdUseCase
    {
        private readonly IHistoricalDataGateway _historicalDataGateway;

        public GetVisitByVisitIdUseCase(IHistoricalDataGateway historicalSocialCareGateway)
        {
            _historicalDataGateway = historicalSocialCareGateway;
        }

        public Visit? Execute(long id)
        {
            return _historicalDataGateway.GetVisitById(id);
        }
    }
}
