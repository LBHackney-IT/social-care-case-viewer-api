using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Boundary.Response;

namespace SocialCareCaseViewerApi.V1.Gateways
{
    public interface IMosaicAPIGateway
    {
        ResidentInformationList GetResidents(ResidentQueryParam rqp, int cursor, int limit);
    }
}
