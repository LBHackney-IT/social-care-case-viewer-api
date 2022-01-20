using FluentAssertions;
using NUnit.Framework;
using SocialCareCaseViewerApi.Tests.V1.Helpers;
using SocialCareCaseViewerApi.V1.Domain;
using SocialCareCaseViewerApi.V1.Factories;

namespace SocialCareCaseViewerApi.Tests.V1.Factories
{
    public class HistoricalEntityFactoryTests
    {
        [Test]
        public void CanConvertHistoricalCaseNoteToCaseNote()
        {
            var historicalCaseNote = HistoricalTestHelper.CreateDatabaseCaseNote();

            var caseNote = historicalCaseNote.ToDomain();

            caseNote.Should().BeEquivalentTo(new CaseNote()
            {
                CaseNoteContent = historicalCaseNote.Note,
                CaseNoteId = historicalCaseNote.Id.ToString(),
                CaseNoteTitle = historicalCaseNote.Title,
                CreatedByEmail = historicalCaseNote.CreatedByWorker.EmailAddress,
                CreatedByName = $"{historicalCaseNote.CreatedByWorker.FirstNames} {historicalCaseNote.CreatedByWorker.LastNames}",
                CreatedOn = historicalCaseNote.CreatedOn,
                MosaicId = historicalCaseNote.PersonId.ToString(),
                NoteType = historicalCaseNote.HistoricalNoteType.Description
            });
        }

        [Test]
        public void CanConvertHistoricalCaseNoteToCaseNoteWhenAssociatedWorkerIsNull()
        {
            var historicalCaseNote = HistoricalTestHelper.CreateDatabaseCaseNote();
            historicalCaseNote.CreatedByWorker = null;

            var caseNote = historicalCaseNote.ToDomain();

            caseNote.CreatedByEmail.Should().BeNull();
            caseNote.CreatedByName.Should().BeNull();
        }

        [Test]
        public void CanConvertHistoricalCaseNoteToCaseNoteWhenAssociatedNoteTypeIsNull()
        {
            var historicalCaseNote = HistoricalTestHelper.CreateDatabaseCaseNote();
            historicalCaseNote.HistoricalNoteType = null;

            var caseNote = historicalCaseNote.ToDomain();

            caseNote.NoteType.Should().BeNull();
        }

        [Test]
        public void WhenConvertingHistoricalCaseNoteToCaseNoteSetsTheNoteContentToNullIfRequested()
        {
            var historicalCaseNote = HistoricalTestHelper.CreateDatabaseCaseNote();

            var caseNote = historicalCaseNote.ToDomain(includeNoteContent: false);

            caseNote.CaseNoteContent.Should().BeNull();
        }

        [Test]
        public void CanConvertHistoricalVisitToVisit()
        {
            var historicalVisit = HistoricalTestHelper.CreateDatabaseVisit();

            var visit = historicalVisit.ToDomain();

            visit.Should().BeEquivalentTo(new Visit()
            {
                ActualDateTime = visit.ActualDateTime,
                SeenAloneFlag = visit.SeenAloneFlag,
                CompletedFlag = visit.CompletedFlag,
                CreatedByEmail = visit.CreatedByEmail,
                CreatedByName = visit.CreatedByName,
                PersonId = visit.PersonId,
                PlannedDateTime = visit.PlannedDateTime,
                ReasonNotPlanned = visit.ReasonNotPlanned,
                ReasonVisitNotMade = visit.ReasonVisitNotMade,
                VisitId = visit.VisitId,
                VisitType = visit.VisitType
            });
        }

        [Test]
        public void CanConvertHistoricalVisitToVisitWhenAssociatedWorkerIsNull()
        {
            var historicalVisit = HistoricalTestHelper.CreateDatabaseVisit();
            historicalVisit.Worker = null;

            var visit = historicalVisit.ToDomain();

            visit.CreatedByEmail.Should().BeNull();
            visit.CreatedByName.Should().BeNull();
        }

        [Test]
        public void CanConvertHistoricalVisitToVisitWhenSeenAloneFlagIsNullAndSetsTheFlagToFalse()
        {
            var historicalVisit = HistoricalTestHelper.CreateDatabaseVisit();
            historicalVisit.SeenAloneFlag = null;

            var visit = historicalVisit.ToDomain();

            visit.SeenAloneFlag.Should().BeFalse();
        }

        [Test]
        public void CanConvertHistoricalVisitToVisitWhenCompletedFlagIsNullAndSetsTheFlagToFalse()
        {
            var historicalVisit = HistoricalTestHelper.CreateDatabaseVisit();
            historicalVisit.CompletedFlag = null;

            var visit = historicalVisit.ToDomain();

            visit.CompletedFlag.Should().BeFalse();
        }
    }
}
