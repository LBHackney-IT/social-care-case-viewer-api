using SocialCareCaseViewerApi.V1.Boundary.Response;
using SocialCareCaseViewerApi.V1.Domain;

namespace SocialCareCaseViewerApi.V1.Gateways
{
    public interface ISocialCarePlatformAPIGateway
    {
        ListCaseNotesResponse GetCaseNotesByPersonId(string id);

        CaseNote GetCaseNoteById(string id);
    }
}
