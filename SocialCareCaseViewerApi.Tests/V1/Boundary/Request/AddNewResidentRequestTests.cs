using AutoFixture;
using Bogus;
using NUnit.Framework;
using SocialCareCaseViewerApi.Tests.V1.Helpers;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Domain;
using System;
using System.Collections.Generic;

namespace SocialCareCaseViewerApi.Tests.V1.Boundary.Request
{
    [TestFixture]
    public class AddNewResidentRequestTests
    {
        private AddNewResidentRequest _request;
        private AddNewResidentRequest _validRequest;
        private Fixture _fixture;
        private Faker _faker;

        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture();
            _faker = new Faker();
            _request = new AddNewResidentRequest();

            _validRequest = new AddNewResidentRequest()
            {
                Title = "Mr",
                FirstName = _faker.Name.FirstName(),
                LastName = _faker.Name.LastName(),
                Gender = "F",
                DateOfBirth = DateTime.Now.AddYears(-30),
                NhsNumber = _faker.Random.Number(),
                AgeGroup = "A",
                Nationality = "British",
                Address =  new AddressDomain()
                {
                    Address = "Flat 1, Street 2",
                    Postcode = "E8",
                    Uprn = 123456
                },
                PhoneNumbers = new List<PhoneNumber>()
                {
                    new PhoneNumber()
                    {
                        Number = _faker.Phone.PhoneNumber(),
                        Type = "Mobile"
                    }
                }
            };
        }

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
        public void RequestHasGender()
        {
            Assert.IsNull(_request.Gender);
        }

        [Test]
        public void RequestHasDateOfBirth()
        {
            Assert.IsNull(_request.DateOfBirth);
        }

        [Test]
        public void RequestHasNHSNumber()
        {
            Assert.IsNull(_request.NhsNumber);
        }

        [Test]
        public void RequestHasAgeGroup()
        {
            Assert.IsNull(_request.AgeGroup);
        }

        [Test]
        public void RequestHasNationality()
        {
            Assert.IsNull(_request.Nationality);
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

        #region Model validation
        [Test]
        public void ValidationFailsIfFirstNameIsNotProvided()
        {
            _validRequest.FirstName = null;

            var errors = ValidationHelper.ValidateModel(_request);

            Assert.AreEqual(1, errors.Count);
        }

        //[Test]
        //public void ValidationFailsIfLastNameIsNotProvided()
        //{
        //    _validRequest.

        //    var errors = ValidationHelper.ValidateModel(_request);

        //    Assert.AreEqual(1, errors.Count);
        //}


        #endregion
    }
}
