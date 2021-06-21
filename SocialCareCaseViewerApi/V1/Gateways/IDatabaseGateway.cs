using System.Collections.Generic;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Boundary.Response;
using SocialCareCaseViewerApi.V1.Infrastructure;
using ResidentInformation = SocialCareCaseViewerApi.V1.Domain.ResidentInformation;
using Team = SocialCareCaseViewerApi.V1.Infrastructure.Team;
using Worker = SocialCareCaseViewerApi.V1.Infrastructure.Worker;

namespace SocialCareCaseViewerApi.V1.Gateways
{
    public interface IDatabaseGateway
    {
        List<ResidentInformation> GetAllResidents(int cursor, int limit, string firstname = null,
            string lastname = null, string dateOfBirth = null, string mosaicid = null, string agegroup = null);

        AddNewResidentResponse AddNewResident(AddNewResidentRequest request);
        List<Allocation> SelectAllocations(long mosaicId, long workerId);
        CreateAllocationResponse CreateAllocation(CreateAllocationRequest request);
        string GetPersonIdByNCReference(string nfReference);
        string GetNCReferenceByPersonId(string personId);
        Worker GetWorkerByWorkerId(int workerId);
        Worker GetWorkerByEmail(string email);
        Team CreateTeam(CreateTeamRequest request);
        IEnumerable<Team> GetTeamsByTeamContextFlag(string context);
        Team GetTeamByTeamName(string name);
        Team GetTeamByTeamId(int id);
        List<dynamic> GetWorkerAllocations(List<Worker> workers);
        UpdateAllocationResponse UpdateAllocation(UpdateAllocationRequest request);
        PostWarningNoteResponse PostWarningNote(PostWarningNoteRequest request);
        Worker CreateWorker(CreateWorkerRequest createWorkerRequest);
        void UpdateWorker(UpdateWorkerRequest updateWorkerRequest);
        void PatchWarningNote(PatchWarningNoteRequest request);
        IEnumerable<WarningNote> GetWarningNotes(long personId);
        Domain.WarningNote GetWarningNoteById(long warningNoteId);
        Person GetPersonDetailsById(long id);
        void UpdatePerson(UpdatePersonRequest request);
        List<Person> GetPersonsByListOfIds(List<long> ids);
        Person GetPersonByMosaicId(long mosaicId);
        Person GetPersonWithPersonalRelationshipsByPersonId(long personId);
    }
}
