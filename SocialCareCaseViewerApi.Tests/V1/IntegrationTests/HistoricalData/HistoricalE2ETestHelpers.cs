using AutoFixture;
using Bogus;
using SocialCareCaseViewerApi.Tests.V1.Helpers;
using SocialCareCaseViewerApi.V1.Infrastructure;

#nullable enable
namespace SocialCareCaseViewerApi.Tests.V1.IntegrationTests.HistoricalData
{
    public static class HistoricalE2ETestHelpers
    {
        public static HistoricalCaseNote AddCaseNoteForASpecificPersonToDb(HistoricalDataContext context, long personId, bool includeNoteContent = true)
        {
            var faker = new Faker();

            var noteTypeCode = faker.Random.String2(5);
            var noteTypeDescription = faker.Random.String2(15);
            var noteType = HistoricalTestHelper.CreateDatabaseNoteType(noteTypeCode, noteTypeDescription);
            context.HistoricalNoteTypes.Add(noteType);
            var savedWorker = HistoricalTestHelper.CreateDatabaseWorker();
            context.HistoricalWorkers.Add(savedWorker);

            var caseNoteId = faker.Random.Long(min: 1);
            var savedCaseNote = HistoricalTestHelper.CreateDatabaseCaseNote(caseNoteId, personId, savedWorker, noteType);

            context.HistoricalCaseNotes.Add(savedCaseNote);
            context.SaveChanges();

            return new HistoricalCaseNote
            {
                CreatedBy = $"{savedWorker.FirstNames} {savedWorker.LastNames}",
                CreatedByWorker = savedCaseNote.CreatedByWorker,
                Id = savedCaseNote.Id,
                LastUpdatedBy = savedCaseNote.LastUpdatedBy,
                Note = includeNoteContent == true ? savedCaseNote.Note : null,
                PersonId = savedCaseNote.PersonId,
                Title = savedCaseNote.Title,
                CreatedOn = savedCaseNote.CreatedOn,
                NoteType = noteType.Description,
                HistoricalNoteType = noteType
            };
        }

        public static HistoricalCaseNote AddCaseNoteWithNoteTypeAndWorkerToDatabase(HistoricalDataContext socialCareContext)
        {
            var noteType = HistoricalTestHelper.CreateDatabaseNoteType();
            var worker = HistoricalTestHelper.CreateDatabaseWorker();
            var caseNote = HistoricalTestHelper.CreateDatabaseCaseNote(createdWorker: worker, noteType: noteType);

            socialCareContext.HistoricalWorkers.Add(worker);
            socialCareContext.HistoricalCaseNotes.Add(caseNote);
            socialCareContext.SaveChanges();

            return new HistoricalCaseNote
            {
                CreatedBy = $"{worker.FirstNames} {worker.LastNames}",
                CreatedByWorker = caseNote.CreatedByWorker,
                Id = caseNote.Id,
                LastUpdatedBy = caseNote.LastUpdatedBy,
                Note = caseNote.Note,
                PersonId = caseNote.PersonId,
                Title = caseNote.Title,
                CreatedOn = caseNote.CreatedOn,
                NoteType = noteType.Type,
                HistoricalNoteType = noteType
            };
        }

        public static HistoricalVisit AddVisitToDatabase(HistoricalDataContext socialCareContext, HistoricalWorker? worker = null)
        {
            var visitInformation = HistoricalTestHelper.CreateDatabaseVisit(worker: worker);
            socialCareContext.HistoricalVisits.Add(visitInformation);
            socialCareContext.SaveChanges();

            return visitInformation;
        }

        public static HistoricalWorker AddWorkerToDatabase(HistoricalDataContext socialCareContext)
        {
            var worker = HistoricalTestHelper.CreateDatabaseWorker();
            socialCareContext.HistoricalWorkers.Add(worker);
            socialCareContext.SaveChanges();

            return worker;
        }
    }
}
