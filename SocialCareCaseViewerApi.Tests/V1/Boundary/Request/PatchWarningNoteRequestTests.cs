using System;
using FluentValidation.TestHelper;
using NUnit.Framework;
using SocialCareCaseViewerApi.V1.Boundary.Requests;

namespace SocialCareCaseViewerApi.Tests.V1.Boundary.Request
{
    [TestFixture]
    public class PatchWarningNoteRequestTests
    {
        private PatchWarningNoteRequestValidator _classUnderTest;


        [SetUp]
        public void SetUp()
        {
            _classUnderTest = new PatchWarningNoteRequestValidator();
        }

        [Test]
        public void GivenARequestWithInvalidWarningNoteIdValidationFails()
        {
            var request = new PatchWarningNoteRequest { Status = "open" };

            _classUnderTest.ShouldHaveValidationErrorFor(x => x.WarningNoteId, request)
                .WithErrorMessage("Warning Note Id must be greater than 1");
        }

        [Test]
        public void GivenARequestWithNoReviewDateValidationFails()
        {
            var request = new PatchWarningNoteRequest { Status = "open" };

            _classUnderTest.ShouldHaveValidationErrorFor(x => x.ReviewDate, request)
                .WithErrorMessage("Review date required");
        }

        [Test]
        public void GivenARequestWithFutureReviewDateValidationFails()
        {
            var request = new PatchWarningNoteRequest
            {
                ReviewDate = DateTime.Now.AddYears(2),
                Status = "open"
            };

            _classUnderTest.ShouldHaveValidationErrorFor(x => x.ReviewDate, request)
                .WithErrorMessage("Review date must be in the past");
        }

        [Test]
        public void GivenARequestWithNoReviewedByEmailValidationFails()
        {
            var request = new PatchWarningNoteRequest
            {
                Status = "open"
            };

            _classUnderTest.ShouldHaveValidationErrorFor(x => x.ReviewedBy, request)
                .WithErrorMessage("Reviewer email required");
        }

        [Test]
        public void GivenARequestWithInvalidReviewedByEmailAddressValidationFails()
        {
            var request = new PatchWarningNoteRequest
            {
                ReviewedBy = "hello",
                Status = "open"
            };

            _classUnderTest.ShouldHaveValidationErrorFor(x => x.ReviewedBy, request)
                .WithErrorMessage("Provide a valid email address");
        }

        [Test]
        public void GivenARequestWithNextReviewDateSetInThePastValidationFails()
        {
            var request = new PatchWarningNoteRequest
            {
                NextReviewDate = new DateTime(2000, 1, 1),
                Status = "open"
            };

            _classUnderTest.ShouldHaveValidationErrorFor(x => x.NextReviewDate, request)
                .WithErrorMessage("Next review date must be in the future");
        }

        [Test]
        public void GivenARequestWithInvalidStatusValidationFails()
        {
            var request = new PatchWarningNoteRequest
            {
                Status = "hello"
            };

            _classUnderTest.ShouldHaveValidationErrorFor(x => x.Status, request)
                .WithErrorMessage("Provide a valid status");
        }

        [Test]
        public void GivenARequestWithEndedDateSetInTheFutureValidationFails()
        {
            var request = new PatchWarningNoteRequest
            {
                EndedDate = DateTime.Now.AddYears(2),
                Status = "open"
            };

            _classUnderTest.ShouldHaveValidationErrorFor(x => x.EndedDate, request)
                .WithErrorMessage("Ended date must be in the past");
        }

        [Test]
        public void GivenARequestWithInvalidEndedByEmailAddressValidationFails()
        {
            var request = new PatchWarningNoteRequest
            {
                EndedBy = "hello",
                Status = "open"
            };

            _classUnderTest.ShouldHaveValidationErrorFor(x => x.EndedBy, request)
                .WithErrorMessage("Provide a valid email address");
        }

        [Test]
        public void GivenARequestWithNoReviewNotesValidationFails()
        {
            var request = new PatchWarningNoteRequest
            {
                ReviewNotes = null,
                Status = "open"
            };

            _classUnderTest.ShouldHaveValidationErrorFor(x => x.ReviewNotes, request)
                .WithErrorMessage("Review notes required");
        }

        [Test]
        public void GivenARequestWithReviewNotesLessThanMinimumLengthValidationFails()
        {
            var request = new PatchWarningNoteRequest
            {
                ReviewNotes = "",
                Status = "open"
            };

            _classUnderTest.ShouldHaveValidationErrorFor(x => x.ReviewNotes, request)
                .WithErrorMessage("Review notes required");
        }

        [Test]
        public void GivenARequestWithReviewNotesMoreThanMaximumLengthValidationFails()
        {
            var request = new PatchWarningNoteRequest
            {
                ReviewNotes = new string('a', 1100),
                Status = "open"
            };

            _classUnderTest.ShouldHaveValidationErrorFor(x => x.ReviewNotes, request)
                .WithErrorMessage("Review notes be less than 1000 characters");
        }

        [Test]
        public void GivenARequestWithManagerNameMoreThanMaximumLengthValidationFails()
        {
            var request = new PatchWarningNoteRequest
            {
                ManagerName = new string('a', 110),
                Status = "open"
            };

            _classUnderTest.ShouldHaveValidationErrorFor(x => x.ManagerName, request)
                .WithErrorMessage("Manager name must be less than 100 characters");
        }

        [Test]
        public void GivenARequestWithDiscussedWithManagerDateSetInTheFutureValidationFails()
        {
            var request = new PatchWarningNoteRequest
            {
                DiscussedWithManagerDate = DateTime.Now.AddYears(2),
                Status = "open"
            };

            _classUnderTest.ShouldHaveValidationErrorFor(x => x.DiscussedWithManagerDate, request)
                .WithErrorMessage("Discussed with manager date must be in the past");
        }


    }
}
