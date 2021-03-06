using NUnit.Framework;
using SocialCareCaseViewerApi.V1.Boundary.Response;

namespace SocialCareCaseViewerApi.Tests.V1.Boundary.Response
{
    [TestFixture]
    public class AddNewResidentResponseTests
    {
        private AddNewResidentResponse _response;

        [SetUp]
        public void SetUp()
        {
            _response = new AddNewResidentResponse();
        }

        [Test]
        public void ResponseHasPersonId()
        {
            Assert.IsNull(_response.PersonId);
        }

        [Test]
        public void ResponseHasAddressId()
        {
            Assert.IsNull(_response.AddressId);
        }

        [Test]
        public void ResponseHasOtherNameIds()
        {
            Assert.IsNull(_response.OtherNameIds);
        }

        [Test]
        public void ResponseHasPhoneNumberIds()
        {
            Assert.IsNull(_response.PhoneNumberIds);
        }

        [Test]
        public void ResponseHasCaseNote()
        {
            Assert.IsNull(_response.CaseNoteId);
        }

        [Test]
        public void ResponseHasCaseNoteErrorMessage()
        {
            Assert.IsNull(_response.CaseNoteErrorMessage);
        }
    }
}
