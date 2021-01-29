using NUnit.Framework;
using SocialCareCaseViewerApi.V1.Domain;

namespace SocialCareCaseViewerApi.Tests.V1.Domain
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
        public void PhoneNumberHasNumber()
        {
            Assert.IsNull(_phoneNumber.Number);
        }

        [Test]
        public void PhoneNumberHasType()
        {
            Assert.IsNull(_phoneNumber.Type);
        }

        [Test]
        public void ValidationFailsIfNumberIsNotProvided()
        {
            _ = new PhoneNumber()
            {
                Type = "Test type"
            };
        }

        [Test]
        public void ValidationFailsIfTypeIsNotProvided()
        {
            _ = new PhoneNumber()
            {
                Number = "777 6666 7777"
            };
        }

    }
}
