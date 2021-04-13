using System.Collections.Generic;
using System.Linq;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Boundary.Response;
using SocialCareCaseViewerApi.V1.Domain;
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

        public List<WarningNote> ExecuteGet(GetWarningNoteRequest request)
        {
            var warningNotes = _databaseGateway.GetWarningNotes(request);

            var response = warningNotes.Select(x => x.ToDomain()).ToList();
            return response;
        }
    }
}
