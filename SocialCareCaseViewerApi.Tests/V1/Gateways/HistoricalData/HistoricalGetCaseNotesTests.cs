using Bogus;
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
    public class HistoricalGetCaseNotesTests : HistoricalDataDatabaseTests
    {
        private HistoricalSocialCareGateway _classUnderTest;
        private long _personId;
        private Faker _faker;

        [SetUp]
        public void Setup()
        {
            _classUnderTest = new HistoricalSocialCareGateway(HistoricalSocialCareContext);
            _personId = 1L;
            _faker = new Faker();
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
            var (caseNote, _, _) = AddCaseNoteWithNoteTypeAndWorkerToDatabase(_personId);

            var response = _classUnderTest.GetAllCaseNotes(_personId);

            response.Count.Should().Be(1);
            response.FirstOrDefault()?.CaseNoteId.Should().Be(caseNote.Id.ToString());
        }

        [Test]
        public void WhenThereAreMultipleMatchesReturnsAListContainingAllMatchingCaseNotes()
        {
            AddCaseNoteWithNoteTypeAndWorkerToDatabase(_personId);
            AddCaseNoteWithNoteTypeAndWorkerToDatabase(_personId, 456);

            var response = _classUnderTest.GetAllCaseNotes(_personId);

            response.Count.Should().Be(2);
        }

        [Test]
        public void WhenThereAreMatchingRecordsReturnsSpecificInformationAboutTheCaseNote()
        {
            var (caseNote, noteType, caseWorker) = AddCaseNoteWithNoteTypeAndWorkerToDatabase(_personId);

            var expectedCaseNoteInformation = new CaseNote
            {
                MosaicId = caseNote.PersonId.ToString(),
                CaseNoteId = caseNote.Id.ToString(),
                NoteType = noteType.Description,
                CaseNoteTitle = caseNote.Title,
                CreatedOn = caseNote.CreatedOn,
                CreatedByName = $"{caseWorker.FirstNames} {caseWorker.LastNames}",
                CreatedByEmail = caseWorker.EmailAddress
            };

            var response = _classUnderTest.GetAllCaseNotes(_personId);

            response.FirstOrDefault().Should().BeEquivalentTo(expectedCaseNoteInformation,
                options =>
                {
                    options.Using<DateTime>(ctx => ctx.Subject.Should().BeCloseTo(ctx.Expectation, 1000)).WhenTypeIs<DateTime>();
                    return options;
                });
        }

        [Test]
        public void WhenListingMatchingRecordsWillNotReturnTheDetailedContentsOfACaseNote()
        {
            AddCaseNoteWithNoteTypeAndWorkerToDatabase(_personId);

            var response = _classUnderTest.GetAllCaseNotes(_personId);

            response.FirstOrDefault()?.CaseNoteContent.Should().BeNullOrEmpty();
        }

        [Test]
        public void WhenThereAreMatchingRecordsItPopulatesTheNoteTypeWithCorrectDescriptionValueFromHistoricalNoteTypes()
        {
            var (_, noteType, _) = AddCaseNoteWithNoteTypeAndWorkerToDatabase(_personId);

            var response = _classUnderTest.GetAllCaseNotes(_personId);

            response.FirstOrDefault()?.NoteType.Should().Be(noteType.Description);
        }

        private (HistoricalCaseNote, HistoricalNoteType, HistoricalWorker) AddCaseNoteWithNoteTypeAndWorkerToDatabase(long personId, long caseNoteId = 123)
        {
            var caseNoteType = _faker.Random.String2(5);
            var caseNoteTypeDescription = _faker.Random.String2(15);

            var noteType = HistoricalTestHelper.CreateDatabaseNoteType(caseNoteType, caseNoteTypeDescription);
            HistoricalSocialCareContext.HistoricalNoteTypes.Add(noteType);

            var worker = HistoricalTestHelper.CreateDatabaseWorker();
            HistoricalSocialCareContext.HistoricalWorkers.Add(worker);

            var caseNote = HistoricalTestHelper.CreateDatabaseCaseNote(caseNoteId, personId, worker, noteType);
            HistoricalSocialCareContext.HistoricalCaseNotes.Add(caseNote);

            HistoricalSocialCareContext.SaveChanges();

            return (caseNote, noteType, worker);
        }
    }
}
