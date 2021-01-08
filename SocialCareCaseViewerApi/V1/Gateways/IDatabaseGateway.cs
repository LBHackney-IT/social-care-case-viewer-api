using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Boundary.Response;
using System.Collections.Generic;
using ResidentInformation = SocialCareCaseViewerApi.V1.Domain.ResidentInformation;
using Worker = SocialCareCaseViewerApi.V1.Infrastructure.Worker;
using Team = SocialCareCaseViewerApi.V1.Infrastructure.Team;

namespace SocialCareCaseViewerApi.V1.Gateways
{
    public interface IDatabaseGateway
    {
        List<ResidentInformation> GetAllResidents(int cursor, int limit, string firstname = null, string lastname = null, string dateOfBirth = null, string mosaicid = null, string agegroup = null);
        AddNewResidentResponse AddNewResident(AddNewResidentRequest request);
        List<Allocation> SelectAllocations(string mosaicId);
        CreateAllocationRequest CreateAllocation(CreateAllocationRequest request);
        string GetPersonIdByNCReference(string nfReference);
        string GetNCReferenceByPersonId(string personId);
        List<Worker> GetWorkers(int teamId);
        List<Team> GetTeams(string context);
    }
}
