using AutoFixture;
using Bogus;
using NUnit.Framework;
using SocialCareCaseViewerApi.Tests.V1.Helpers;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Domain;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SocialCareCaseViewerApi.Tests.V1.Boundary.Request
{
    [TestFixture]
    public class AddNewResidentRequestTests
    {
        private AddNewResidentRequest _request;
        private Fixture _fixture;
        private Faker _faker;

        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture();
            _faker = new Faker();
            _request = new AddNewResidentRequest();
        }

        private AddNewResidentRequest GetValidRequest()
        {
            return new AddNewResidentRequest()
            {
                Title = "Title",
                FirstName = _faker.Name.FirstName(),
                LastName = _faker.Name.LastName(),
                OtherNames = _fixture.Create<List<OtherName>>(),
                Gender = "M", //TODO: set and test valid values
                DateOfBirth = DateTime.Now.AddYears(-30),
                DateOfDeath = DateTime.Now,             
                Ethnicity = "Ethinicity",
                FirstLanguage = "English",
                Religion = "Religion",
                SexualOrientation = "Sexual orientation",
                NhsNumber = _faker.Random.Number(),
                Address = _fixture.Create<AddressDomain>(),
                PhoneNumbers = _fixture.Create<List<PhoneNumber>>(), //TODO: make sure only one is set to main
                EmailAddress = _faker.Internet.Email(),
                PreferredMethodOfContact = "Email", //TOOD: set and test valid values?
                ContextFlag = "A", //TOOD: set and test valid values,
                CreatedBy = _faker.Internet.Email()
            };
        }

        #region Model
        [Test]
        public void RequestHasTitle() 
        {
            Assert.IsNull(_request.Title);
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
        public void RequestHasOtherNames()
        {
            Assert.IsNull(_request.OtherNames);
        }

        [Test]
        public void RequestHasGender() 
        {
            Assert.IsNull(_request.Gender);
        }

        [Test]
        public void RequestHasDateOfBirth() 
        {
            Assert.AreEqual(null, _request.DateOfBirth);
        }

        [Test]
        public void RequestHasDateOfDeath()
        {
            Assert.IsNull(_request.DateOfDeath);
        }


        [Test]
        public void RequestHasEthnicity()
        {
            Assert.IsNull(_request.Ethnicity);
        }

        [Test]
        public void RequestHasFirstLanguage()
        {
            Assert.IsNull(_request.FirstLanguage);
        }

        [Test]
        public void RequestHasReligion()
        {
            Assert.IsNull(_request.Religion);
        }

        [Test]
        public void RequestHasSexualOrientation()
        {
            Assert.IsNull(_request.SexualOrientation);
        }

        [Test]
        public void RequestHasNHSNumber()
        {
            Assert.IsNull(_request.NhsNumber);
        }

        [Test]
        public void RequestHasAddress()
        {
            Assert.IsNull(_request.Address);
        }

        [Test]
        public void RequestHasPhoneNumbers()
        {
            Assert.IsNull(_request.PhoneNumbers);
        }

        [Test]
        public void RequestHasEmailAddress()
        {
            Assert.IsNull(_request.EmailAddress);
        }

        [Test]
        public void RequestHasPreferredMethodOfContact()
        {
            Assert.IsNull(_request.PreferredMethodOfContact);
        }

        [Test]
        public void RequestHasContextFlag()
        {
            Assert.IsNull(_request.ContextFlag);
        }

        [Test]
        public void RequestHasCreatedBy()
        {
            Assert.IsNull(_request.CreatedBy);
        }
        #endregion

        #region Model validation

        [Test]
        public void ValidationPassesWhenAllPropertiesAreSetWithValidValues()
        {
            var request = GetValidRequest();

            var errors = ValidationHelper.ValidateModel(request);

            Assert.AreEqual(0, errors.Count);

        }

        [Test]
        public void ValidationFailsIfFirstNameIsNotProvided()
        {
            var request = GetValidRequest();

            request.FirstName = null;

            var errors = ValidationHelper.ValidateModel(request);

            Assert.AreEqual(1, errors.Count);
            Assert.IsTrue(errors.First().ErrorMessage == "The FirstName field is required.");
        }
        [Test]
        public void ValidationFailsIfLastNameIsNotProvided()
        {
            var request = GetValidRequest();
            request.LastName = null;

            var errors = ValidationHelper.ValidateModel(request);

            Assert.AreEqual(1, errors.Count);
            Assert.IsTrue(errors.First().ErrorMessage == "The LastName field is required.");
        }

        [Test]
        public void ValidationFailsIfDateOfBirthIsNotProvided()
        {
            var request = GetValidRequest();
            request.DateOfBirth = null;

            var errors = ValidationHelper.ValidateModel(request);

            Assert.AreEqual(1, errors.Count);
            Assert.IsTrue(errors.First().ErrorMessage == "The DateOfBirth field is required.");
        }

        [Test]
        public void ValidationFailsIfEmailAddressIsNotInValidFormat()
        {
            var request = GetValidRequest();
            request.EmailAddress = "this is not valid email address";

            var errors = ValidationHelper.ValidateModel(request);

            Assert.AreEqual(1, errors.Count);
            Assert.IsTrue(errors.First().ErrorMessage == "The EmailAddress field is not a valid e-mail address.");
        }

        [Test]
        public void ValidationPassesIfEmailAddressIsInValidFormat()
        {
            var request = GetValidRequest();
            request.EmailAddress = "valid@domain.com";

            var errors = ValidationHelper.ValidateModel(request);

            Assert.AreEqual(0, errors.Count);
        }

        [Test]
        public void ValidationFailsIfCreatedByEmailAddressIsNotInValidFormat()
        {
            var request = GetValidRequest();
            request.CreatedBy = "this is not valid email address";

            var errors = ValidationHelper.ValidateModel(request);

            Assert.AreEqual(1, errors.Count);
            Assert.IsTrue(errors.First().ErrorMessage == "The CreatedBy field is not a valid e-mail address.");
        }


        #endregion
    }
}
