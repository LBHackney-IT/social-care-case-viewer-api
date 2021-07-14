using System;
using System.Collections.Generic;
using Bogus;
using FluentAssertions;
using NUnit.Framework;
using SocialCareCaseViewerApi.Tests.V1.Helpers;
using SocialCareCaseViewerApi.V1.Boundary.Requests;

namespace SocialCareCaseViewerApi.Tests.V1.Boundary.Request
{
    [TestFixture]
    public class CreatePersonalRelationshipRequestTests
    {
        private readonly Faker _faker = new Faker();
        CreatePersonalRelationshipRequestValidator createPersonalRelationshipRequestValidator;

        [SetUp]
        public void SetUp()
        {
            createPersonalRelationshipRequestValidator = new CreatePersonalRelationshipRequestValidator();
        }

        [Test]
        public void WhenRequestIsNullReturnsErrorsWithMessagesApartFromDetails()
        {
            var badRequest = new CreatePersonalRelationshipRequest();

            var response = createPersonalRelationshipRequestValidator.Validate(badRequest);

            response.IsValid.Should().BeFalse();
            response.Errors.Should().HaveCount(4);
            response.Errors.Should().Contain(e => e.ErrorMessage == "'personId' must be provided.");
            response.Errors.Should().Contain(e => e.ErrorMessage == "'otherPersonId' must be provided.");
            response.Errors.Should().Contain(e => e.ErrorMessage == "'type' must be provided.");
            response.Errors.Should().Contain(e => e.ErrorMessage == "'createdBy' must be provided.");
        }

        [Test]
        [TestCase("Yes")]
        [TestCase("No")]
        [TestCase("Foobar")]
        public void WhenIsMainCarerIsInvalidReturnsErrorWithMessage(string isMainCarer)
        {
            var badRequest = PersonalRelationshipsHelper.CreatePersonalRelationshipRequest(isMainCarer: isMainCarer);

            var response = createPersonalRelationshipRequestValidator.Validate(badRequest);

            response.IsValid.Should().BeFalse();
            response.Errors.Should().Contain(e => e.ErrorMessage == "'isMainCarer' must be 'Y' or 'N'.");
        }

        [Test]
        [TestCase("Y")]
        [TestCase("y")]
        [TestCase("N")]
        [TestCase("n")]
        public void WhenIsMainCarerIsValidReturnsItIsValid(string isMainCarer)
        {
            var validRequest = PersonalRelationshipsHelper.CreatePersonalRelationshipRequest(isMainCarer: isMainCarer);

            var response = createPersonalRelationshipRequestValidator.Validate(validRequest);

            response.IsValid.Should().BeTrue();
        }

        [Test]
        [TestCase("Yes")]
        [TestCase("No")]
        [TestCase("Foobar")]
        public void WhenIsInformalCarerIsInvalidReturnsErrorWithMessage(string isInformalCarer)
        {
            var badRequest = PersonalRelationshipsHelper.CreatePersonalRelationshipRequest(isInformalCarer: isInformalCarer);

            var response = createPersonalRelationshipRequestValidator.Validate(badRequest);

            response.IsValid.Should().BeFalse();
            response.Errors.Should().Contain(e => e.ErrorMessage == "'isInformalCarer' must be 'Y' or 'N'.");
        }

        [Test]
        [TestCase("Y")]
        [TestCase("y")]
        [TestCase("N")]
        [TestCase("n")]
        public void WhenIsInformalCarerIsValidReturnsItIsValid(string isInformalCarer)
        {
            var validRequest = PersonalRelationshipsHelper.CreatePersonalRelationshipRequest(isInformalCarer: isInformalCarer);

            var response = createPersonalRelationshipRequestValidator.Validate(validRequest);

            response.IsValid.Should().BeTrue();
        }

        [Test]
        public void WhenDetailsIsAbove1000CharactersReturnsErrorWithMessage()
        {
            var badRequest = PersonalRelationshipsHelper.CreatePersonalRelationshipRequest(details: _faker.Random.String(1001));

            var response = createPersonalRelationshipRequestValidator.Validate(badRequest);

            response.IsValid.Should().BeFalse();
            response.Errors.Should().Contain(e => e.ErrorMessage == "'details' must be less than or equal to 1,000 characters.");
        }

        [Test]
        public void WhenDetailsIs1000CharactersReturnsItIsValid()
        {
            var validRequest = PersonalRelationshipsHelper.CreatePersonalRelationshipRequest(details: _faker.Random.String(1000));

            var response = createPersonalRelationshipRequestValidator.Validate(validRequest);

            response.IsValid.Should().BeTrue();
        }

        [Test]
        public void WhenRequestIsValidReturnsItIsValid()
        {
            var validRequest = PersonalRelationshipsHelper.CreatePersonalRelationshipRequest();

            var response = createPersonalRelationshipRequestValidator.Validate(validRequest);

            response.IsValid.Should().BeTrue();
        }
    }
}
