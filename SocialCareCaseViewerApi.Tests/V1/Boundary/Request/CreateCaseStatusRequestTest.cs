using Bogus;
using FluentAssertions;
using NUnit.Framework;
using SocialCareCaseViewerApi.Tests.V1.Helpers;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using System;
using System.Collections.Generic;

#nullable enable
namespace SocialCareCaseViewerApi.Tests.V1.Boundary.Request
{
    [TestFixture]
    public class CreateCaseStatusRequestTests
    {
        private readonly Faker _faker = new Faker();
        CreateCaseStatusRequestValidator _createCaseStatusRequestValidator = null!;

        [SetUp]
        public void SetUp()
        {
            _createCaseStatusRequestValidator = new CreateCaseStatusRequestValidator();
        }

        [Test]
        public void WhenRequestIsNullReturnsErrorsWithMessagesApartFromDetails()
        {
            var badRequest = new CreateCaseStatusRequest();
            var response = _createCaseStatusRequestValidator.Validate(badRequest);

            response.IsValid.Should().BeFalse();
            response.Errors.Should().HaveCount(5);
            response.Errors.Should().Contain(e => e.ErrorMessage == "'personId' must be provided.");
            response.Errors.Should().Contain(e => e.ErrorMessage == "'type' must be provided.");
            response.Errors.Should().Contain(e => e.ErrorMessage == "'createdBy' must be provided.");
            response.Errors.Should().Contain(e => e.ErrorMessage == "'startDate' must be provided.");
            response.Errors.Should().Contain(e => e.ErrorMessage == "'type' must be CIN, CP or LAC.");
        }

        [Test]
        public void WhenStartDateIsInTheFutureReturnsErrorWithMessage()
        {
            var badRequest = CaseStatusHelper.CreateCaseStatusRequest(startDate: DateTime.Today.AddDays(1));

            var response = _createCaseStatusRequestValidator.Validate(badRequest);

            response.IsValid.Should().BeFalse();
            response.Errors.Should().Contain(e => e.ErrorMessage == "'start_date' must be today or in the past");
        }

        [Test]
        public void WhenStartDateIsTodayReturnsItIsValid()
        {
            var badRequest = CaseStatusHelper.CreateCaseStatusRequest(startDate: DateTime.Today);

            var response = _createCaseStatusRequestValidator.Validate(badRequest);

            response.IsValid.Should().BeTrue();
        }

        [Test]
        public void WhenStartDateIsInThePastReturnsItIsValid()
        {
            var badRequest = CaseStatusHelper.CreateCaseStatusRequest(startDate: DateTime.Today.AddDays(-2));

            var response = _createCaseStatusRequestValidator.Validate(badRequest);

            response.IsValid.Should().BeTrue();
        }

        [Test]
        public void WhenStartDateIsNotProvidedReturnsErrorMessage()
        {
            var badRequest = CaseStatusHelper.CreateCaseStatusRequest();
            badRequest.StartDate = DateTime.MinValue;

            var response = _createCaseStatusRequestValidator.Validate(badRequest);

            response.IsValid.Should().BeFalse();
            response.Errors.Should().Contain(e => e.ErrorMessage == "'startDate' must be provided.");
        }

        [Test]
        public void WhenNotesIsAbove1000CharactersReturnsErrorWithMessage()
        {
            var badRequest = CaseStatusHelper.CreateCaseStatusRequest(notes: _faker.Random.String(1001));

            var response = _createCaseStatusRequestValidator.Validate(badRequest);

            response.IsValid.Should().BeFalse();
            response.Errors.Should().Contain(e => e.ErrorMessage == "'notes' must be less than or equal to 1,000 characters.");
        }

        [Test]
        public void WhenCreatedByIsNotAnEmailAddressReturnsErrorWithMessage()
        {
            var badRequest = CaseStatusHelper.CreateCaseStatusRequest(createdBy: "foobar");

            var response = _createCaseStatusRequestValidator.Validate(badRequest);

            response.IsValid.Should().BeFalse();
            response.Errors.Should().Contain(e => e.ErrorMessage == "'createdBy' must be an email address.");
        }

