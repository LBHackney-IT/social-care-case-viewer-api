using SocialCareCaseViewerApi.V1.Boundary.Response;
using SocialCareCaseViewerApi.V1.Factories;
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

        public CaseNoteResponse ExecuteGetById(string id)
        {
            var caseNote = _socialCarePlatformAPIGateway.GetCaseNoteById(id);

            return ResponseFactory.ToResponse(caseNote);
        }
    }
}
