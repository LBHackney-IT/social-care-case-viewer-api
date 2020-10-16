using System.Collections.Generic;
using ResidentInformation = SocialCareCaseViewerApi.V1.Domain.ResidentInformation;

namespace SocialCareCaseViewerApi.V1.Gateways
{
    public interface IDatabaseGateway
    {
        List<ResidentInformation> GetAllResidents(int cursor, int limit, string firstname = null, string lastname = null, string date_of_birth = null, string mosaicid = null, string agegroup = null);
    }
}
