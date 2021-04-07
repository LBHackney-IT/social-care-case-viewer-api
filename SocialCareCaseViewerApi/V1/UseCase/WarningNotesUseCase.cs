using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Boundary.Response;
using SocialCareCaseViewerApi.V1.Gateways;
using SocialCareCaseViewerApi.V1.UseCase.Interfaces;

namespace SocialCareCaseViewerApi.V1.UseCase
{
    public class WarningNotesUseCase : IWarningNotesUseCase
    {
        private IDatabaseGateway _databaseGateway;
        public WarningNotesUseCase(IDatabaseGateway databaseGateway)
        {
            _databaseGateway = databaseGateway;
        }

        public PostWarningNoteResponse ExecutePost(PostWarningNoteRequest request)
        {
            return _databaseGateway.PostWarningNote(request);
        }

    }
}
