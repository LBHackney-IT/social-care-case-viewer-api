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
        public void PhoneNamberIsAuditEntity()
        {
            Assert.IsInstanceOf<IAuditEntity>(_phoneNumber);
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

        #region Audit properties

        [Test]
        public void PhoneNumberHasCreatedAt()
        {
            Assert.IsNull(_phoneNumber.CreatedAt);
        }

        [Test]
        public void PhoneNumberHasCreatedBy()
        {
            Assert.IsNull(_phoneNumber.CreatedBy);
        }

        [Test]
        public void PhoneNumberHasLastUpdatedAt()
        {
            Assert.IsNull(_phoneNumber.LastModifiedAt);
        }

        [Test]
        public void PhoneNumberHasLastUpdatedBy()
        {
            Assert.IsNull(_phoneNumber.LastModifiedBy);
        }
        #endregion
    }
}
