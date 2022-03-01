using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Boundary.Response;

#nullable enable
namespace SocialCareCaseViewerApi.V1.UseCase.Interfaces
{
    public interface IResidentUseCase
    {
        AddNewResidentResponse AddNewResident(AddNewResidentRequest request);
        ResidentInformationList GetResidentsByQuery(ResidentQueryParam rqp, int cursor, int limit);
        GetPersonResponse? GetResident(GetPersonRequest request);
        void UpdateResident(UpdatePersonRequest request);
        CreateAllocationResponse AllocateResidentToTheTeam(AllocateResidentToTheTeamRequest request);
        ResidentInformationList GetUnallocatedList(int teamId, int cursor, int limit);
    }
}
