using System;
using AutoFixture;
using Bogus;
using FluentAssertions;
using NUnit.Framework;
using SocialCareCaseViewerApi.Tests.V1.Helpers;
using SocialCareCaseViewerApi.V1.Boundary.Requests;

namespace SocialCareCaseViewerApi.Tests.V1.Boundary.Request
{
    [TestFixture]
    public class PostWarningNoteRequestTests
    {
        private PostWarningNoteRequest _classUnderTest;
        private Fixture _fixture;
        private Faker _faker;


        [SetUp]
        public void SetUp()
        {
            _classUnderTest = new PostWarningNoteRequest();
            _fixture = new Fixture();
            _faker = new Faker();
        }

        [Test]
        public void RequestHasPersonId()
        {
            _classUnderTest.PersonId.Should().Be(0);
        }

        [Test]
        public void RequestHasStartDate()
        {
            _classUnderTest.StartDate.Should().Be(null);
        }

        [Test]
        public void RequestHasReviewDate()
        {
            _classUnderTest.ReviewDate.Should().Be(null);
        }

        [Test]
        public void RequestHasEndDate()
        {
            _classUnderTest.EndDate.Should().Be(null);
        }

        [Test]
        public void RequestHasDisclosedWithIndividual()
        {
            _classUnderTest.DisclosedWithIndividual.Should().Be(false);
        }

        [Test]
        public void RequestHasDisclosedDetails()
        {
            _classUnderTest.DisclosedDetails.Should().Be(null);
        }

        [Test]
        public void RequestHasNotes()
        {
            _classUnderTest.Notes.Should().Be(null);
        }

        [Test]
        public void RequestHasNoteType()
        {
            _classUnderTest.NoteType.Should().Be(null);
        }

        [Test]
        public void RequestHasDisclosedDate()
        {
            _classUnderTest.DisclosedDate.Should().Be(null);
        }

        [Test]
        public void RequestHasDisclosedHow()
        {
            _classUnderTest.DisclosedHow.Should().Be(null);
        }

        [Test]
        public void RequestHasWarningNarrative()
        {
            _classUnderTest.WarningNarrative.Should().Be(null);
        }

        [Test]
        public void RequestHasManagerName()
        {
            _classUnderTest.ManagerName.Should().Be(null);
        }

        [Test]
        public void RequestHasDiscussedWithManagerDate()
        {
            _classUnderTest.DiscussedWithManagerDate.Should().Be(null);
        }

        [Test]
        public void RequestHasCreatedBy()
        {
            _classUnderTest.CreatedBy.Should().Be(null);
        }

        #region Model validation
        [Test]
        public void ValidationFailsIfPersonIdIsNotBiggerThan0()
        {
            var request = GetValidPostWarningNoteRequest();
            request.PersonId = 0;
            var error = ValidationHelper.ValidateModel(request);

            error.Count.Should().Be(1);
            error.Should().Contain(x => x.ErrorMessage.Contains("Please enter a value bigger than 0"));
        }

        [Test]
        public void ValidationFailsIfStartDateIsNotProvided()
        {
            var request = GetValidPostWarningNoteRequest();
            request.StartDate = null;

            var error = ValidationHelper.ValidateModel(request);

            error.Count.Should().Be(1);
            error.Should().Contain(x => x.ErrorMessage.Contains("The StartDate field is required"));
        }

        [Test]
        public void ValidationFailsIfDisclosedDetailsIsLongerThan1000Characters()
        {
            var request = GetValidPostWarningNoteRequest();
            request.DisclosedDetails = new string('a', 1001);
            var error = ValidationHelper.ValidateModel(request);

            error.Count.Should().Be(1);
            error.Should().Contain(x => x.ErrorMessage.Contains("Character limit of 1000 exceeded"));
        }

        [Test]
        public void ValidationPassesIfDisclosedDetailsIsNull()
        {
            var request = GetValidPostWarningNoteRequest();
            request.DisclosedDetails = null;
            var error = ValidationHelper.ValidateModel(request);

            error.Count.Should().Be(0);
        }

        [Test]
        public void ValidationFailsIfNotesIsLongerThan1000Characters()
        {
            var request = GetValidPostWarningNoteRequest();
            request.Notes = new string('a', 1001);
            var error = ValidationHelper.ValidateModel(request);

            error.Count.Should().Be(1);
            error.Should().Contain(x => x.ErrorMessage.Contains("Character limit of 1000 exceeded"));
        }

