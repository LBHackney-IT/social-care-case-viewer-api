using System;
using System.Collections.Generic;
using System.Linq;
using AutoFixture;
using Bogus;
using FluentAssertions;
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
                TeamId = 777,
                CreatedBy = 1,
                Summary = _faker.Lorem.Sentence(),
                CarePackage = _faker.Name.Random.String(),
                RagRating = "red",
                AllocationDate = _faker.Date.Recent()
            };
        }

        [Test]
        public void ValidationPassesWhenAllPropertiesAreSetWithValidValues()
        {
            var request = GetValidRequest();
            var validator = new AllocateResidentToTheTeamRequestValidator();

            var validationResponse = validator.Validate(request);

            validationResponse.IsValid.Should().Be(true);

        }

        [Test]
        public void ValidationFailsIfResidentIdLessThan1()
        {
            var request = GetValidRequest();
            request.PersonId = 0;
            var validator = new AllocateResidentToTheTeamRequestValidator();

            var validationResponse = validator.Validate(request);

            validationResponse.IsValid.Should().Be(false);
            validationResponse.ToString().Should().Be("Resident Id must be grater than 0");
        }

        [Test]
        public void ValidationFailsIfTeamIdLessThan1()
        {
            var request = GetValidRequest();
            request.TeamId = 0;
            request.RagRating = "red";
            var validator = new AllocateResidentToTheTeamRequestValidator();

            var validationResponse = validator.Validate(request);

            validationResponse.IsValid.Should().Be(false);
            validationResponse.ToString().Should().Be("Team Id must be grater than 0");
        }

        [Test]
        public void ValidationFailsIfRagRatingDifferentThanExpected()
        {
            var request = GetValidRequest();
            request.RagRating = "Yellow";
            var validator = new AllocateResidentToTheTeamRequestValidator();

            var validationResponse = validator.Validate(request);

            validationResponse.IsValid.Should().Be(false);
            validationResponse.ToString().Should().Be("RAG rating must be 'green', 'red', 'amber' or 'purple'");
        }
        [Test]
        public void ValidationFailsIfRagRatingNull()
        {
            var request = GetValidRequest();
            request.RagRating = null;
            var validator = new AllocateResidentToTheTeamRequestValidator();

            var validationResponse = validator.Validate(request);

            validationResponse.IsValid.Should().Be(false);
            validationResponse.ToString().Should().Be("RAG rating must be provided");
        }
    }
}
