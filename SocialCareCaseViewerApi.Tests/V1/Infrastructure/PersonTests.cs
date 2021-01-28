using NUnit.Framework;
using SocialCareCaseViewerApi.V1.Infrastructure;

namespace SocialCareCaseViewerApi.Tests.V1.Infrastructure
{
    [TestFixture]
    public class PersonTests
    {
        private Person _person;

        [SetUp]
        public void SetUp()
        {
            _person = new Person();
        }

        [Test]
        public void PersonHasId()
        {
            Assert.AreEqual(0, _person.Id);
        }
        [Test]
        public void PersonHasTitle()
        {
            Assert.IsNull(_person.Title);
        }
        [Test]
        public void PersonHasFirstName()
        {
            Assert.IsNull(_person.FirstName);
        }
        [Test]
        public void PersonHasLastName()
        {
            Assert.IsNull(_person.LastName);
        }
        [Test]
        public void PersonHasFullName()
        {
            Assert.IsNull(_person.FullName);
        }
        [Test]
        public void PersonHasDateOfBirth()
        {
            Assert.IsNull(_person.DateOfBirth);
        }

        [Test]
        public void PersonHasDateOfDeath()
        {
            Assert.IsNull(_person.DateOfDeath);
        }

        [Test]
        public void PersonHasEthnicity()
        {
            Assert.IsNull(_person.Ethnicity);
        }

        [Test]
        public void PersonHasFirstLanguage()
        {
            Assert.IsNull(_person.FirstLanguage);
        }

        [Test]
        public void PersonHasReligion()
        {
            Assert.IsNull(_person.Religion);
        }

        [Test]
        public void PersonHasEmailAddress()
        {
            Assert.IsNull(_person.EmailAddress);
        }

        [Test]
        public void PersonHasGender()
        {
            Assert.IsNull(_person.Gender);
        }
        [Test]
        public void PersonHasNationality()
        {
            Assert.IsNull(_person.Nationality);
        }
        [Test]
        public void PersonHasNhsNumber()
        {
            Assert.IsNull(_person.NhsNumber);
        }
        [Test]
        public void PersonHasPersonIdLegacy()
        {
            Assert.IsNull(_person.PersonIdLegacy);
        }
        [Test]
        public void PersonHasAgeContext()
        {
            Assert.IsNull(_person.AgeContext);
        }
        [Test]
        public void PersonHasDataIsFromDmPersonsBackup()
        {
            Assert.IsNull(_person.DataIsFromDmPersonsBackup);
        }

        [Test]
        public void PersonHasSexualOrientation()
        {
            Assert.IsNull(_person.SexualOrientation);
        }

        [Test]
        public void PersonHasPreferredMethodOfContact()
        {
            Assert.IsNull(_person.PreferredMethodOfContact);
        }
    }
}
