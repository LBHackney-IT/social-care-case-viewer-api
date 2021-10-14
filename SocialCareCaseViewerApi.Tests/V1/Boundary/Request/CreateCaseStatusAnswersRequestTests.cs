using FluentAssertions;
using NUnit.Framework;
using SocialCareCaseViewerApi.Tests.V1.Helpers;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
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
            response.Errors.Should().HaveCount(3);
            response.Errors.Should().Contain(e => e.ErrorMessage == "'caseStatusId' must be provided");
            response.Errors.Should().Contain(e => e.ErrorMessage == "'createdBy' must be provided");
            response.Errors.Should().Contain(e => e.ErrorMessage == "Answers must be provided");
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
            response.Errors.Should().Contain(e => e.ErrorMessage == "Option must not be empty");
            response.Errors.Should().Contain(e => e.ErrorMessage == "Value must not be empty");
        }

        [Test]
        public void WhenStartDateIsInTheFutureReturnsErrorWithMessage()
        {
            var badRequest = CaseStatusHelper.CreateCaseStatusAnswerRequest(startDate: System.DateTime.Today.AddDays(1));

            var response = _createCaseStatusAnswerRequestValidator.Validate(badRequest);

            response.IsValid.Should().BeFalse();
            response.Errors.Should().Contain(e => e.ErrorMessage == "'start_date' must be in the past");
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

            var response = _createCaseStatusAnswerRequestValidator.Validate(validRequest);

            response.IsValid.Should().BeTrue();
        }
    }
}
