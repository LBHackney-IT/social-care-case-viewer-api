using AutoFixture;
using FluentAssertions;
using NUnit.Framework;
using SocialCareCaseViewerApi.Tests.V1.Helpers;
using SocialCareCaseViewerApi.V1.Domain;
using SocialCareCaseViewerApi.V1.Gateways;
using SocialCareCaseViewerApi.V1.Infrastructure;
using System;
using System.Linq;

namespace SocialCareCaseViewerApi.Tests.V1.Gateways.HistoricalData
{
    public class HistoricalGetCaseNotesTests : DatabaseTests
    {
        private HistoricalSocialCareGateway _classUnderTest;

        [SetUp]
        public void Setup()
        {
            _classUnderTest = new HistoricalSocialCareGateway(DatabaseContext);
        }

        [Test]
        public void WhenThereAreNoMatchingRecordsReturnsEmptyList()
        {
            var response = _classUnderTest.GetAllCaseNotes(123);

            response.Should().BeEmpty();
        }

        [Test]
        public void WhenThereIsOneMatchReturnsAListContainingTheMatchingCaseNote()
        {
            var person = AddPersonToDatabase();
            var (caseNote, _, _) = AddCaseNoteWithNoteTypeAndWorkerToDatabase(person.Id);

            var response = _classUnderTest.GetAllCaseNotes(person.Id);

            response.Count.Should().Be(1);
            response.FirstOrDefault()?.CaseNoteId.Should().Be(caseNote.Id.ToString());
        }

        [Test]
        public void WhenThereAreMultipleMatchesReturnsAListContainingAllMatchingCaseNotes()
        {
            var person = AddPersonToDatabase();
            AddCaseNoteWithNoteTypeAndWorkerToDatabase(person.Id);
            AddCaseNoteWithNoteTypeAndWorkerToDatabase(person.Id, 456);

            var response = _classUnderTest.GetAllCaseNotes(person.Id);

            response.Count.Should().Be(2);
        }

        [Test]
        public void WhenThereAreMatchingRecordsReturnsSpecificInformationAboutTheCaseNote()
        {
            var person = AddPersonToDatabase();
            var (caseNote, noteType, caseWorker) = AddCaseNoteWithNoteTypeAndWorkerToDatabase(person.Id);

            var expectedCaseNoteInformation = new CaseNote
            {
                MosaicId = caseNote.PersonId.ToString(),
                CaseNoteId = caseNote.Id.ToString(),
                NoteType = noteType.Description,
                CaseNoteTitle = caseNote.Title,
                CreatedOn = (DateTime) caseNote.CreatedOn,
                CreatedByName = $"{caseWorker.FirstNames} {caseWorker.LastNames}",
                CreatedByEmail = caseWorker.EmailAddress
            };

            var response = _classUnderTest.GetAllCaseNotes(person.Id);

            response.FirstOrDefault().Should().BeEquivalentTo(expectedCaseNoteInformation);
        }

        [Test]
        public void WhenListingMatchingRecordsWillNotReturnTheDetailedContentsOfACaseNote()
        {
            var person = AddPersonToDatabase();
            AddCaseNoteWithNoteTypeAndWorkerToDatabase(person.Id);

            var response = _classUnderTest.GetAllCaseNotes(person.Id);

            response.FirstOrDefault()?.CaseNoteContent.Should().BeNullOrEmpty();
        }

        private Person AddPersonToDatabase()
        {
            var databaseEntity = TestHelpers.CreatePerson();
            DatabaseContext.Persons.Add(databaseEntity);
            DatabaseContext.SaveChanges();
            return databaseEntity;
        }

        private (HistoricalCaseNote, HistoricalNoteType, HistoricalWorker) AddCaseNoteWithNoteTypeAndWorkerToDatabase(long personId, long caseNoteId = 123)
        {
            var faker = new Fixture();

            var caseNoteType = faker.Create<HistoricalNoteType>().Type;
            var caseNoteTypeDescription = faker.Create<HistoricalNoteType>().Description;
            var noteType = HistoricalTestHelper.CreateDatabaseNoteType(caseNoteType, caseNoteTypeDescription);
            DatabaseContext.HistoricalNoteTypes.Add(noteType);

            var worker = HistoricalTestHelper.CreateDatabaseWorker();
            DatabaseContext.HistoricalWorkers.Add(worker);

            var caseNote = HistoricalTestHelper.CreateDatabaseCaseNote(caseNoteId, personId, noteType.Type, worker);
            DatabaseContext.HistoricalCaseNotes.Add(caseNote);

            DatabaseContext.SaveChanges();

            return (caseNote, noteType, worker);
        }
    }
}