        [Test]
        public void WhenRequestIsValidReturnsItIsValid()
        {
            var validRequest = CaseStatusHelper.CreateCaseStatusRequest();

            var response = _createCaseStatusRequestValidator.Validate(validRequest);

            response.IsValid.Should().BeTrue();
        }

        [Test]
        [TestCase("CP")]
        [TestCase("LAC")]
        public void WhenAnswersAreNotProvidedForTypesThatRequireThemReturnsAnErrorWithMessages(string type)
        {
            var answersWithoutValues = CaseStatusHelper.CreateCaseStatusRequestAnswers(min: 2, max: 2);

            foreach (var a in answersWithoutValues)
            {
                a.Option = "";
                a.Value = "";
            };

            var request = CaseStatusHelper.CreateCaseStatusRequest(answers: answersWithoutValues, type: type);

            var response = _createCaseStatusRequestValidator.Validate(request);

            response.IsValid.Should().BeFalse();
            response.Errors.Should().Contain(e => e.ErrorMessage == "'option' must not be empty");
            response.Errors.Should().Contain(e => e.ErrorMessage == "'value' must not be empty");
        }

        [Test]
        [TestCase(0)]
        [TestCase(2)]
        public void WhenTypeIsCPAndTheAnswerCountIsNotOneReturnsErrorWithMessage(int answerCount)
        {
            var answers = CaseStatusHelper.CreateCaseStatusRequestAnswers(min: answerCount, max: answerCount);

            var request = CaseStatusHelper.CreateCaseStatusRequest(answers: answers, type: "CP");

            var response = _createCaseStatusRequestValidator.Validate(request);

            response.IsValid.Should().BeFalse();
            response.Errors.Should().Contain(e => e.ErrorMessage == "CP type must have one answer only");
        }

        [Test]
        public void WhenTypeIsCPAndTheAnswerCountIsOneReturnsItIsValid()
        {
            var answers = CaseStatusHelper.CreateCaseStatusRequestAnswers(min: 1, max: 1);

            var request = CaseStatusHelper.CreateCaseStatusRequest(answers: answers);

            var response = _createCaseStatusRequestValidator.Validate(request);

            response.IsValid.Should().BeTrue();
        }

        [Test]
        [TestCase(1)]
        [TestCase(3)]
        public void WhenTypeIsLACAndTheAnswerCountIsNotTwoReturnsErrorWithMessage(int answerCount)
        {
            var answers = CaseStatusHelper.CreateCaseStatusRequestAnswers(min: answerCount, max: answerCount);

            var request = CaseStatusHelper.CreateCaseStatusRequest(answers: answers, type: "LAC");

            var response = _createCaseStatusRequestValidator.Validate(request);

            response.IsValid.Should().BeFalse();
            response.Errors.Should().Contain(e => e.ErrorMessage == "LAC type must have two answers only");
        }

        [Test]
        public void WhenTypeIsLACAndTheAnswerCountIsOneReturnsItIsValid()
        {
            var answers = CaseStatusHelper.CreateCaseStatusRequestAnswers(min: 2, max: 2);

            var request = CaseStatusHelper.CreateCaseStatusRequest(answers: answers);

            var response = _createCaseStatusRequestValidator.Validate(request);

            response.IsValid.Should().BeTrue();
        }

        [Test]
        public void WhenTypeIsNotCPorCINorLACReturnsErrorWithMessage()
        {
            var validRequest = CaseStatusHelper.CreateCaseStatusRequest(type: "random");

            var response = _createCaseStatusRequestValidator.Validate(validRequest);

            response.IsValid.Should().BeFalse();
            response.Errors.Should().Contain(e => e.ErrorMessage == "'type' must be CIN, CP or LAC.");
        }

        [Test]
        [TestCase("CIN")]
        [TestCase("CP")]
        [TestCase("LAC")]
        public void WhenTypeIsCPorCINorLACReturnsItIsValid(string type)
        {
            var validRequest = CaseStatusHelper.CreateCaseStatusRequest(type: type);

            if (type == "LAC")
            {
                validRequest.Answers.AddRange(CaseStatusHelper.CreateCaseStatusRequestAnswers(min: 1, max: 1));
            }

            var response = _createCaseStatusRequestValidator.Validate(validRequest);

            response.IsValid.Should().BeTrue();
        }
    }
}
