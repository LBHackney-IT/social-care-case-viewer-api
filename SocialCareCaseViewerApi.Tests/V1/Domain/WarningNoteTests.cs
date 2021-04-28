using FluentAssertions;
using NUnit.Framework;
using SocialCareCaseViewerApi.V1.Domain;

namespace SocialCareCaseViewerApi.Tests.V1.Domain
{
    [TestFixture]
    public class WarningNoteTests
    {
        private WarningNote _classUnderTest;

        [SetUp]
        public void SetUp()
        {
            _classUnderTest = new WarningNote();
        }

        [Test]
        public void WarningNoteContainsTheRelevantInformationAboutItself()
        {
            // Unique ID
            _classUnderTest.Id.Should().Be(0);

            // Current Status
            _classUnderTest.Status.Should().BeNull();

            // Associated with a specific resident
            _classUnderTest.PersonId.Should().Be(0);

            // Start and End Dates
            _classUnderTest.StartDate.Should().BeNull();
            _classUnderTest.EndDate.Should().BeNull();

            // Review Details
            _classUnderTest.ReviewDate.Should().BeNull();
            _classUnderTest.NextReviewDate.Should().BeNull();

            // Creator of the Note
            _classUnderTest.CreatedBy.Should().BeNull();

            // Details about notifying an individual
            _classUnderTest.DisclosedWithIndividual.Should().BeFalse();
            _classUnderTest.DisclosedDetails.Should().BeNull();
            _classUnderTest.DisclosedDate.Should().BeNull();
            _classUnderTest.DisclosedHow.Should().BeNull();

            // Warning Note Content
            _classUnderTest.Notes.Should().BeNull();
            _classUnderTest.NoteType.Should().BeNull();
            _classUnderTest.WarningNarrative.Should().BeNull();

            // Informing a Manager
            _classUnderTest.ManagerName.Should().BeNull();
            _classUnderTest.DiscussedWithManagerDate.Should().BeNull();
        }
    }
}
