using NUnit.Framework;
using SocialCareCaseViewerApi.V1.Infrastructure;

namespace SocialCareCaseViewerApi.Tests.V1.Infrastructure
{
    [TestFixture]
    public class PhoneNumberTests
    {
        private PhoneNumber _phoneNumber;

        [SetUp]
        public void SetUp()
        {
            _phoneNumber = new PhoneNumber();
        }

        [Test]
        public void PhoneNumberHasPersonId()
        {
            Assert.AreEqual(0, _phoneNumber.PersonId);
        }
        [Test]
        public void PhoneNumberHasNumber()
        {
            Assert.IsNull(_phoneNumber.Number);
        }
        [Test]
        public void PhoneNumberHasType()
        {
            Assert.IsNull(_phoneNumber.Type);
        }
    }
}
