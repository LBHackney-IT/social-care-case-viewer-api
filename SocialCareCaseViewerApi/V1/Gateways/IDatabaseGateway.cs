using System.Collections.Generic;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Boundary.Response;
using ResidentInformation = SocialCareCaseViewerApi.V1.Domain.ResidentInformation;
using Team = SocialCareCaseViewerApi.V1.Infrastructure.Team;
using Worker = SocialCareCaseViewerApi.V1.Infrastructure.Worker;

namespace SocialCareCaseViewerApi.V1.Gateways
{
    public interface IDatabaseGateway
    {
        List<ResidentInformation> GetAllResidents(int cursor, int limit, string firstname = null, string lastname = null, string dateOfBirth = null, string mosaicid = null, string agegroup = null);
        AddNewResidentResponse AddNewResident(AddNewResidentRequest request);
        List<Allocation> SelectAllocations(long mosaicId, long workerId);
        CreateAllocationResponse CreateAllocation(CreateAllocationRequest request);
        string GetPersonIdByNCReference(string nfReference);
        string GetNCReferenceByPersonId(string personId);
        List<Worker> GetWorkers(int teamId, int workerId);
        List<dynamic> GetWorkerAllocations(List<Worker> workers);
        List<Team> GetTeams(string context);
        UpdateAllocationResponse UpdateAllocation(UpdateAllocationRequest request);
    }
}
