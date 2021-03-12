using SocialCareCaseViewerApi.V1.Boundary.Response;
using SocialCareCaseViewerApi.V1.Domain;
using SocialCareCaseViewerApi.V1.Gateways;
using SocialCareCaseViewerApi.V1.UseCase.Interfaces;

namespace SocialCareCaseViewerApi.V1.UseCase
{
    public class CaseNotesUseCase : ICaseNotesUseCase
    {
        private ISocialCarePlatformAPIGateway _socialCarePlatformAPIGateway;

        public CaseNotesUseCase(ISocialCarePlatformAPIGateway socialCarePlatformAPIGateway)
        {
            _socialCarePlatformAPIGateway = socialCarePlatformAPIGateway;
        }

        public ListCaseNotesResponse ExecuteGetByPersonId(string id)
        {
            return _socialCarePlatformAPIGateway.GetCaseNotesByPersonId(id);
        }

        public CaseNote ExecuteGetById(string id)
        {
            return _socialCarePlatformAPIGateway.GetCaseNoteById(id);
        }
    }
}
