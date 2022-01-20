using FluentAssertions;
using NUnit.Framework;
using SocialCareCaseViewerApi.Tests.V1.Helpers;
using SocialCareCaseViewerApi.V1.Boundary.Requests;

namespace SocialCareCaseViewerApi.Tests.V1.Boundary.Request
{
    [TestFixture]
    public class ListCaseNotesRequestTests
    {
        private ListCaseNotesRequest _request;

        [SetUp]
        public void SetUp()
        {
            _request = new ListCaseNotesRequest();
        }

        [Test]
        public void ValidationFailsIfPersonIdIsNotProvided()
        {
            var errors = ValidationHelper.ValidateModel(_request);

            errors.Count.Should().Be(1);
        }

        [Test]
        public void ValidationSucceedsWhenValidPersonIdIsProvided()
        {
            _request.Id = 1L;

            var errors = ValidationHelper.ValidateModel(_request);

            errors.Count.Should().Be(0);
        }
    }
}
