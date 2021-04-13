using FluentAssertions;
using NUnit.Framework;
using SocialCareCaseViewerApi.V1.Infrastructure;

namespace SocialCareCaseViewerApi.Tests.V1.Infrastructure
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
        public void WarningNoteIsAuditEntity()
        {
            _classUnderTest.Should().BeAssignableTo<IAuditEntity>();
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
            _classUnderTest.DisclosedWithIndividual.Should().Be(false);
        }

        [Test]
        public void WarningNoteHasNotificationDetails()
        {
            _classUnderTest.DisclosedDetails.Should().Be(null);
        }

        [Test]
        public void WarningNoteHasNotes()
        {
            _classUnderTest.Notes.Should().Be(null);
        }

        [Test]
        public void WarningNoteHasNextReviewDate()
        {
            _classUnderTest.NextReviewDate.Should().Be(null);
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
            _classUnderTest.DisclosedDate.Should().Be(null);
        }

        [Test]
        public void WarningNoteHasHowInformed()
        {
            _classUnderTest.DisclosedHow.Should().Be(null);
        }

        [Test]
        public void WarningNoteHasWarningNarrative()
        {
            _classUnderTest.WarningNarrative.Should().Be(null);
        }

        [Test]
        public void WarningNoteHasManagersName()
        {
            _classUnderTest.ManagerName.Should().Be(null);
        }

        [Test]
        public void WarningNoteHasDateManagerInformed()
        {
            _classUnderTest.DiscussedWithManagerDate.Should().Be(null);
        }

        #region Audit properties

        [Test]
        public void WarningNoteHasCreatedAt()
        {
            _classUnderTest.CreatedAt.Should().Be(null);
        }

        [Test]
        public void WarningNoteHasCreatedBy()
        {
            _classUnderTest.CreatedBy.Should().Be(null);
        }

        [Test]
        public void WarningNoteHasLastModifiedAt()
        {
            _classUnderTest.LastModifiedAt.Should().Be(null);
        }

        [Test]
        public void WarningNoteHadLastModifiedBy()
        {
            _classUnderTest.LastModifiedBy.Should().Be(null);
        }

        #endregion

        [Test]
        public void CloneReturnsMemberwiseClone()
        {
            var response = _classUnderTest.Clone();
            response.Should().NotBe(_classUnderTest);
            response.Should().BeEquivalentTo(_classUnderTest);
        }
    }
}
