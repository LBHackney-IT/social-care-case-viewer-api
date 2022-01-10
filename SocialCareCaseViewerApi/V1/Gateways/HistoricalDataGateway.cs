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
    public class HistoricalDataGateway : IHistoricalDataGateway
    {
        private readonly HistoricalDataContext _databaseContext;

        public HistoricalDataGateway(HistoricalDataContext databaseContext)
        {
            _databaseContext = databaseContext;
        }

        public List<CaseNote> GetCaseNotesByPersonId(long personId)
        {
            var caseNotes = _databaseContext.HistoricalCaseNotes
                .Where(note => note.PersonId == personId)
                .Include(x => x.CreatedByWorker)
                .Include(x => x.HistoricalNoteType)
                .ToList();

            return caseNotes.Select(x => x.ToDomain(includeNoteContent: false)).ToList();
        }

        public CaseNote? GetCaseNoteById(long caseNoteId)
        {
            var caseNote = _databaseContext.HistoricalCaseNotes
                .Include(caseNote => caseNote.CreatedByWorker)
                .Include(caseNote => caseNote.HistoricalNoteType)
                .FirstOrDefault(caseNote => caseNote.Id == caseNoteId);

            if (caseNote == null) return null;

            return caseNote.ToDomain();
        }

        public IEnumerable<Visit> GetVisitByPersonId(long personId)
        {
            var visits = _databaseContext.HistoricalVisits
                .Where(visit => visit.PersonId == personId)
                .Include(visit => visit.Worker)
                .ToList();

            return visits.Select(x => x.ToDomain());
        }

        public Visit? GetVisitById(long visitId)
        {
            var visitInformation = _databaseContext.HistoricalVisits
                .Include(visit => visit.Worker)
                .FirstOrDefault(visit => visit.VisitId == visitId);

            return visitInformation?.ToDomain();
        }
    }
}
