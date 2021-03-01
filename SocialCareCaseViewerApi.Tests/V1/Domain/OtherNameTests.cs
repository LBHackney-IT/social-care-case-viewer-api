using NUnit.Framework;
using SocialCareCaseViewerApi.V1.Domain;

namespace SocialCareCaseViewerApi.Tests.V1.Domain
{
    [TestFixture]
    public class OtherNameTests
    {
        private OtherName _otherName;

        [SetUp]
        public void SetUp()
        {
            _otherName = new OtherName();
        }

        [Test]
        public void OtherNameHasFirstName()
        {
            Assert.IsNull(_otherName.FirstName);
        }

        [Test]
        public void OtherNameHasLastName()
        {
            Assert.IsNull(_otherName.FirstName);
        }
    }
}
