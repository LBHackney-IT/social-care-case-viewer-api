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
    public class CreateCaseStatusAnswersRequestTests
    {
        CreateCaseStatusAnswerRequestValidator _createCaseStatusAnswerRequestValidator = null!;

        [SetUp]
        public void SetUp()
        {
            _createCaseStatusAnswerRequestValidator = new CreateCaseStatusAnswerRequestValidator();
        }

        [Test]
        public void WhenRequestIsNullReturnsErrorsWithMessages()
        {
            var badRequest = new CreateCaseStatusAnswerRequest();
            var response = _createCaseStatusAnswerRequestValidator.Validate(badRequest);

            response.IsValid.Should().BeFalse();
            response.Errors.Should().HaveCount(6);
            response.Errors.Should().Contain(e => e.ErrorMessage == "'caseStatusId' must be provided");
            response.Errors.Should().Contain(e => e.ErrorMessage == "'createdBy' must be provided");
            response.Errors.Should().Contain(e => e.ErrorMessage == "'answers' must be provided");
            response.Errors.Should().Contain(e => e.ErrorMessage == "Provide two answers only");
            response.Errors.Should().Contain(e => e.ErrorMessage == "'startDate' must be provided");
            response.Errors.Should().Contain(e => e.ErrorMessage == "'startDate' must have a valid value");
        }

        [Test]
        public void WhenAnswersAreNotProvidedReturnsAnErrorWithMessages()
        {
            var requestWithoutAnswers = CaseStatusHelper
                .CreateCaseStatusAnswerRequest(answers:
                    new List<CaseStatusRequestAnswers>() {
                        new CaseStatusRequestAnswers() {
                            Option = "", Value = ""
                        }
                    }
                );

            var response = _createCaseStatusAnswerRequestValidator.Validate(requestWithoutAnswers);

            response.IsValid.Should().BeFalse();
            response.Errors.Should().Contain(e => e.ErrorMessage == "'option' must not be empty");
            response.Errors.Should().Contain(e => e.ErrorMessage == "'value' must not be empty");
        }

        [Test]
        [TestCase(1)]
        [TestCase(3)]
        public void WhenAnswerCountIsNotTwoReturnsErrorWithMessage(int answerCount)
        {
            var answers = CaseStatusHelper.CreateCaseStatusRequestAnswers(min: answerCount, max: answerCount);

            var request = CaseStatusHelper.CreateCaseStatusAnswerRequest(answers: answers);

            var response = _createCaseStatusAnswerRequestValidator.Validate(request);

            response.IsValid.Should().BeFalse();
            response.Errors.Should().Contain(e => e.ErrorMessage == "Provide two answers only");
        }

        [Test]
        public void WhenAnswerCountIsTwoReturnsItIsValid()
        {
            var answers = CaseStatusHelper.CreateCaseStatusRequestAnswers(min: 2, max: 2);

            var request = CaseStatusHelper.CreateCaseStatusAnswerRequest(answers: answers);

            var response = _createCaseStatusAnswerRequestValidator.Validate(request);

            response.IsValid.Should().BeTrue();
        }

        [Test]
        public void WhenStartDateIsDefaultDateTimeValueReturnsErrorWithMessage()
        {
            var badRequest = CaseStatusHelper.CreateCaseStatusAnswerRequest(startDate: DateTime.MinValue);

            var response = _createCaseStatusAnswerRequestValidator.Validate(badRequest);

            response.IsValid.Should().BeFalse();
            response.Errors.Should().Contain(e => e.ErrorMessage == "'startDate' must have a valid value");
        }

        [Test]
        public void WhenCreatedByIsNotAnEmailAddressReturnsErrorWithMessage()
        {
            var badRequest = CaseStatusHelper.CreateCaseStatusAnswerRequest(createdBy: "foobar");

            var response = _createCaseStatusAnswerRequestValidator.Validate(badRequest);

            response.IsValid.Should().BeFalse();
            response.Errors.Should().Contain(e => e.ErrorMessage == "'createdBy' must be an email address");
        }

        [Test]
        public void WhenRequestIsValidReturnsIsValid()
        {
            var validRequest = CaseStatusHelper.CreateCaseStatusAnswerRequest();

            validRequest.Answers.Add(new CaseStatusRequestAnswers() { Option = "legalStatus", Value = "L1" });

            var response = _createCaseStatusAnswerRequestValidator.Validate(validRequest);

            response.IsValid.Should().BeTrue();
        }
    }
}
