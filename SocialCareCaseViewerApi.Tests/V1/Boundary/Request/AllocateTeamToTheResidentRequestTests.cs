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
    public class AllocateResidentToTheTeamTests
    {
        private Fixture _fixture;
        private Faker _faker;

        [SetUp]
        public void SetUp()
        {
            _fixture = new Fixture();
            _faker = new Faker();
        }

        private AllocateResidentToTheTeamRequest GetValidRequest()
        {
            return new AllocateResidentToTheTeamRequest()
            {
                PersonId = 555,
                AllocatedTeamId = 777,
                CreatedBy = 1,
                Summary = _faker.Lorem.Sentence(),
                CarePackage = _faker.Name.Random.String(),
                RagRating = _faker.Name.Random.String(),
                AllocationDate = _faker.Date.Recent()
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
        public void ValidationFailsIfResidentIdLessThan1()
        {
            var request = GetValidRequest();

            request.PersonId = 0;

            var errors = ValidationHelper.ValidateModel(request);

            Assert.AreEqual(1, errors.Count);
            Assert.IsTrue(errors.First().ErrorMessage == "Resident Id must be grater than 1.");
        }

        [Test]
        public void ValidationFailsIfTeamIdLessThan1()
        {
            var request = GetValidRequest();

            request.AllocatedTeamId = 0;

            var errors = ValidationHelper.ValidateModel(request);

            Assert.AreEqual(1, errors.Count);
            Assert.IsTrue(errors.First().ErrorMessage == "Resident Id must be grater than 1.");
        }
    }
}
