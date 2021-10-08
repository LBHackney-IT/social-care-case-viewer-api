using System.Linq;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Boundary.Response;
using SocialCareCaseViewerApi.V1.Factories;
using SocialCareCaseViewerApi.V1.Gateways.Interfaces;
using SocialCareCaseViewerApi.V1.UseCase.Interfaces;

namespace SocialCareCaseViewerApi.V1.UseCase
{
    public class WarningNoteUseCase : IWarningNoteUseCase
    {
        private readonly IDatabaseGateway _databaseGateway;
        public WarningNoteUseCase(IDatabaseGateway databaseGateway)
        {
            _databaseGateway = databaseGateway;
        }

        public PostWarningNoteResponse ExecutePost(PostWarningNoteRequest request)
        {
            return _databaseGateway.PostWarningNote(request);
        }

        public ListWarningNotesResponse ExecuteGet(long personId)
        {
            var warningNotes = _databaseGateway.GetWarningNotes(personId);

            return new ListWarningNotesResponse
            {
                WarningNotes = warningNotes?.Select(x => x.ToDomain()).ToList()
            };
        }

        public WarningNoteResponse ExecuteGetWarningNoteById(long warningNoteId)
        {
            var warningNote = _databaseGateway.GetWarningNoteById(warningNoteId);

            return warningNote?.ToResponse();
        }

        public void ExecutePatch(PatchWarningNoteRequest request)
        {
            _databaseGateway.PatchWarningNote(request);
        }
    }
}
