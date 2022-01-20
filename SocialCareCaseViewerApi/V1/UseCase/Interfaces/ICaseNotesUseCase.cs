using SocialCareCaseViewerApi.V1.Boundary.Response;

namespace SocialCareCaseViewerApi.V1.UseCase.Interfaces
{
    public interface ICaseNotesUseCase
    {
        ListCaseNotesResponse ExecuteGetByPersonId(long personId);

        CaseNoteResponse ExecuteGetById(string id);
    }
}
