using FluentAssertions;
using NUnit.Framework;
using SocialCareCaseViewerApi.Tests.V1.Helpers;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using System.Linq;

namespace SocialCareCaseViewerApi.Tests.V1.Boundary.Request
{
    [TestFixture]
    public class GetCaseByIdRequestTests
    {
        private GetCaseByIdRequest _getCaseByIdRequest;

        [SetUp]
        public void SetUp()
        {
            _getCaseByIdRequest = new GetCaseByIdRequest();
        }

        [Test]
        public void RequestHasRecordId()
        {
            _getCaseByIdRequest.Id.Should().Be(null);
        }

        #region Model validation
        [Test]
        public void ModelValidationFailsIfRecordIdIsNotProvided()
        {
            var errors = ValidationHelper.ValidateModel(_getCaseByIdRequest);
            errors.FirstOrDefault().ToString().Should().Be("The Id field is required.");
            errors.Should().Contain(x => x.ErrorMessage.Contains("The Id field is required."));
        }

        [Test]
        public void ModelValidationFailsIfRecordIdIsProvidedButTheStringLengthIsLessThan24()
        {
            _getCaseByIdRequest.Id = "1";
            var errors = ValidationHelper.ValidateModel(_getCaseByIdRequest);
            errors.Should().Contain(x => x.ErrorMessage.Contains("The id must be 24 characters"));
        }

        [Test]
        public void ModelValidationFailsIfRecordIdIsProvidedButTheStringLengthIsGreaterThan24()
        {
            _getCaseByIdRequest.Id = "123456789123456789123456789";
            var errors = ValidationHelper.ValidateModel(_getCaseByIdRequest);
            errors.Should().Contain(x => x.ErrorMessage.Contains("The id must be 24 characters"));
        }

        [Test]
        public void ModelValidationSucceedsIfRecordIdIsProvidedAndTheStringValueIsValid()
        {
            _getCaseByIdRequest.Id = "123456781234567812345678";
            var errors = ValidationHelper.ValidateModel(_getCaseByIdRequest);
            errors.Should().BeEmpty();
        }
        #endregion
    }
}
