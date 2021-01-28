using NUnit.Framework;
using SocialCareCaseViewerApi.V1.Infrastructure;

namespace SocialCareCaseViewerApi.Tests.V1.Infrastructure
{
    [TestFixture]
    public class PersonOtherNameTests
    {
        private PersonOtherName _otherName;

        [SetUp]
        public void SetUp()
        {
            _otherName = new PersonOtherName();
        }

        [Test]
        public void NameHasId()
        {
            Assert.AreEqual(0, _otherName.Id);
        }

        [Test]
        public void NameHasPersonId()
        {
            Assert.AreEqual(0,_otherName.PersonId);
        }

        [Test]
        public void NameHasFirstName()
        {
            Assert.IsNull(_otherName.FirstName);
        }

        [Test]
        public void NameHasLastName()
        {
            Assert.IsNull(_otherName.LastName);
        }
    }
}