        [Test]
        public void ValidationFailsIfNotesIsNull()
        {
            var request = GetValidPostWarningNoteRequest();
            request.Notes = null;
            var error = ValidationHelper.ValidateModel(request);

            error.Count.Should().Be(1);
            error.Should().Contain(x => x.ErrorMessage.Contains("The Notes field is required"));

        }

        [Test]
        public void ValidationFailsIfNoteTypesIsLongerThan50Characters()
        {
            var request = GetValidPostWarningNoteRequest();
            request.NoteType = new string('a', 51);
            var error = ValidationHelper.ValidateModel(request);

            error.Count.Should().Be(1);
            error.Should().Contain(x => x.ErrorMessage.Contains("Character limit of 50 exceeded"));
        }

        [Test]
        public void ValidationPassesIfNoteTypeIsNull()
        {
            var request = GetValidPostWarningNoteRequest();
            request.NoteType = null;
            var error = ValidationHelper.ValidateModel(request);

            error.Count.Should().Be(0);
        }

        [Test]
        public void ValidationFailsIfDisclosedHowIsLongerThan50Characters()
        {
            var request = GetValidPostWarningNoteRequest();
            request.DisclosedHow = new string('a', 51);
            var error = ValidationHelper.ValidateModel(request);

            error.Count.Should().Be(1);
            error.Should().Contain(x => x.ErrorMessage.Contains("Character limit of 50 exceeded"));
        }

        [Test]
        public void ValidationPassesIfDisclosedHowsNull()
        {
            var request = GetValidPostWarningNoteRequest();
            request.DisclosedHow = null;
            var error = ValidationHelper.ValidateModel(request);

            error.Count.Should().Be(0);
        }

        [Test]
        public void ValidationFailsIfWarningNarrativeIsLongerThan1000Characters()
        {
            var request = GetValidPostWarningNoteRequest();
            request.WarningNarrative = new string('a', 1001);
            var error = ValidationHelper.ValidateModel(request);

            error.Count.Should().Be(1);
            error.Should().Contain(x => x.ErrorMessage.Contains("Character limit of 1000 exceeded"));
        }

        [Test]
        public void ValidationPassesIfWarningNarrativeIsNull()
        {
            var request = GetValidPostWarningNoteRequest();
            request.WarningNarrative = null;
            var error = ValidationHelper.ValidateModel(request);

            error.Count.Should().Be(0);
        }

        [Test]
        public void ValidationFailsIfManagerNameIsLongerThan100Characters()
        {
            var request = GetValidPostWarningNoteRequest();
            request.ManagerName = new string('a', 101);
            var error = ValidationHelper.ValidateModel(request);

            error.Count.Should().Be(1);
            error.Should().Contain(x => x.ErrorMessage.Contains("Character limit of 100 exceeded"));
        }

        [Test]
        public void ValidationPassesIfManagerNameIsNull()
        {
            var request = GetValidPostWarningNoteRequest();
            request.ManagerName = null;
            var error = ValidationHelper.ValidateModel(request);

            error.Count.Should().Be(0);
        }

        [Test]
        public void ValidationFailsIfCreatedByIsNotProvided()
        {
            var request = GetValidPostWarningNoteRequest();
            request.CreatedBy = null;
            var error = ValidationHelper.ValidateModel(request);

            error.Count.Should().Be(1);
            error.Should().Contain(x => x.ErrorMessage.Contains("The CreatedBy field is required"));
        }

        [Test]

        public void ValidationFailesIfCreatedByIsNotAValidEmailAddress()
        {
            var request = GetValidPostWarningNoteRequest();
            request.CreatedBy = "string";
            var error = ValidationHelper.ValidateModel(request);

            error.Count.Should().Be(1);
            error.Should().Contain(x => x.ErrorMessage.Contains("The CreatedBy field is not a valid e-mail address"));

        }
        #endregion

        private PostWarningNoteRequest GetValidPostWarningNoteRequest()
        {
            return new PostWarningNoteRequest
            {
                PersonId = _fixture.Create<long>(),
                StartDate = DateTime.Now,
                ReviewDate = DateTime.Now,
                EndDate = DateTime.Now,
                DisclosedWithIndividual = false,
                DisclosedDetails = _fixture.Create<string>(),
                Notes = _fixture.Create<string>(),
                NoteType = _fixture.Create<string>(),
                DisclosedDate = DateTime.Now,
                DisclosedHow = _fixture.Create<string>(),
                WarningNarrative = _fixture.Create<string>(),
                ManagerName = _fixture.Create<string>(),
                DiscussedWithManagerDate = DateTime.Now,
                CreatedBy = _faker.Internet.Email()
            };
        }
    }
}
