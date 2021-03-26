using SocialCareCaseViewerApi.V1.Boundary.Response;
using SocialCareCaseViewerApi.V1.Gateways;
using SocialCareCaseViewerApi.V1.UseCase.Interfaces;

namespace SocialCareCaseViewerApi.V1.UseCase
{
    public class VisitsUseCase : IVisitsUseCase
    {
        private ISocialCarePlatformAPIGateway _socialCarePlatformAPIGateway;

        public VisitsUseCase(ISocialCarePlatformAPIGateway socialCarePlatformAPIGateway)
        {
            _socialCarePlatformAPIGateway = socialCarePlatformAPIGateway;
        }

        public ListVisitsResponse ExecuteGetByPersonId(string id)
        {
            return _socialCarePlatformAPIGateway.GetVisitsByPersonId(id);
        }
    }
}
