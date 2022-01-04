using AutoFixture;
using Bogus;
using SocialCareCaseViewerApi.Tests.V1.Helpers;
using SocialCareCaseViewerApi.V1.Infrastructure;
using Person = SocialCareCaseViewerApi.V1.Infrastructure.Person;

#nullable enable
namespace SocialCareCaseViewerApi.Tests.V1.IntegrationTests.HistoricalData
{
    public static class HistoricalE2ETestHelpers
    {
        public static Person AddPersonToDatabase(DatabaseContext context)
        {
            var person = TestHelpers.CreatePerson();
            context.Persons.Add(person);
            context.SaveChanges();

            return person;
        }

        public static HistoricalCaseNote AddCaseNoteForASpecificPersonToDb(DatabaseContext context, long personId, bool includeNoteContent = true)
        {
            var fixture = new Fixture();
            var faker = new Faker();

            var noteTypeCode = fixture.Create<HistoricalNoteType>().Type;
            var noteTypeDescription = fixture.Create<HistoricalNoteType>().Description;
            var noteType = HistoricalTestHelper.CreateDatabaseNoteType(noteTypeCode!, noteTypeDescription!);
            context.HistoricalNoteTypes.Add(noteType);
            var savedWorker = HistoricalTestHelper.CreateDatabaseWorker();
            context.HistoricalWorkers.Add(savedWorker);

            var caseNoteId = faker.Random.Long(min: 1);
            var savedCaseNote = HistoricalTestHelper.CreateDatabaseCaseNote(caseNoteId, personId, noteType.Type!, savedWorker);

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
                NoteType = noteType.Description
            };
        }

        public static HistoricalCaseNote AddCaseNoteWithNoteTypeAndWorkerToDatabase(DatabaseContext socialCareContext)
        {
            var noteType = HistoricalTestHelper.CreateDatabaseNoteType();
            var worker = HistoricalTestHelper.CreateDatabaseWorker();
            var caseNote = HistoricalTestHelper.CreateDatabaseCaseNote(noteType: noteType.Type!, createdWorker: worker);

            socialCareContext.HistoricalNoteTypes.Add(noteType);
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
                NoteType = noteType.Description
            };
        }

        public static HistoricalVisit AddVisitToDatabase(DatabaseContext socialCareContext, HistoricalWorker? worker = null)
        {
            var visitInformation = HistoricalTestHelper.CreateDatabaseVisit(worker: worker);
            socialCareContext.HistoricalVisits.Add(visitInformation);
            socialCareContext.SaveChanges();

            return visitInformation;
        }

        public static HistoricalWorker AddWorkerToDatabase(DatabaseContext socialCareContext)
        {
            var worker = HistoricalTestHelper.CreateDatabaseWorker();
            socialCareContext.HistoricalWorkers.Add(worker);
            socialCareContext.SaveChanges();

            return worker;
        }
    }
}
