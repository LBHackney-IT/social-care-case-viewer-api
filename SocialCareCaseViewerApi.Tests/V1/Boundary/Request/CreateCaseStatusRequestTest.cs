using Bogus;
using FluentAssertions;
using NUnit.Framework;
using SocialCareCaseViewerApi.Tests.V1.Helpers;
using SocialCareCaseViewerApi.V1.Boundary.Requests;

namespace SocialCareCaseViewerApi.Tests.V1.Boundary.Request
{
    [TestFixture]
    public class CreateCaseStatusRequestTests
    {
        CreateCaseStatusRequestValidator createCaseStatusRequestValidator;

        [SetUp]
        public void SetUp()
        {
            createCaseStatusRequestValidator = new CreateCaseStatusRequestValidator();
        }

        [Test]
        public void WhenRequestIsNullReturnsErrorsWithMessagesApartFromDetails()
        {
            var badRequest = new CreateCaseStatusRequest();
            var response = createCaseStatusRequestValidator.Validate(badRequest);

            response.IsValid.Should().BeFalse();
            response.Errors.Should().HaveCount(3);
            response.Errors.Should().Contain(e => e.ErrorMessage == "'personId' must be provided.");
            response.Errors.Should().Contain(e => e.ErrorMessage == "'type' must be provided.");
            response.Errors.Should().Contain(e => e.ErrorMessage == "'createdBy' must be provided.");
        }

        [Test]
        public void WhenStartDateIsInTheFutureReturnsErrorWithMessage()
        {
            var badRequest = CaseStatusHelper.CreateCaseStatusRequest(startDate: System.DateTime.Today.AddDays(1));

            var response = createCaseStatusRequestValidator.Validate(badRequest);

            response.IsValid.Should().BeFalse();
            response.Errors.Should().Contain(e => e.ErrorMessage == "'start_date' must be in the past");
        }


        [Test]
        public void WhenCreatedByIsNotAnEmailAddressReturnsErrorWithMessage()
        {
            var badRequest = CaseStatusHelper.CreateCaseStatusRequest(createdBy: "foobar");

            var response = createCaseStatusRequestValidator.Validate(badRequest);

            response.IsValid.Should().BeFalse();
            response.Errors.Should().Contain(e => e.ErrorMessage == "'createdBy' must be an email address.");
        }

        [Test]
        public void WhenRequestIsValidReturnsItIsValid()
        {
            var validRequest = CaseStatusHelper.CreateCaseStatusRequest();

            var response = createCaseStatusRequestValidator.Validate(validRequest);

            response.IsValid.Should().BeTrue();
        }


    }
}
