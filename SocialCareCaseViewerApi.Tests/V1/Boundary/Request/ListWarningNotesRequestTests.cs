using FluentAssertions;
using NUnit.Framework;
using SocialCareCaseViewerApi.Tests.V1.Helpers;
using SocialCareCaseViewerApi.V1.Boundary.Requests;

namespace SocialCareCaseViewerApi.Tests.V1.Boundary.Request
{
    [TestFixture]
    public class ListWarningNotesRequestTests
    {
        private ListWarningNotesRequest _classUnderTest;

        [SetUp]
        public void SetUp()
        {
            _classUnderTest = new ListWarningNotesRequest();
        }

        [Test]
        public void ListWarningNotesRequestShouldHavePersonId()
        {
            _classUnderTest.PersonId.Should().Be(0);
        }

        #region Model validation
        [Test]
        public void ValidationFailsWhenPersonIdIsNotProvided()
        {
            var errors = ValidationHelper.ValidateModel(_classUnderTest);

            errors.Count.Should().Be(1);
            errors.Should().Contain(x => x.ErrorMessage.Contains("Please provide person_id"));
        }

        [Test]
        public void ValidationPassesWhenPersonIdIsProvided()
        {
            _classUnderTest.PersonId = 1234;
            var errors = ValidationHelper.ValidateModel(_classUnderTest);

            errors.Count.Should().Be(0);
        }
        #endregion
    }
}