using SocialCareCaseViewerApi.V1.Boundary.Response;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SocialCareCaseViewerApi.V1.Gateways
{
    public interface IProcessDataGateway
    {
        IEnumerable<CareCaseData> GetProcessData(string mosaicId, string officerEmail);
    }
}
