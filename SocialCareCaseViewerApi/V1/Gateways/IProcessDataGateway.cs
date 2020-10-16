using SocialCareCaseViewerApi.V1.Boundary.Response;

namespace SocialCareCaseViewerApi.V1.Gateways
{
    public interface IProcessDataGateway
    {
        CareCaseData Task GetProcessData(string mosaicId, string officerEmail);
    }
}
