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
    }
}
