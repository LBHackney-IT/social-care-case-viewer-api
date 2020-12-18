using System.Collections.Generic;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Boundary.Response;
using ResidentInformation = SocialCareCaseViewerApi.V1.Domain.ResidentInformation;

namespace SocialCareCaseViewerApi.V1.Gateways
{
    public interface IDatabaseGateway
    {
        List<ResidentInformation> GetAllResidents(int cursor, int limit, string firstname = null, string lastname = null, string dateOfBirth = null, string mosaicid = null, string agegroup = null);
        AddNewResidentResponse AddNewResident(AddNewResidentRequest request);
        List<CfsAllocation> SelectCfsAllocations(long? mosaicId, string officerEmail);
        List<AscAllocation> SelectAscAllocations(long? mosaicId, string officerEmail);
        string GetPersonIdByNCReference(string nfReference);
        string GetNCReferenceByPersonId(string personId);
    }
}
