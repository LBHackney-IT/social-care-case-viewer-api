using Bogus;
using NUnit.Framework;
using SocialCareCaseViewerApi.Tests.V1.Helpers;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using System;
using System.Linq;

namespace SocialCareCaseViewerApi.Tests.V1.Boundary.Request
{
    [TestFixture]
    public class CreateCaseNoteRequestTests
    {
        private CreateCaseNoteRequest _request;
        private Faker _faker;

        [SetUp]
        public void SetUp()
        {
            _request = new CreateCaseNoteRequest();
            _faker = new Faker();
        }

        private CreateCaseNoteRequest GetValidRequest()
        {
            return new CreateCaseNoteRequest()
            {
                FormName = "Form name",
                FormNameOverall = "Form name overal",
                FirstName = _faker.Person.FirstName,
                LastName = _faker.Person.LastName,
                WorkerEmail = _faker.Internet.Email(),
                DateOfBirth = DateTime.Now.AddYears(-20).ToString(),
                PersonId = 5,
                ContextFlag = "A",
                CaseFormData = _faker.Random.String()
            };
        }

        //TODO: set in the gateway
        //[Test]
        //public void RequestHasTimeStamp()
        //{
        //    Assert.AreEqual(DateTime.MinValue, _request.TimeStamp);
        //}

        [Test]
        public void RequestHasFormName()
        {
            Assert.IsNull(_request.FormName);
        }

        [Test]
        public void RequestHasFormNameOverall()
        {
            Assert.IsNull(_request.FormNameOverall);
        }
        [Test]
        public void RequestHasFirstName()
        {
            Assert.IsNull(_request.FirstName);
        }
        [Test]
        public void RequestHasLastName()
        {
            Assert.IsNull(_request.LastName);
        }
        [Test]
        public void RequestHasWorkerEmail()
        {
            Assert.IsNull(_request.WorkerEmail);
        }
        [Test]
        public void RequestHasDateOfBirth()
        {
            Assert.IsNull(_request.DateOfBirth);
        }
        [Test]
        public void RequestHasMosaicId()
        {
            Assert.AreEqual(0, _request.PersonId);
        }
        [Test]
        public void RequestHasContextFlag()
        {
            Assert.IsNull(_request.ContextFlag);
        }

        [Test]
        public void RequestHasCaseFormData()
        {
            Assert.IsNull(_request.CaseFormData);
        }

        #region model validation

        [Test]
        public void ValidationFailsIfFormNameIsNotProvided()
        {
            var request = GetValidRequest();
            request.FormName = null;

            var errors = ValidationHelper.ValidateModel(request);

            Assert.AreEqual(1, errors.Count);
        }

        [Test]
        public void ValidationFailsIfFormNameOveralIsNotProvided()
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

        #endregion
    }
}
