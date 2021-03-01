using NUnit.Framework;
using SocialCareCaseViewerApi.Tests.V1.Helpers;
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

        #region model validation
        [Test]
        [TestCase("077 1234 5678")]
        [TestCase("string")]
        public void ValidationFailsIfPhoneNumberContainsSpacesOrCharactersOtherThanNumbers(string testNumber)
        {
            PhoneNumber number = new PhoneNumber() { Number = testNumber, Type = "test type" };

            var errors = ValidationHelper.ValidateModel(number);

            Assert.AreEqual(1, errors.Count);
        }

        [Test]
        [TestCase("07712345678")]
        public void ValidationSucceedsIfPhoneNumberContainsOnlyNumbersWithoutSpaces(string testNumber)
        {
            PhoneNumber number = new PhoneNumber() { Number = testNumber, Type = "test type" };

            var errors = ValidationHelper.ValidateModel(number);

            Assert.AreEqual(0, errors.Count);
        }
        #endregion
    }
}
