using System;
using System.Linq;
using Bogus;
using NUnit.Framework;
using SocialCareCaseViewerApi.Tests.V1.Helpers;
using SocialCareCaseViewerApi.V1.Boundary.Requests;

namespace SocialCareCaseViewerApi.Tests.V1.Boundary.Request
{
    [TestFixture]
    public class CreateCaseNoteRequestTests
    {
        private Faker _faker;

        [SetUp]
        public void SetUp()
        {
            _faker = new Faker();
        }

        private CreateCaseNoteRequest GetValidRequest()
        {
            return new CreateCaseNoteRequest()
            {
                FormName = "Form name",
                FormNameOverall = "Form name overall",
                FirstName = _faker.Person.FirstName,
                LastName = _faker.Person.LastName,
                WorkerEmail = _faker.Internet.Email(),
                DateOfBirth = DateTime.Now.AddYears(-20),
                DateOfEvent = DateTime.Now.AddDays(-3),
                PersonId = 5,
                ContextFlag = "A",
                CaseFormData = _faker.Random.String()
            };
        }

        [Test]
        public void ValidationFailsIfFormNameIsNotProvided()
        {
            var request = GetValidRequest();
            request.FormName = null;

            var errors = ValidationHelper.ValidateModel(request);

            Assert.AreEqual(1, errors.Count);
        }

        [Test]
        public void ValidationFailsIfFormNameOverallIsNotProvided()
        {
            var request = GetValidRequest();
            request.FormNameOverall = null;

            var errors = ValidationHelper.ValidateModel(request);

            Assert.AreEqual(1, errors.Count);
        }

        [Test]
        public void ValidationFailsIfFirstNameIsNotProvided()
        {
            var request = GetValidRequest();
            request.FirstName = null;

            var errors = ValidationHelper.ValidateModel(request);

            Assert.AreEqual(1, errors.Count);
        }

        [Test]
        public void ValidationFailsIfLastNameIsNotProvided()
        {
            var request = GetValidRequest();
            request.LastName = null;

            var errors = ValidationHelper.ValidateModel(request);

            Assert.AreEqual(1, errors.Count);
        }

        [Test]
        public void ValidationFailsIfWorkerEmailIsNotProvided()
        {
            var request = GetValidRequest();
            request.WorkerEmail = null;

            var errors = ValidationHelper.ValidateModel(request);

            Assert.AreEqual(1, errors.Count);
        }

        [TestCase("not a valid email address")]
        [Test]
        public void ValidationFailsIfWorkerEmailIsNotValidEmail(string emailAddress)
        {
            var request = GetValidRequest();
            request.WorkerEmail = emailAddress;

            var errors = ValidationHelper.ValidateModel(request);

            Assert.AreEqual(1, errors.Count);
        }

        [Test]
        public void ValidationSucceedsIfWorkerEmailIsValidEmail()
        {
            var request = GetValidRequest();

            var errors = ValidationHelper.ValidateModel(request);

            Assert.AreEqual(0, errors.Count);
        }

        [Test]
        public void ValidationFailsIfMosaicIdIsNotProvided()
        {
            var request = GetValidRequest();
            request.PersonId = 0;

            var errors = ValidationHelper.ValidateModel(request);

            Assert.AreEqual(1, errors.Count);
        }

        [Test]
        public void ValidationFailsIfContextFlagIsNotProvided()
        {
            var request = GetValidRequest();
            request.ContextFlag = null;

            var errors = ValidationHelper.ValidateModel(request);

            Assert.AreEqual(1, errors.Count);
        }

        [Test]
        [TestCase("A")]
        [TestCase("C")]
        public void ValidationSucceedsIfContextIsProvidedAndTheValueIsValid(string context)
        {
            var request = GetValidRequest();
            request.ContextFlag = context;

            var errors = ValidationHelper.ValidateModel(request);
            Assert.IsTrue(errors.Count == 0);
        }

        [Test]
        public void ValidationFailsIfContextIsProvidedButTheValueIsNotEitherAorC()
        {
            var request = GetValidRequest();
            request.ContextFlag = "d";

            var errors = ValidationHelper.ValidateModel(request);
            Assert.IsTrue(errors.Count == 1);
            Assert.IsTrue(errors.Any(x => x.ErrorMessage.Contains("The context_flag must be 'A' or 'C' only")));
        }

        [Test]
        public void ValidationFailsIfCaseFormDataIsNotProvided()
        {
            var request = GetValidRequest();
            request.CaseFormData = null;

            var errors = ValidationHelper.ValidateModel(request);

            Assert.AreEqual(1, errors.Count);
        }
    }
}
