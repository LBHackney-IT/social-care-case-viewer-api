using NUnit.Framework;
using SocialCareCaseViewerApi.V1.Boundary.Response;

namespace SocialCareCaseViewerApi.Tests.V1.Boundary.Response
{
    [TestFixture]
    public class UpdateAllocationResponseTests
    {
        private UpdateAllocationResponse _updateAllocationResponse;

        [SetUp]
        public void SetUp()
        {
            _updateAllocationResponse = new UpdateAllocationResponse();
        }

        [Test]
        public void ResponseHasCaseNoteId()
        {
            Assert.AreEqual(null, _updateAllocationResponse.CaseNoteId);
        }
    }
}
