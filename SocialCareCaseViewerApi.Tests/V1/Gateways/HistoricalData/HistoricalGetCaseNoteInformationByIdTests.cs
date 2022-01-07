using FluentAssertions;
using NUnit.Framework;
using SocialCareCaseViewerApi.Tests.V1.Helpers;
using SocialCareCaseViewerApi.V1.Gateways;
using SocialCareCaseViewerApi.V1.Gateways.Interfaces;
using SocialCareCaseViewerApi.V1.Infrastructure;

namespace SocialCareCaseViewerApi.Tests.V1.Gateways.HistoricalData
{
    [NonParallelizable]
    [TestFixture]
    public class HistoricalGetCaseNoteInformationByIdTests : HistoricalDataDatabaseTests
    {
        private IHistoricalSocialCareGateway _classUnderTest;

        [SetUp]
        public void Setup()
        {
            _classUnderTest = new HistoricalSocialCareGateway(HistoricalSocialCareContext);
        }

        [Test]
        public void WhenThereIsNoMatchingRecordReturnsNull()
        {
            const int existentCaseNoteId = 123;
            const int nonexistentCaseNoteId = 456;
            AddCaseNoteWithNoteTypeAndWorkerToDatabase(id: existentCaseNoteId);

            var response = _classUnderTest.GetCaseNoteInformationById(nonexistentCaseNoteId);

            response.Should().BeNull();
        }

        [Test]
        public void WhenThereAreMultipleCaseNotesReturnsCaseNoteWithMatchingId()
        {
            var caseNote = AddCaseNoteWithNoteTypeAndWorkerToDatabase(id: 123);
            AddCaseNoteWithNoteTypeAndWorkerToDatabase(id: 456, caseNoteType: "DIFF", caseNoteTypeDescription: "Different");

            var response = _classUnderTest.GetCaseNoteInformationById(caseNote.Id);

            response?.CaseNoteId.Should().Be(caseNote.Id.ToString());
        }

        [Test]
        public void WhenThereIsAMatchingRecordReturnsDetailsFromCaseNotes()
        {
            var caseNote = AddCaseNoteWithNoteTypeAndWorkerToDatabase();

            var response = _classUnderTest.GetCaseNoteInformationById(caseNote.Id);

            response?.MosaicId.Should().Be(caseNote.PersonId.ToString());
            response?.CaseNoteId.Should().Be(caseNote.Id.ToString());
            response?.CaseNoteTitle.Should().Be(caseNote.Title);
            response?.CreatedOn.Should().BeCloseTo(caseNote.CreatedOn);
            response?.CaseNoteContent.Should().Be(caseNote.Note);
        }

        [Test]
        public void WhenThereIsAMatchingRecordReturnsNoteTypeFromNoteTypes()
        {
            const string noteType = "NOTETYPE";
            const string noteTypeDescription = "Note Type Description";
            var caseNote = AddCaseNoteWithNoteTypeAndWorkerToDatabase(caseNoteType: noteType, caseNoteTypeDescription: noteTypeDescription);

            var response = _classUnderTest.GetCaseNoteInformationById(caseNote.Id);

            response?.NoteType.Should().Be(noteTypeDescription);
        }

        [Test]
        public void WhenThereIsAMatchingRecordReturnsDetailsFromWorkers()
        {
            const string workerEmailAddress = "adora@grayskull.com";
            const string workerFirstName = "Foo";
            const string workerLastname = "Bar";
            var caseNote = AddCaseNoteWithNoteTypeAndWorkerToDatabase(workerFirstName, workerLastname, workerEmailAddress);

            var response = _classUnderTest.GetCaseNoteInformationById(caseNote.Id);

            response?.CreatedByName.Should().Be($"{workerFirstName} {workerLastname}");
            response?.CreatedByEmail.Should().Be(workerEmailAddress);
        }

        [Test]
        public void WhenNoteTypeCannotBeFoundReturnsNullForNoteType()
        {
            var nonexistentNoteType = HistoricalTestHelper.CreateDatabaseNoteType("NONEXISTENT", "Non existent Case Note Type");
            var worker = HistoricalTestHelper.CreateDatabaseWorker();
            var caseNote = HistoricalTestHelper.CreateDatabaseCaseNote(includeNoteType: false);

            HistoricalSocialCareContext.HistoricalNoteTypes.Add(nonexistentNoteType);
            HistoricalSocialCareContext.HistoricalWorkers.Add(worker);
            HistoricalSocialCareContext.HistoricalCaseNotes.Add(caseNote);
            HistoricalSocialCareContext.SaveChanges();

            var response = _classUnderTest.GetCaseNoteInformationById(caseNote.Id);

            response?.NoteType.Should().BeNull();
        }

        private HistoricalCaseNote AddCaseNoteWithNoteTypeAndWorkerToDatabase(string workerFirstName = null, string workerLastName = null,
            string workerEmailAddress = null, long id = 123, long personId = 123, string caseNoteType = "CASSUMASC",
            string caseNoteTypeDescription = "Case Summary (ASC)")
        {
            var noteType = HistoricalTestHelper.CreateDatabaseNoteType(caseNoteType, caseNoteTypeDescription);
            var worker = HistoricalTestHelper.CreateDatabaseWorker(workerFirstName, workerLastName, workerEmailAddress);
            var caseNote = HistoricalTestHelper.CreateDatabaseCaseNote(id, personId, worker, noteType);

            HistoricalSocialCareContext.HistoricalNoteTypes.Add(noteType);
            HistoricalSocialCareContext.HistoricalWorkers.Add(worker);
            HistoricalSocialCareContext.HistoricalCaseNotes.Add(caseNote);
            HistoricalSocialCareContext.SaveChanges();

            return caseNote;
        }
    }
}
