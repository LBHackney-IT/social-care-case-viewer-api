using SocialCareCaseViewerApi.V1.Boundary.Response;
using SocialCareCaseViewerApi.V1.Domain;

namespace SocialCareCaseViewerApi.V1.UseCase.Interfaces
{
    public interface ICaseNotesUseCase
    {
        ListCaseNotesResponse ExecuteGetByPersonId(string personId);

        CaseNoteResponse ExecuteGetById(string id);
    }
}
