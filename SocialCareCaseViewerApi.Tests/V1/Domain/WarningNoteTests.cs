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
        public void WarningNoteHasId()
        {
            _classUnderTest.Id.Should().Be(0);
        }

        [Test]
        public void WarningNoteHasPersonId()
        {
            _classUnderTest.PersonId.Should().Be(0);
        }

        [Test]
        public void WarningNoteHasStartDate()
        {
            _classUnderTest.StartDate.Should().Be(null);
        }

        [Test]
        public void WarningNoteHasEndDate()
        {
            _classUnderTest.EndDate.Should().Be(null);
        }

        [Test]
        public void WarningNoteHasIndividualNotified()
        {
            _classUnderTest.IndividualNotified.Should().Be(false);
        }

        [Test]
        public void WarningNoteHasNotificationDetails()
        {
            _classUnderTest.NotificationDetails.Should().Be(null);
        }

        [Test]
        public void WarningNoteHasReviewDetails()
        {
            _classUnderTest.ReviewDetails.Should().Be(null);
        }

        [Test]
        public void WarningNoteHasNoteType()
        {
            _classUnderTest.NoteType.Should().Be(null);
        }

        [Test]
        public void WarningNoteHasStatus()
        {
            _classUnderTest.Status.Should().Be(null);
        }

        [Test]
        public void WarningNoteHasDateInformed()
        {
            _classUnderTest.DateInformed.Should().Be(null);
        }

        [Test]
        public void WarningNoteHasHowInformed()
        {
            _classUnderTest.HowInformed.Should().Be(null);
        }

        [Test]
        public void WarningNoteHasWarningNarrative()
        {
            _classUnderTest.WarningNarrative.Should().Be(null);
        }

        [Test]
        public void WarningNoteHasManagersName()
        {
            _classUnderTest.ManagersName.Should().Be(null);
        }

        [Test]
        public void WarningNoteHasDateManagerInformed()
        {
            _classUnderTest.DateManagerInformed.Should().Be(null);
        }
    }
}