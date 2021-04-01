using System.Linq;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Boundary.Response;
using SocialCareCaseViewerApi.V1.Factories;
using SocialCareCaseViewerApi.V1.Gateways;
using SocialCareCaseViewerApi.V1.UseCase.Interfaces;

namespace SocialCareCaseViewerApi.V1.UseCase
{
    public class WarningNoteUseCase : IWarningNoteUseCase
    {
        private IDatabaseGateway _databaseGateway;
        public WarningNoteUseCase(IDatabaseGateway databaseGateway)
        {
            _databaseGateway = databaseGateway;
        }

        public CreateWarningNoteResponse ExecutePost(CreateWarningNoteRequest request)
        {
            return _databaseGateway.CreateWarningNote(request);
        }

        public GetWarningNoteResponse ExecuteGet(GetWarningNoteRequest request)
        {
            var warningNotes = _databaseGateway.GetWarningNotes(request);

            var response = warningNotes.Select(x => x.ToDomain()).ToList();
            return new GetWarningNoteResponse() { WarningNotes = response };
        }
    }
}
