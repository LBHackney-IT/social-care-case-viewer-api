using System;
using System.Collections.Generic;
using System.Linq;
using AutoFixture;
using Bogus;
using NUnit.Framework;
using SocialCareCaseViewerApi.Tests.V1.Helpers;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Domain;

namespace SocialCareCaseViewerApi.Tests.V1.Boundary.Request
{
    [TestFixture]
    public class UpdatePersonRequestTests
    {
        private Fixture _fixture;
        private Faker _faker;

        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture();
            _faker = new Faker();
        }

        private UpdatePersonRequest GetValidRequest()
        {
            return new UpdatePersonRequest()
            {
                Id = 555,
                Title = "Title",
                FirstName = _faker.Name.FirstName(),
                LastName = _faker.Name.LastName(),
                OtherNames = _fixture.Create<List<OtherName>>(),
                Gender = "M",
                DateOfBirth = DateTime.Now.AddYears(-30),
                DateOfDeath = DateTime.Now,
                Ethnicity = "Ethnicity",
                FirstLanguage = "English",
                Religion = "Religion",
                SexualOrientation = "Sexual orientation",
                NhsNumber = _faker.Random.Number(),
                Address = _fixture.Create<AddressDomain>(),
                PhoneNumbers = _fixture.Create<List<PhoneNumber>>(),
                EmailAddress = _faker.Internet.Email(),
                PreferredMethodOfContact = "Email",
                ContextFlag = "A",
                CreatedBy = _faker.Internet.Email(),
                Restricted = "Y"
            };
        }

        [Test]
        public void ValidationPassesWhenAllPropertiesAreSetWithValidValues()
        {
            var request = GetValidRequest();

            var errors = ValidationHelper.ValidateModel(request);

            Assert.AreEqual(0, errors.Count);
        }

        [Test]
        public void ValidationFailsIfPersonIdIsNotBiggerThan0()
        {
            UpdatePersonRequest request = GetValidRequest();
            request.Id = 0;

            var errors = ValidationHelper.ValidateModel(request);

            Assert.AreEqual(errors.Count, 1);
            Assert.IsTrue(errors.Any(x => x.ErrorMessage.Contains("Please enter valid person id")));
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
        public void ValidationPassesIfEmailAddressIsEmptyString()
        {
            var request = GetValidRequest();
            request.EmailAddress = "";

            var errors = ValidationHelper.ValidateModel(request);

            Assert.AreEqual(0, errors.Count);
        }

        [Test]
        [TestCase("A")]
        [TestCase("C")]
        public void ModelValidationSucceedsIfContextIsProvidedAndTheValueIsValid(string context)
        {
            var request = GetValidRequest();
            request.ContextFlag = context;

            var errors = ValidationHelper.ValidateModel(request);
            Assert.IsTrue(errors.Count == 0);
        }

        [Test]
        public void ModelValidationFailsIfContextIsProvidedButTheValueIsNotEitherAorC()
        {
            var request = GetValidRequest();
            request.ContextFlag = "d";

            var errors = ValidationHelper.ValidateModel(request);
            Assert.IsTrue(errors.Count == 1);
            Assert.IsTrue(errors.Any(x => x.ErrorMessage.Contains("The context_flag must be 'A' or 'C' only")));
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

        [Test]
        [TestCase("Y")]
        [TestCase("N")]
        public void ModelValidationSucceedsIfRestrictedIsProvidedAndTheValueIsValid(string restricted)
        {
            var request = GetValidRequest();
            request.Restricted = restricted;

            var errors = ValidationHelper.ValidateModel(request);
            Assert.IsTrue(errors.Count == 0);
        }

        [Test]
        public void ModelValidationFailsIfRestrictedIsProvidedButTheValueIsNotEitherYorN()
        {
            var request = GetValidRequest();
            request.Restricted = "X";

            var errors = ValidationHelper.ValidateModel(request);
            Assert.IsTrue(errors.Count == 1);
            Assert.IsTrue(errors.Any(x => x.ErrorMessage.Contains("Restricted must be 'Y' or 'N' only")));
        }
    }
}
