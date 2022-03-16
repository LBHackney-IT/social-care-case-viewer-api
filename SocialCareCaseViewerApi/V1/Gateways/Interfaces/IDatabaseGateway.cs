using System.Collections.Generic;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Boundary.Response;
using SocialCareCaseViewerApi.V1.Infrastructure;
using Team = SocialCareCaseViewerApi.V1.Infrastructure.Team;
using Worker = SocialCareCaseViewerApi.V1.Infrastructure.Worker;

namespace SocialCareCaseViewerApi.V1.Gateways.Interfaces
{
    public interface IDatabaseGateway
    {
        (List<ResidentInformation>, int) GetResidentsBySearchCriteria(int cursor, int limit, long? id = null, string firstName = null,
         string lastName = null, string dateOfBirth = null, string postcode = null, string address = null, string contextFlag = null);

        AddNewResidentResponse AddNewResident(AddNewResidentRequest request);
        List<Allocation> SelectAllocations(long mosaicId, long workerId, string workerEmail, long teamId);
        CreateAllocationResponse CreateAllocation(CreateAllocationRequest request);
        string GetPersonIdByNCReference(string nfReference);
        string GetNCReferenceByPersonId(string personId);
        Worker GetWorkerByEmail(string email);
        Team GetTeamByTeamName(string name);
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
        List<long> GetPersonIdsByEmergencyId(string id);
        Person GetPersonWithPersonalRelationshipsByPersonId(long personId, bool includeEndedRelationships = false);
        PersonalRelationshipType GetPersonalRelationshipTypeByDescription(string description);
        PersonalRelationship CreatePersonalRelationship(CreatePersonalRelationshipRequest request);
        PersonalRelationship GetPersonalRelationshipById(long relationshipId);
        void DeleteRelationship(long relationshipId);
        void CreateRequestAudit(CreateRequestAuditRequest request);
    }
}
