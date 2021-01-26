using NUnit.Framework;
using SocialCareCaseViewerApi.Tests.V1.Helpers;
using SocialCareCaseViewerApi.V1.Domain;

namespace SocialCareCaseViewerApi.Tests.V1.Domain
{
    [TestFixture]
    public class EmergencyContactDetailTests
    {
        private EmergencyContactDetail _emergencyContactDetails;

        [SetUp]
        public void SetUp()
        {
            _emergencyContactDetails = new EmergencyContactDetail();
        }

        [Test]
        public void ContactHasFirstName()
        {
            Assert.IsNull(_emergencyContactDetails.FirstName);
        }
        [Test]
        public void ContactHasLasttName()
        {
            Assert.IsNull(_emergencyContactDetails.LastName);
        }

        [Test]
        public void ContactHasPhoneNumber()
        {
            Assert.IsNull(_emergencyContactDetails.PhoneNumber);
        }

        [Test]
        public void ContactHasEmailAddress()
        {
            Assert.IsNull(_emergencyContactDetails.EmailAddress);
        }

        #region Model validation
        [Test]
        public void ModelValidationFailsIfEmailIsNotInCorrectFormat()
        {
            var errors = ValidationHelper.ValidateModel(_emergencyContactDetails);

            Assert.AreEqual(1, errors.Count);
        }
        #endregion
    }
}
