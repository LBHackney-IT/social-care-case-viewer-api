using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Boundary.Response;
using System.Collections.Generic;
using ResidentInformation = SocialCareCaseViewerApi.V1.Domain.ResidentInformation;

namespace SocialCareCaseViewerApi.V1.Gateways
{
    public interface IDatabaseGateway
    {
        List<ResidentInformation> GetAllResidents(int cursor, int limit, string firstname = null, string lastname = null);
        AddNewResidentResponse AddNewResident(AddNewResidentRequest request);
    }
}
