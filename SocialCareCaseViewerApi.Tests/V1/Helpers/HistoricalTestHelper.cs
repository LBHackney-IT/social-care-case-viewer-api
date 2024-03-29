using AutoFixture;
using Bogus;
using SocialCareCaseViewerApi.V1.Infrastructure;

#nullable enable
namespace SocialCareCaseViewerApi.Tests.V1.Helpers
{
    public static class HistoricalTestHelper
    {
        public static HistoricalCaseNote CreateDatabaseCaseNote(long id = 123, long personId = 123, HistoricalWorker? createdWorker = null, HistoricalNoteType? noteType = null, bool includeNoteType = true)
        {
            createdWorker ??= CreateDatabaseWorker();
            noteType ??= CreateDatabaseNoteType();

            HistoricalCaseNote caseNote = new Faker<HistoricalCaseNote>()
                .RuleFor(caseNote => caseNote.Id, id)
                .RuleFor(caseNote => caseNote.PersonId, personId)
                .RuleFor(caseNote => caseNote.NoteType, noteType.Type)
                .RuleFor(caseNote => caseNote.CreatedBy, createdWorker.SystemUserId)
                .RuleFor(caseNote => caseNote.CreatedByWorker, createdWorker)
                .RuleFor(caseNote => caseNote.CreatedOn, f => f.Date.Past())
                .RuleFor(caseNote => caseNote.Note, f => f.Random.String2(100))
                .RuleFor(caseNote => caseNote.Title, f => f.Random.String2(20));

            if (includeNoteType)
            {
                caseNote.HistoricalNoteType = noteType;
            }

            return caseNote;
        }

        public static HistoricalNoteType CreateDatabaseNoteType(string noteType = "CASSUMASC", string description = "Case Summary (ASC)")
        {
            return new Faker<HistoricalNoteType>()
                .RuleFor(noteType => noteType.Type, noteType)
                .RuleFor(noteType => noteType.Description, description);
        }

        public static HistoricalWorker CreateDatabaseWorker(
            string? firstName = null,
            string? lastName = null,
            string? email = null)
        {
            return new Faker<HistoricalWorker>()
                .RuleFor(worker => worker.Id, f => f.UniqueIndex)
                .RuleFor(worker => worker.FirstNames, f => firstName ?? f.Random.String2(30))
                .RuleFor(worker => worker.LastNames, f => lastName ?? f.Random.String2(30))
                .RuleFor(worker => worker.EmailAddress, f => email ?? f.Person.Email)
                .RuleFor(worker => worker.SystemUserId, f => f.Random.String2(10));
        }

        public static HistoricalVisit CreateDatabaseVisit(
            long? visitId = null,
            long? personId = null,
            long? orgId = null,
            long? workerId = null,
            long? cpVisitScheduleStepId = null,
            long? cpRegistrationId = null,
            HistoricalWorker? worker = null)
        {
            worker ??= CreateDatabaseWorker();

            HistoricalVisit visit = new Faker<HistoricalVisit>()
                .RuleFor(v => v.VisitId, f => visitId ?? f.UniqueIndex)
                .RuleFor(v => v.PersonId, f => personId ?? f.UniqueIndex)
                .RuleFor(v => v.VisitType, f => f.Random.String2(1, 20))
                .RuleFor(v => v.PlannedDateTime, f => f.Date.Past())
                .RuleFor(v => v.ActualDateTime, f => f.Date.Past())
                .RuleFor(v => v.ReasonNotPlanned, f => f.Random.String2(1, 16))
                .RuleFor(v => v.ReasonVisitNotMade, f => f.Random.String2(1, 16))
                .RuleFor(v => v.SeenAloneFlag, f => f.Random.String2(1, "YN"))
                .RuleFor(v => v.CompletedFlag, f => f.Random.String2(1, "YN"))
                .RuleFor(v => v.OrgId, f => orgId ?? f.UniqueIndex)
                .RuleFor(v => v.WorkerId, f => workerId ?? worker.Id)
                .RuleFor(v => v.CpRegistrationId, f => cpRegistrationId ?? f.UniqueIndex)
                .RuleFor(v => v.CpVisitScheduleStepId, f => cpVisitScheduleStepId ?? f.UniqueIndex)
                .RuleFor(v => v.CpVisitScheduleDays, f => f.Random.Number(999))
                .RuleFor(v => v.CpVisitOnTime, f => f.Random.String2(1, "YN"))
                .RuleFor(v => v.Worker, worker);

            return visit;
        }
    }
}
