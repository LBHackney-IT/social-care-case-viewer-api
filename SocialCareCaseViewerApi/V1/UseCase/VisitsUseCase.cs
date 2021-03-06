using System.Collections.Generic;
using System.Linq;
using SocialCareCaseViewerApi.V1.Boundary.Response;
using SocialCareCaseViewerApi.V1.Domain;
using SocialCareCaseViewerApi.V1.Gateways;
using SocialCareCaseViewerApi.V1.UseCase.Interfaces;

namespace SocialCareCaseViewerApi.V1.UseCase
{
    public class VisitsUseCase : IVisitsUseCase
    {
        private readonly ISocialCarePlatformAPIGateway _socialCarePlatformAPIGateway;

        public VisitsUseCase(ISocialCarePlatformAPIGateway socialCarePlatformAPIGateway)
        {
            _socialCarePlatformAPIGateway = socialCarePlatformAPIGateway;
        }

        public List<Visit> ExecuteGetByPersonId(string id)
        {
            return _socialCarePlatformAPIGateway.GetVisitsByPersonId(id).ToList();
        }
    }
}
