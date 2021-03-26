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
    public class CreateWarningNoteRequestTests
    {
        private CreateWarningNoteRequest _classUnderTest;
        private Fixture _fixture;
        private Faker _faker;


        [SetUp]
        public void SetUp()
        {
            _classUnderTest = new CreateWarningNoteRequest();
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
        public void RequestHasEndDate()
        {
            _classUnderTest.EndDate.Should().Be(null);
        }

        [Test]
        public void RequestHasIndividualNotified()
        {
            _classUnderTest.IndividualNotified.Should().Be(false);
        }

        [Test]
        public void RequestHasNotificationDetails()
        {
            _classUnderTest.NotificationDetails.Should().Be(null);
        }

        [Test]
        public void RequestHasReviewDetails()
        {
            _classUnderTest.ReviewDetails.Should().Be(null);
        }

        [Test]
        public void RequestHasNoteType()
        {
            _classUnderTest.NoteType.Should().Be(null);
        }

        [Test]
        public void RequestHasStatus()
        {
            _classUnderTest.Status.Should().Be(null);
        }

        [Test]
        public void RequestHasDateInformed()
        {
            _classUnderTest.DateInformed.Should().Be(null);
        }

        [Test]
        public void RequestHasHowInformed()
        {
            _classUnderTest.HowInformed.Should().Be(null);
        }

        [Test]
        public void RequestHasWarningNarrative()
        {
            _classUnderTest.WarningNarrative.Should().Be(null);
        }

        [Test]
        public void RequestHasManagersName()
        {
            _classUnderTest.ManagersName.Should().Be(null);
        }

        [Test]
        public void RequestHasDateManagerInformed()
        {
            _classUnderTest.DateManagerInformed.Should().Be(null);
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
            var request = GetValidCreateWarningNoteRequest();
            request.PersonId = 0;
            var error = ValidationHelper.ValidateModel(request);

            error.Count.Should().Be(1);
            error.Should().Contain(x => x.ErrorMessage.Contains("Please enter a value bigger than 0"));
        }

        [Test]
        public void ValidationFailsIfStartDateIsNotProvided()
        {
            var request = GetValidCreateWarningNoteRequest();
            request.StartDate = null;

            var error = ValidationHelper.ValidateModel(request);

            error.Count.Should().Be(1);
            error.Should().Contain(x => x.ErrorMessage.Contains("The StartDate field is required"));
        }

        [Test]
        public void ValidationFailsIfNotificationDetailsIsLongerThan1000Characters()
        {
            var request = GetValidCreateWarningNoteRequest();
            request.NotificationDetails = new string('a', 1001);
            var error = ValidationHelper.ValidateModel(request);

            error.Count.Should().Be(1);
            error.Should().Contain(x => x.ErrorMessage.Contains("Character limit of 1000 exceeded"));
        }

        [Test]
        public void ValidationPassesIfNotificationDetailsIsNull()
        {
            var request = GetValidCreateWarningNoteRequest();
            request.NotificationDetails = null;
            var error = ValidationHelper.ValidateModel(request);

            error.Count.Should().Be(0);
        }

        [Test]
        public void ValidationFailsIfReviewDetailsIsLongerThan1000Characters()
        {
            var request = GetValidCreateWarningNoteRequest();
            request.ReviewDetails = new string('a', 1001);
            var error = ValidationHelper.ValidateModel(request);

            error.Count.Should().Be(1);
            error.Should().Contain(x => x.ErrorMessage.Contains("Character limit of 1000 exceeded"));
        }

        [Test]
        public void ValidationPassesIfReviewDetailsIsNull()
        {
            var request = GetValidCreateWarningNoteRequest();
            request.ReviewDetails = null;
            var error = ValidationHelper.ValidateModel(request);

            error.Count.Should().Be(0);
        }

        [Test]
        public void ValidationFailsIfNoteTypesIsLongerThan50Characters()
        {
            var request = GetValidCreateWarningNoteRequest();
            request.NoteType = new string('a', 51);
            var error = ValidationHelper.ValidateModel(request);

            error.Count.Should().Be(1);
            error.Should().Contain(x => x.ErrorMessage.Contains("Character limit of 50 exceeded"));
        }

        [Test]
        public void ValidationPassesIfNoteTypeIsNull()
        {
            var request = GetValidCreateWarningNoteRequest();
            request.NoteType = null;
            var error = ValidationHelper.ValidateModel(request);

            error.Count.Should().Be(0);
        }

        [Test]
        public void ValidationFailsIfStatusIsLongerThan50Characters()
        {
            var request = GetValidCreateWarningNoteRequest();
            request.Status = new string('a', 51);
            var error = ValidationHelper.ValidateModel(request);

            error.Count.Should().Be(1);
            error.Should().Contain(x => x.ErrorMessage.Contains("Character limit of 50 exceeded"));
        }

        [Test]
        public void ValidationPassesIfStatusIsNull()
        {
            var request = GetValidCreateWarningNoteRequest();
            request.Status = null;
            var error = ValidationHelper.ValidateModel(request);

            error.Count.Should().Be(0);
        }

        [Test]
        public void ValidationFailsIfHowInformedIsLongerThan50Characters()
        {
            var request = GetValidCreateWarningNoteRequest();
            request.HowInformed = new string('a', 51);
            var error = ValidationHelper.ValidateModel(request);

            error.Count.Should().Be(1);
            error.Should().Contain(x => x.ErrorMessage.Contains("Character limit of 50 exceeded"));
        }

        [Test]
        public void ValidationPassesIfHowInformedIsNull()
        {
            var request = GetValidCreateWarningNoteRequest();
            request.HowInformed = null;
            var error = ValidationHelper.ValidateModel(request);

            error.Count.Should().Be(0);
        }

        [Test]
        public void ValidationFailsIfWarningNarrativeIsLongerThan1000Characters()
        {
            var request = GetValidCreateWarningNoteRequest();
            request.WarningNarrative = new string('a', 1001);
            var error = ValidationHelper.ValidateModel(request);

            error.Count.Should().Be(1);
            error.Should().Contain(x => x.ErrorMessage.Contains("Character limit of 1000 exceeded"));
        }

        [Test]
        public void ValidationPassesIfWarningNarrativeIsNull()
        {
            var request = GetValidCreateWarningNoteRequest();
            request.WarningNarrative = null;
            var error = ValidationHelper.ValidateModel(request);

            error.Count.Should().Be(0);
        }

        [Test]
        public void ValidationFailsIfManagersNameIsLongerThan100Characters()
        {
            var request = GetValidCreateWarningNoteRequest();
            request.ManagersName = new string('a', 101);
            var error = ValidationHelper.ValidateModel(request);

            error.Count.Should().Be(1);
            error.Should().Contain(x => x.ErrorMessage.Contains("Character limit of 100 exceeded"));
        }

        [Test]
        public void ValidationPassesIfManagersNameIsNull()
        {
            var request = GetValidCreateWarningNoteRequest();
            request.ManagersName = null;
            var error = ValidationHelper.ValidateModel(request);

            error.Count.Should().Be(0);
        }

        [Test]
        public void ValidationFailsIfCreatedByIsNotProvided()
        {
            var request = GetValidCreateWarningNoteRequest();
            request.CreatedBy = null;
            var error = ValidationHelper.ValidateModel(request);

            error.Count.Should().Be(1);
            error.Should().Contain(x => x.ErrorMessage.Contains("The CreatedBy field is required"));
        }

        [Test]

        public void ValidationFailesIfCreatedByIsNotAValidEmailAddress()
        {
            var request = GetValidCreateWarningNoteRequest();
            request.CreatedBy = "string";
            var error = ValidationHelper.ValidateModel(request);

            error.Count.Should().Be(1);
            error.Should().Contain(x => x.ErrorMessage.Contains("The CreatedBy field is not a valid e-mail address"));

        }
        #endregion

        private CreateWarningNoteRequest GetValidCreateWarningNoteRequest()
        {
            return new CreateWarningNoteRequest()
            {
                PersonId = _fixture.Create<long>(),
                StartDate = DateTime.Now,
                EndDate = DateTime.Now,
                IndividualNotified = false,
                NotificationDetails = _fixture.Create<string>(),
                ReviewDetails = _fixture.Create<string>(),
                NoteType = _fixture.Create<string>(),
                Status = _fixture.Create<string>(),
                DateInformed = DateTime.Now,
                HowInformed = _fixture.Create<string>(),
                WarningNarrative = _fixture.Create<string>(),
                ManagersName = _fixture.Create<string>(),
                DateManagerInformed = DateTime.Now,
                CreatedBy = _faker.Internet.Email()
            };
        }
    }
}
