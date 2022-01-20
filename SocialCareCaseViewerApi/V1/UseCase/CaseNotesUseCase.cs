using SocialCareCaseViewerApi.V1.Boundary.Response;
using SocialCareCaseViewerApi.V1.Exceptions;
using SocialCareCaseViewerApi.V1.Factories;
using SocialCareCaseViewerApi.V1.Gateways.Interfaces;
using SocialCareCaseViewerApi.V1.UseCase.Interfaces;
using System;

namespace SocialCareCaseViewerApi.V1.UseCase
{
    public class CaseNotesUseCase : ICaseNotesUseCase
    {
        private readonly IHistoricalDataGateway _historicalDataGateway;

        public CaseNotesUseCase(IHistoricalDataGateway historicalSocialCareGateway)
        {
            _historicalDataGateway = historicalSocialCareGateway;
        }

        public ListCaseNotesResponse ExecuteGetByPersonId(long id)
        {
            var caseNotes = _historicalDataGateway.GetCaseNotesByPersonId(id);

            return new ListCaseNotesResponse()
            {
                CaseNotes = caseNotes
            };
        }

        public CaseNoteResponse ExecuteGetById(string id)
        {
            long noteId;

            try
            {
                noteId = Convert.ToInt64(id);
            }
            catch
            {
                throw new CaseNoteIdConversionException($"Note id conversion failed for {id}");
            }

            var caseNote = _historicalDataGateway.GetCaseNoteById(noteId);

            if (caseNote == null)
            {
                throw new CaseNoteNotFoundException();
            }

            return ResponseFactory.ToResponse(caseNote);
        }
    }
}
