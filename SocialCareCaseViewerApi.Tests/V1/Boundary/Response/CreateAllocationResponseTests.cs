using NUnit.Framework;
using SocialCareCaseViewerApi.V1.Boundary.Response;

namespace SocialCareCaseViewerApi.Tests.V1.Boundary.Response
{
    [TestFixture]
    public class CreateAllocationResponseTests
    {
        private CreateAllocationResponse _createAllocationResponse;

        [SetUp]
        public void SetUp()
        {
            _createAllocationResponse = new CreateAllocationResponse();
        }

        [Test]
        public void ResponseHasCaseNoteId()
        {
            Assert.AreEqual(null, _createAllocationResponse.CaseNoteId);
        }
    }
}
