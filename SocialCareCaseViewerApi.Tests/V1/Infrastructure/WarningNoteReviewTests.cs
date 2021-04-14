using System;
using FluentAssertions;
using NUnit.Framework;
using SocialCareCaseViewerApi.V1.Infrastructure;

namespace SocialCareCaseViewerApi.Tests.V1.Infrastructure
{
    [TestFixture]
    public class WarningNoteReviewTests
    {
        private WarningNoteReview _classUnderTest;

        [SetUp]
        public void SetUp()
        {
            _classUnderTest = new WarningNoteReview();
        }

        [Test]
        public void WarningNoteReviewIsAuditEntity()
        {
            _classUnderTest.Should().BeAssignableTo<IAuditEntity>();
        }

        [Test]
        public void WarningNoteReviewHasId()
        {
            Assert.That(_classUnderTest,
                Has.Property("Id").
                InstanceOf(typeof(long)));
        }

        [Test]
        public void WarningNoteReviewHasWarningNoteId()
        {
            Assert.That(_classUnderTest,
                Has.Property("WarningNoteId").
                InstanceOf(typeof(long)));
        }

        [Test]
        public void WarningNoteReviewHasReviewDate()
        {
            Assert.That(_classUnderTest,
                Has.Property("ReviewDate").
                InstanceOf(typeof(DateTime)));
        }

       [Test]
        public void WarningNoteReviewHasDisclosedWithIndividual()
        {
            Assert.That(_classUnderTest,
                Has.Property("DisclosedWithIndividual").
                InstanceOf(typeof(bool)));
        }

        [Test]
        public void WarningNoteReviewHasNotes()
        {
            Assert.That(_classUnderTest,
                Has.Property("Notes"));
        }

        [Test]
        public void WarningNoteReviewManagerName()
        {
            Assert.That(_classUnderTest,
                Has.Property("ManagerName"));
        }

        [Test]
        public void WarningNoteReviewHasDiscussedWithManagerDate()
        {
            Assert.That(_classUnderTest,
                Has.Property("DiscussedWithManagerDate").
                InstanceOf(typeof(DateTime)));
        }

        [Test]
        public void WarningNoteReviewHasCreatedAt()
        {
            Assert.That(_classUnderTest,
                Has.Property("CreatedAt").
                InstanceOf(typeof(DateTime)));
        }

        [Test]
        public void WarningNoteReviewHasCreatedBy()
        {
            Assert.That(_classUnderTest,
                Has.Property("CreatedBy"));
        }

        [Test]
        public void WarningNoteReviewHasLastModifiedAt()
        {
            Assert.That(_classUnderTest,
                Has.Property("LastModifiedAt").
                InstanceOf(typeof(DateTime)));
        }

        [Test]
        public void WarningNoteReviewHasLastModifiedBy()
        {
            Assert.That(_classUnderTest,
                Has.Property("LastModifiedBy"));
        }
    }
}