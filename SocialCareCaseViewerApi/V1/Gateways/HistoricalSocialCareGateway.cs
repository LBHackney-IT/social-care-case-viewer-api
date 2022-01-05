using Microsoft.EntityFrameworkCore;
using SocialCareCaseViewerApi.V1.Domain;
using SocialCareCaseViewerApi.V1.Factories;
using SocialCareCaseViewerApi.V1.Gateways.Interfaces;
using SocialCareCaseViewerApi.V1.Infrastructure;
using System.Collections.Generic;
using System.Linq;

#nullable enable
namespace SocialCareCaseViewerApi.V1.Gateways
{
    public class HistoricalSocialCareGateway : IHistoricalSocialCareGateway
    {
        private readonly HistoricalSocialCareContext _databaseContext;

        public HistoricalSocialCareGateway(HistoricalSocialCareContext databaseContext)
        {
            _databaseContext = databaseContext;
        }

        public List<CaseNote> GetAllCaseNotes(long personId)
        {
            var caseNotes = _databaseContext.HistoricalCaseNotes
                .Where(note => note.PersonId == personId)
                .Include(x => x.CreatedByWorker)
                .ToList();

            return caseNotes.Select(AddRelatedInformationToCaseNote).ToList(); //TODO: TK look at refactoring this so object are always converted the same way
        }

        public CaseNote? GetCaseNoteInformationById(long caseNoteId)
        {
            var caseNote = _databaseContext.HistoricalCaseNotes.FirstOrDefault(caseNote => caseNote.Id == caseNoteId);

            if (caseNote == null) return null;

            var caseNoteInformation = caseNote.ToDomain();

            var noteType = _databaseContext.HistoricalNoteTypes.FirstOrDefault(noteType => noteType.Type == caseNote.NoteType);
            caseNoteInformation.NoteType = noteType?.Description;

            var createdByWorker = _databaseContext.HistoricalWorkers.FirstOrDefault(worker => worker.SystemUserId == caseNote.CreatedBy);
            if (createdByWorker != null)
            {
                caseNoteInformation.CreatedByName = $"{createdByWorker.FirstNames} {createdByWorker.LastNames}";
                caseNoteInformation.CreatedByEmail = createdByWorker.EmailAddress;
            }

            return caseNoteInformation;
        }

        public IEnumerable<Visit> GetVisitInformationByPersonId(long personId)
        {
            var visits = _databaseContext.HistoricalVisits
                .Where(visit => visit.PersonId == personId)
                .Include(visit => visit.Worker)
                .ToList();

            return visits.Select(x => x.ToDomain());
        }

        public Visit? GetVisitInformationByVisitId(long visitId)
        {
            var visitInformation = _databaseContext.HistoricalVisits
                .Include(visit => visit.Worker)
                .FirstOrDefault(visit => visit.VisitId == visitId);

            return visitInformation?.ToDomain();
        }

        private string? LookUpNoteTypeDescription(string noteTypeCode)
        {
            return _databaseContext.HistoricalNoteTypes.FirstOrDefault(type => type.Type.Equals(noteTypeCode))?.Description;
        }

        private CaseNote AddRelatedInformationToCaseNote(HistoricalCaseNote caseNote)
        {
            var caseNoteInformation = caseNote.ToDomain(includeNoteContent: false);
            caseNoteInformation.NoteType = LookUpNoteTypeDescription(caseNote.NoteType);

            var worker = caseNote.CreatedByWorker;

            caseNoteInformation.CreatedByName = worker != null ? $"{worker.FirstNames} {worker.LastNames}" : null;
            caseNoteInformation.CreatedByEmail = worker?.EmailAddress;

            return caseNoteInformation;
        }
    }
}
