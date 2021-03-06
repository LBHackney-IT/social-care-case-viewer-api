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
        public void NameIsAuditEntity()
        {
            Assert.IsInstanceOf<IAuditEntity>(_otherName);
        }

        [Test]
        public void NameHasId()
        {
            Assert.AreEqual(0, _otherName.Id);
        }

        [Test]
        public void NameHasPersonId()
        {
            Assert.AreEqual(0, _otherName.PersonId);
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

        #region Audit properties

        [Test]
        public void NameHasCreatedAt()
        {
            Assert.IsNull(_otherName.CreatedAt);
        }

        [Test]
        public void NameHasCreatedBy()
        {
            Assert.IsNull(_otherName.CreatedBy);
        }

        [Test]
        public void NameHasLastUpdatedAt()
        {
            Assert.IsNull(_otherName.LastModifiedAt);
        }

        [Test]
        public void NameHasLastUpdatedBy()
        {
            Assert.IsNull(_otherName.LastModifiedBy);
        }
        #endregion
    }
}
