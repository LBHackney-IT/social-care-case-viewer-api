using System.Linq;
using NUnit.Framework;
using SocialCareCaseViewerApi.Tests.V1.Helpers;
using SocialCareCaseViewerApi.V1.Boundary.Requests;

namespace SocialCareCaseViewerApi.Tests.V1.Boundary.Request
{
    [TestFixture]
    public class ListTeamsRequestTests
    {
        private GetTeamsRequest _getTeamsRequest;

        [SetUp]
        public void SetUp()
        {
            _getTeamsRequest = new GetTeamsRequest();
        }

        [Test]
        public void RequestHasContextFlag()
        {
            Assert.AreEqual(null, _getTeamsRequest.ContextFlag);
        }

        #region Model validation
        [Test]
        public void ModelValidationFailsIfContextIsNotProvided() //TODO: move message check to separate tests
        {
            GetTeamsRequest request = new GetTeamsRequest();
            var errors = ValidationHelper.ValidateModel(request);
            Assert.IsTrue(errors.Any(x => x.ErrorMessage.Contains("ContextFlag field is required")));
            Assert.IsTrue(errors.Count == 1);
        }

        [Test]
        public void ModelValidationFailsIfContextIsProvidedButTheValueIsLongerThanOne()
        {
            GetTeamsRequest request = new GetTeamsRequest() { ContextFlag = "random" };
            var errors = ValidationHelper.ValidateModel(request);
            Assert.IsTrue(errors.Any(x => x.ErrorMessage.Contains("The context_flag must be 1 character")));
        }

        [Test]
        public void ModelValidationFailsIfContextIsProvidedButTheValueIsNotEitherAorC()
        {
            GetTeamsRequest request = new GetTeamsRequest() { ContextFlag = "d" };
            var errors = ValidationHelper.ValidateModel(request);
            Assert.IsTrue(errors.Count == 1);
            Assert.IsTrue(errors.Any(x => x.ErrorMessage.Contains("The context_flag must be 'A', 'B' or 'C' only")));
        }

        [Test]
        [TestCase("A")]
        [TestCase("B")]
        [TestCase("C")]
        public void ModelValidationSucceedsIfContextIsProvidedAndTheValueIsValid(string context)
        {
            GetTeamsRequest request = new GetTeamsRequest() { ContextFlag = context };
            var errors = ValidationHelper.ValidateModel(request);
            Assert.IsTrue(errors.Count == 0);
        }

        [Test]
        [TestCase("a")]
        [TestCase("b")]
        [TestCase("c")]
        public void ModelValidationSucceedsIfContextIsProvidedAndTheValueIsValidIgnoringCase(string context)
        {
            GetTeamsRequest request = new GetTeamsRequest() { ContextFlag = context };
            var errors = ValidationHelper.ValidateModel(request);
            Assert.IsTrue(errors.Count == 0);
        }
        #endregion
    }
}
