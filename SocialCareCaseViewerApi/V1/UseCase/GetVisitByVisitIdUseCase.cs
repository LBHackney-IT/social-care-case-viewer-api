using SocialCareCaseViewerApi.V1.Domain;
using SocialCareCaseViewerApi.V1.Gateways;
using SocialCareCaseViewerApi.V1.UseCase.Interfaces;

namespace SocialCareCaseViewerApi.V1.UseCase
{
    public class GetVisitByVisitIdUseCase : IGetVisitByVisitIdUseCase
    {
        private readonly ISocialCarePlatformAPIGateway _socialCarePlatformAPIGateway;

        public GetVisitByVisitIdUseCase(ISocialCarePlatformAPIGateway socialCarePlatformAPIGateway)
        {
            _socialCarePlatformAPIGateway = socialCarePlatformAPIGateway;
        }

        public Visit Execute(long id)
        {
            return _socialCarePlatformAPIGateway.GetVisitByVisitId(id);
        }
    }
}
