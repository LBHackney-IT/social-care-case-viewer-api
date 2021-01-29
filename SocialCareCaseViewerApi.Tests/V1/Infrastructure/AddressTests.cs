using NUnit.Framework;
using SocialCareCaseViewerApi.V1.Infrastructure;

namespace SocialCareCaseViewerApi.Tests.V1.Infrastructure
{

    [TestFixture]
    public class AddressTests
    {
        private Address _address;

        [SetUp]
        public void Setup()
        {
            _address = new Address();
        }

        [Test]
        public void AddressHasPersonAddressId()
        {
            Assert.AreEqual(0, _address.PersonAddressId);
        }

        [Test]
        public void AddressHasAddressId()
        {
            Assert.AreEqual(0, _address.AddressId);
        }

        [Test]
        public void AddressHasPerson()
        {
            Assert.IsNull(_address.Person);
        }

        [Test]
        public void AddressHasPersonId()
        {
            Assert.IsNull(_address.PersonId);
        }

        [Test]
        public void AddressHasEndDate()
        {
            Assert.IsNull(_address.EndDate);
        }

        [Test]
        public void AddressHasAddressLines()
        {
            Assert.IsNull(_address.AddressLines);
        }

        [Test]
        public void AddressHasPostCode()
        {
            Assert.IsNull(_address.PostCode);
        }

        [Test]
        public void AddressHasUprn()
        {
            Assert.IsNull(_address.Uprn);
        }

        [Test]
        public void AddressHasDataIsFromDmPersonsBackup()
        {
            Assert.IsNull(_address.DataIsFromDmPersonsBackup);
        }

        [Test]
        public void AddressHasIsDisplayAddress()
        {
            Assert.IsNull(_address.IsDisplayAddress);
        }
    }
}
