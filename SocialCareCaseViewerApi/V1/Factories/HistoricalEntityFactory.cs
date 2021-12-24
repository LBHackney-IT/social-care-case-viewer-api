using SocialCareCaseViewerApi.V1.Domain;
using SocialCareCaseViewerApi.V1.Infrastructure;

#nullable enable
namespace SocialCareCaseViewerApi.V1.Factories
{
    public static class HistoricalEntityFactory
    {
        public static CaseNote ToDomain(this HistoricalCaseNote caseNote)
        {
            return new CaseNote
            {
                MosaicId = caseNote.PersonId.ToString(),
                CaseNoteId = caseNote.Id.ToString(),
                CaseNoteTitle = caseNote.Title,
                CreatedOn = caseNote.CreatedOn,
                NoteType = caseNote.NoteType,
                CreatedByName = caseNote.CreatedBy,
                CreatedByEmail = caseNote.CreatedByWorker.EmailAddress,
                CaseNoteContent = caseNote.Note
            };
        }

        public static Visit ToDomain(this HistoricalVisit visit)
        {

            return new Visit
            {
                VisitId = visit.VisitId,
                PersonId = visit.PersonId,
                VisitType = visit.VisitType,
                PlannedDateTime = visit.PlannedDateTime,
                ActualDateTime = visit.ActualDateTime,
                ReasonNotPlanned = visit.ReasonNotPlanned,
                ReasonVisitNotMade = visit.ReasonVisitNotMade,
                SeenAloneFlag = !string.IsNullOrEmpty(visit.SeenAloneFlag) && visit.SeenAloneFlag.Equals("Y"),
                CompletedFlag = !string.IsNullOrEmpty(visit.CompletedFlag) && visit.CompletedFlag.Equals("Y"),
                CreatedByEmail = visit.Worker.EmailAddress,
                CreatedByName = $"{visit.Worker.FirstNames} {visit.Worker.LastNames}"
            };
        }
    }
}
