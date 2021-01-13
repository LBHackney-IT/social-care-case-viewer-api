using System.Linq;
using NUnit.Framework;
using SocialCareCaseViewerApi.Tests.V1.Helpers;
using SocialCareCaseViewerApi.V1.Boundary.Requests

namespace SocialCareCaseViewerApi.Tests.V1.Boundary.Request
{
    [TestFixture]
    public class ListTeamsRequestTests
    {
        private ListTeamsRequest _listTeamsRequest;

        [SetUp]
        public void SetUp()
        {
            _listTeamsRequest = new ListTeamsRequest();
        }

        [Test]
        public void RequestHasContextFlag()
        {
            Assert.AreEqual(null, _listTeamsRequest.ContextFlag);
        }

        #region Model validation
        [Test]
        public void ModelValidationFailsIfContextIsNotProvided() //TODO: move message check to separate tests
        {
            ListTeamsRequest request = new ListTeamsRequest();
            var errors = ValidationHelper.ValidateModel(request);
            Assert.IsTrue(errors.Any(x => x.ErrorMessage.Contains("ContextFlag field is required")));
            Assert.IsTrue(errors.Count == 1);
        }

        [Test]
        public void ModelValidationFailsIfContextIsProvidedButTheValueIsLongerThanOne()
        {
            ListTeamsRequest request = new ListTeamsRequest() { ContextFlag = "random" };
            var errors = ValidationHelper.ValidateModel(request);
            Assert.IsTrue(errors.Any(x => x.ErrorMessage.Contains("The context_flag must be 1 character")));
        }

        [Test]
        public void ModelValidationFailsIfContextIsProvidedButTheValueIsNotEitherAorC()
        {
            ListTeamsRequest request = new ListTeamsRequest() { ContextFlag = "d" };
            var errors = ValidationHelper.ValidateModel(request);
            Assert.IsTrue(errors.Count == 1);
            Assert.IsTrue(errors.Any(x => x.ErrorMessage.Contains("The context_flag must be either 'A' or 'C' only")));
        }

        [Test]
        [TestCase("A")]
        [TestCase("C")]
        public void ModelValidationSucceedsIfContextIsProvidedAndTheValueIsValid(string context)
        {
            ListTeamsRequest request = new ListTeamsRequest() { ContextFlag = context };
            var errors = ValidationHelper.ValidateModel(request);
            Assert.IsTrue(errors.Count == 0);
        }

        [Test]
        [TestCase("a")]
        [TestCase("c")]
        public void ModelValidationSucceedsIfContextIsProvidedAndTheValueIsValidIgnoringCase(string context)
        {
            ListTeamsRequest request = new ListTeamsRequest() { ContextFlag = context };
            var errors = ValidationHelper.ValidateModel(request);
            Assert.IsTrue(errors.Count == 0);
        }
        #endregion
    }
}
