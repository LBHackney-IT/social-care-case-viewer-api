using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoFixture;
using Bogus;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Boundary.Response;
using SocialCareCaseViewerApi.V1.Domain;
using SocialCareCaseViewerApi.V1.Gateways;
using SocialCareCaseViewerApi.V1.Infrastructure;
using Allocation = SocialCareCaseViewerApi.V1.Infrastructure.AllocationSet;
using Person = SocialCareCaseViewerApi.V1.Infrastructure.Person;
using PhoneNumber = SocialCareCaseViewerApi.V1.Domain.PhoneNumber;
using Team = SocialCareCaseViewerApi.V1.Infrastructure.Team;
using Worker = SocialCareCaseViewerApi.V1.Infrastructure.Worker;

namespace SocialCareCaseViewerApi.Tests.V1.Gateways
{
    [TestFixture]
    public class DatabaseGatewayTests : DatabaseTests
    {
        private DatabaseGateway _classUnderTest;
        private Mock<IProcessDataGateway> _mockProcessDataGateway;
        private Faker _faker;
        private Fixture _fixture;

        [SetUp]
        public void Setup()
        {
            _mockProcessDataGateway = new Mock<IProcessDataGateway>();
            _classUnderTest = new DatabaseGateway(DatabaseContext, _mockProcessDataGateway.Object);
            _faker = new Faker();
            _fixture = new Fixture();
        }

        [Test]
        public void CreatingAnAllocationShouldInsertIntoTheDatabase()
        {
            string allocatedByEmail = _faker.Internet.Email();
            string workerEmail = _faker.Internet.Email();
            string personName = $"{_faker.Name.FirstName()} {_faker.Name.LastName()}";

            Worker worker = new Worker()
            {
                Email = workerEmail,
                FirstName = _faker.Name.FirstName(),
                LastName = _faker.Name.LastName()
            };

            Worker allocatedByWorker = new Worker()
            {
                Email = allocatedByEmail,
                FirstName = _faker.Name.FirstName(),
                LastName = _faker.Name.LastName()
            };

            Person person = new Person()
            {
                FullName = personName
            };

            Team team = new Team()
            {
                Context = "A",
                Name = "Test team"
            };

            DatabaseContext.Teams.Add(team);
            DatabaseContext.Persons.Add(person);
            DatabaseContext.Workers.Add(worker);
            DatabaseContext.Workers.Add(allocatedByWorker);
            DatabaseContext.SaveChanges();

            var request = new CreateAllocationRequest()
            {
                AllocatedWorkerId = worker.Id,
                MosaicId = person.Id,
                CreatedBy = allocatedByEmail,
                AllocatedTeamId = team.Id
            };

            DatabaseContext.SaveChanges();

            //TODO: add process data gw tests
            _mockProcessDataGateway.Setup(x => x.InsertCaseNoteDocument(It.IsAny<CaseNotesDocument>())).Returns(Task.FromResult(_faker.Random.Guid().ToString()));

            var response = _classUnderTest.CreateAllocation(request);

            var query = DatabaseContext.Allocations;

            var insertedRecord = query.First(x => x.Id == response.AllocationId);

            Assert.IsNotNull(insertedRecord);
            insertedRecord.PersonId.Should().NotBe(null);
            Assert.AreEqual(insertedRecord.PersonId, request.MosaicId);
            Assert.AreEqual(insertedRecord.WorkerId, worker.Id);

            //audit properties
            Assert.AreEqual(insertedRecord.CreatedBy, allocatedByEmail);
            Assert.IsNotNull(insertedRecord.CreatedAt);
            Assert.AreNotEqual(DateTime.MinValue, insertedRecord.CreatedAt);
            Assert.IsNull(insertedRecord.LastModifiedAt);
            Assert.IsNull(insertedRecord.LastModifiedBy);

            //TODO: setup aduit tests separately
            //var auditRecord = DatabaseContext.Audits.First(x => x.KeyValues.RootElement.GetProperty("Id").GetString() == insertedRecord.Id.ToString() && x.TableName == "sccv_allocations_combined" && x.EntityState == "Added");
            //Assert.AreEqual(auditRecord.KeyValues.RootElement.GetProperty("Id").GetInt64(), response.AllocationId);

            //Assert.AreEqual(auditRecord.NewValues.RootElement.GetProperty("PersonId").GetInt64(), person.Id);
            //Assert.AreEqual(auditRecord.NewValues.RootElement.GetProperty("TeamId").GetInt64(), team.Id);
            //Assert.AreEqual(auditRecord.NewValues.RootElement.GetProperty("WorkerId").GetInt64(), worker.Id);
            //Assert.AreEqual(auditRecord.NewValues.RootElement.GetProperty("CaseStatus").GetString(), "Open");

            //Assert.IsTrue(auditRecord.NewValues.RootElement.GetProperty("CaseClosureDate").GetString() == null);
            //Assert.IsTrue(auditRecord.NewValues.RootElement.GetProperty("AllocationEndDate").GetString() == null);
            //Assert.IsNotNull(auditRecord.NewValues.RootElement.GetProperty("AllocationStartDate").GetDateTime());

            //Assert.IsTrue(auditRecord.NewValues.RootElement.GetProperty("CreatedBy").GetString() == allocatedByEmail);
            //Assert.IsNotNull(auditRecord.NewValues.RootElement.GetProperty("CreatedAt").GetDateTime());
            //Assert.IsTrue(auditRecord.NewValues.RootElement.GetProperty("LastModifiedAt").GetString() == null);
            //Assert.IsTrue(auditRecord.NewValues.RootElement.GetProperty("LastModifiedBy").GetString() == null);
        }

        [Test]
        //deallocation
        public void UpdatingAllocationShouldUpdateTheRecordInTheDatabase()
        {
            string deAllocatedByEmail = _faker.Internet.Email();
            string workerEmail = _faker.Internet.Email();
            string deallocationReason = "Test reason";
            string personName = $"{_faker.Name.FirstName()} {_faker.Name.LastName()}";

            DateTime allocationStartDate = DateTime.Now.AddDays(-60);

            //add test data
            //TODO: add helper for setting up test data
            Worker worker = new Worker()
            {
                Email = workerEmail,
                FirstName = _faker.Name.FirstName(),
                LastName = _faker.Name.LastName()
            };

            Worker deAllocatedByWorker = new Worker()
            {
                Email = deAllocatedByEmail,
                FirstName = _faker.Name.FirstName(),
                LastName = _faker.Name.LastName()
            };

            Team team = new Team()
            {
                Context = "A",
                Name = "Test team"
            };

            Person person = new Person()
            {
                FullName = personName
            };

            DatabaseContext.Workers.Add(worker);
            DatabaseContext.Workers.Add(deAllocatedByWorker);
            DatabaseContext.Persons.Add(person);
            DatabaseContext.Teams.Add(team);

            DatabaseContext.SaveChanges();

            string createdByEmail = _faker.Internet.Email();

            Allocation allocation = new Allocation()
            {
                AllocationEndDate = null,
                AllocationStartDate = allocationStartDate,
                CreatedAt = allocationStartDate,
                CaseStatus = "Open",
                CaseClosureDate = null,
                LastModifiedAt = null,
                CreatedBy = createdByEmail,
                LastModifiedBy = null,
                PersonId = person.Id,
                TeamId = team.Id,
                WorkerId = worker.Id
            };

            DatabaseContext.Add(allocation);
            DatabaseContext.SaveChanges();

            var request = new UpdateAllocationRequest()
            {
                CreatedBy = deAllocatedByEmail,
                DeallocationReason = deallocationReason,
                Id = allocation.Id
            };

            DatabaseContext.SaveChanges();

            //TODO: add process data gw tests
            _mockProcessDataGateway.Setup(x => x.InsertCaseNoteDocument(It.IsAny<CaseNotesDocument>())).Returns(Task.FromResult(_faker.Random.Guid().ToString()));

            var response = _classUnderTest.UpdateAllocation(request);

            var query = DatabaseContext.Allocations;

            var updatedRecord = query.First(x => x.Id == allocation.Id);

            Assert.IsNotNull(updatedRecord);
            Assert.IsNotNull(updatedRecord.AllocationEndDate);
            Assert.AreEqual("Closed", updatedRecord.CaseStatus);
            Assert.AreEqual(worker.Id, updatedRecord.WorkerId);
            Assert.AreEqual(team.Id, updatedRecord.TeamId);
            Assert.IsNotNull(updatedRecord.CaseClosureDate);

            //audit properties
            Assert.AreEqual(updatedRecord.CreatedBy, createdByEmail);
            Assert.IsNotNull(updatedRecord.CreatedAt);
            Assert.AreEqual(updatedRecord.CreatedAt, allocation.CreatedAt);
            Assert.IsNotNull(updatedRecord.LastModifiedAt);
            Assert.AreEqual(updatedRecord.LastModifiedBy, deAllocatedByEmail);

            //audit record
            //var auditRecord = DatabaseContext.Audits.First(x => x.KeyValues.RootElement.GetProperty("Id").GetString() == updatedRecord.Id.ToString() && x.EntityState == "Modified");

            ////key value
            //Assert.AreEqual(auditRecord.KeyValues.RootElement.GetProperty("Id").GetInt64(), allocation.Id);

            ////old values
            //Assert.AreEqual(auditRecord.OldValues.RootElement.GetProperty("CaseStatus").GetString(), "Open");
            //Assert.IsTrue(auditRecord.OldValues.RootElement.GetProperty("LastModifiedBy").GetString() == null);
            //Assert.IsTrue(auditRecord.OldValues.RootElement.GetProperty("CaseClosureDate").GetString() == null);
            //Assert.IsTrue(auditRecord.OldValues.RootElement.GetProperty("AllocationEndDate").GetString() == null);

            ////new values
            //Assert.AreEqual(auditRecord.NewValues.RootElement.GetProperty("CaseStatus").GetString(), "Closed");
            //Assert.IsTrue(auditRecord.NewValues.RootElement.GetProperty("LastModifiedBy").GetString() == deAllocatedByEmail);
            //Assert.IsNotNull(auditRecord.NewValues.RootElement.GetProperty("CaseClosureDate").GetDateTime());
            //Assert.IsNotNull(auditRecord.NewValues.RootElement.GetProperty("AllocationEndDate").GetDateTime());
        }

        [Test]
        public void CreatingAPersonShouldCreateCorrectRecordsAndAuditRecordsInTheDatabase()
        {
            //TODO: clean up before each test
            DatabaseContext.Audits.RemoveRange(DatabaseContext.Audits);
            DatabaseContext.Persons.RemoveRange(DatabaseContext.Persons);
            DatabaseContext.PersonOtherNames.RemoveRange(DatabaseContext.PersonOtherNames);
            DatabaseContext.Addresses.RemoveRange(DatabaseContext.Addresses);
            DatabaseContext.PhoneNumbers.RemoveRange(DatabaseContext.PhoneNumbers);

            string title = "Mr";
            string firstName = _faker.Name.FirstName();
            string lastName = _faker.Name.LastName();

            string otherNameFirstOne = _faker.Name.FirstName();
            string otherNameLastOne = _faker.Name.LastName();

            string otherNameFirstTwo = _faker.Name.FirstName();
            string otherNameLastTwo = _faker.Name.LastName();

            string gender = "M";

            OtherName otherNameOne = new OtherName() { FirstName = otherNameFirstOne, LastName = otherNameLastOne };
            OtherName otherNameTwo = new OtherName() { FirstName = otherNameFirstTwo, LastName = otherNameLastTwo };

            List<OtherName> otherNames = new List<OtherName>()
            {
                otherNameOne,
                otherNameTwo
            };

            DateTime dateOfBirth = DateTime.Now.AddYears(-70);
            DateTime dateOfDeath = DateTime.Now.AddDays(-10);

            string ethnicity = "A.A1"; //TODO test for valid values
            string firstLanguage = "English";
            string religion = _faker.Random.Word();
            string sexualOrientation = _faker.Random.Word();
            long nhsNumber = _faker.Random.Number();

            string addressLine = "1 Test Street";
            long uprn = 1234567;
            string postCode = "E18";
            string isDiplayAddress = "Y";
            string dataIsFromDmPersonsBackup = "N";

            AddressDomain address = new AddressDomain()
            {
                Address = addressLine,
                Postcode = postCode,
                Uprn = uprn
            };

            string phoneNumberOne = "07755555555";
            string phoneNumberTypeOne = "Mobile";

            string phoneNumberTwo = "07977777777";
            string phoneNumberTypeTwo = "Fax";

            List<PhoneNumber> phoneNumbers = new List<PhoneNumber>()
            {
                new PhoneNumber() { Number = phoneNumberOne, Type = phoneNumberTypeOne},
                new PhoneNumber() { Number = phoneNumberTwo, Type = phoneNumberTypeTwo}
            };

            string emailAddress = _faker.Internet.Email();
            string preferredMethodOfContact = _faker.Random.Word();
            string contextFlag = "A"; //TODO test for valid values
            string createdBy = _faker.Internet.Email();

            AddNewResidentRequest request = new AddNewResidentRequest()
            {
                Title = title,
                FirstName = firstName,
                LastName = lastName,
                OtherNames = otherNames,
                Gender = gender,
                DateOfBirth = dateOfBirth,
                DateOfDeath = dateOfDeath,
                Ethnicity = ethnicity,
                FirstLanguage = firstLanguage,
                Religion = religion,
                SexualOrientation = sexualOrientation,
                NhsNumber = nhsNumber,
                Address = address,
                PhoneNumbers = phoneNumbers,
                EmailAddress = emailAddress,
                PreferredMethodOfContact = preferredMethodOfContact,
                ContextFlag = contextFlag,
                CreatedBy = createdBy
            };

            AddNewResidentResponse response = _classUnderTest.AddNewResident(request);

            Assert.IsNotNull(response.PersonId);

            Assert.IsNotNull(response.OtherNameIds);
            Assert.AreEqual(2, response.OtherNameIds.Count);

            Assert.IsNotNull(response.AddressId);

            Assert.IsNotNull(response.PhoneNumberIds);
            Assert.AreEqual(2, response.PhoneNumberIds.Count);

            //check that person was created with correct values
            var person = DatabaseContext.Persons.FirstOrDefault(x => x.Id == response.PersonId);

            Assert.IsNotNull(person);

            Assert.AreEqual(title, person.Title);
            Assert.AreEqual(firstName, person.FirstName);
            Assert.AreEqual(lastName, person.LastName);
            Assert.AreEqual(gender, person.Gender);
            Assert.AreEqual(dateOfBirth, person.DateOfBirth);
            Assert.AreEqual(dateOfDeath, person.DateOfDeath);
            Assert.AreEqual(ethnicity, person.Ethnicity);
            Assert.AreEqual(firstLanguage, person.FirstLanguage);
            Assert.AreEqual(religion, person.Religion);
            Assert.AreEqual(sexualOrientation, person.SexualOrientation);
            Assert.AreEqual(nhsNumber, person.NhsNumber);
            Assert.AreEqual(emailAddress, person.EmailAddress);
            Assert.AreEqual(preferredMethodOfContact, person.PreferredMethodOfContact);
            Assert.AreEqual(contextFlag, person.AgeContext);

            //check that othernames were created with correct values
            Assert.AreEqual(2, person.OtherNames.Count);
            Assert.IsTrue(person.OtherNames.Any(x => x.FirstName == otherNameFirstOne));
            Assert.IsTrue(person.OtherNames.Any(x => x.LastName == otherNameLastOne));
            Assert.IsTrue(person.OtherNames.Any(x => x.FirstName == otherNameFirstTwo));
            Assert.IsTrue(person.OtherNames.Any(x => x.LastName == otherNameLastTwo));
            Assert.IsTrue(person.OtherNames.All(x => x.PersonId == person.Id));

            //check that address was created with correct values
            Assert.IsNotNull(person.Addresses);
            Assert.AreEqual(addressLine, person.Addresses.First().AddressLines);
            Assert.AreEqual(postCode, person.Addresses.First().PostCode);
            Assert.AreEqual(uprn, person.Addresses.First().Uprn);
            Assert.AreEqual(isDiplayAddress, person.Addresses.First().IsDisplayAddress);
            Assert.AreEqual(dataIsFromDmPersonsBackup, person.Addresses.First().DataIsFromDmPersonsBackup);

            //check that phone numbers were created with correct values
            Assert.AreEqual(2, person.PhoneNumbers.Count);
            Assert.IsTrue(person.PhoneNumbers.Any(x => x.Number == phoneNumberOne.ToString()));
            Assert.IsTrue(person.PhoneNumbers.Any(x => x.Type == phoneNumberTypeOne));
            Assert.IsTrue(person.PhoneNumbers.Any(x => x.Number == phoneNumberTwo.ToString()));
            Assert.IsTrue(person.PhoneNumbers.Any(x => x.Type == phoneNumberTypeTwo));
            Assert.IsTrue(person.PhoneNumbers.All(x => x.PersonId == person.Id));

            //audit details
            Assert.AreEqual(createdBy, person.CreatedBy);
            Assert.IsNotNull(person.CreatedAt);

            Assert.IsTrue(person.Addresses.All(x => x.CreatedBy == createdBy));
            Assert.IsTrue(person.Addresses.All(x => x.CreatedBy != null));

            Assert.IsTrue(person.OtherNames.All(x => x.CreatedBy == createdBy));
            Assert.IsTrue(person.OtherNames.All(x => x.CreatedBy != null));

            Assert.IsTrue(person.PhoneNumbers.All(x => x.CreatedBy == createdBy));
            Assert.IsTrue(person.PhoneNumbers.All(x => x.CreatedBy != null));

            //TODO: create separate test setup for audit
            //////person audit record
            //var personRecord = DatabaseContext.Audits.First(x => x.KeyValues.RootElement.GetProperty("Id").GetInt64() == person.Id && x.EntityState == "Added" && x.TableName == "dm_persons");
            //Assert.IsNotNull(personRecord.KeyValues.RootElement.GetProperty("Id").GetInt64());

            //Assert.IsTrue(personRecord.NewValues.RootElement.GetProperty("Title").GetString() == person.Title);
            //Assert.IsTrue(personRecord.NewValues.RootElement.GetProperty("FirstName").GetString() == person.FirstName);
            //Assert.IsTrue(personRecord.NewValues.RootElement.GetProperty("LastName").GetString() == person.LastName);
            //Assert.IsTrue(personRecord.NewValues.RootElement.GetProperty("Gender").GetString() == person.Gender);
            //Assert.IsTrue(personRecord.NewValues.RootElement.GetProperty("DateOfBirth").GetDateTime() == person.DateOfBirth);
            //Assert.IsTrue(personRecord.NewValues.RootElement.GetProperty("DateOfDeath").GetDateTime() == person.DateOfDeath);
            //Assert.IsTrue(personRecord.NewValues.RootElement.GetProperty("Ethnicity").GetString() == person.Ethnicity);
            //Assert.IsTrue(personRecord.NewValues.RootElement.GetProperty("FirstLanguage").GetString() == person.FirstLanguage);
            //Assert.IsTrue(personRecord.NewValues.RootElement.GetProperty("Religion").GetString() == person.Religion);
            //Assert.IsTrue(personRecord.NewValues.RootElement.GetProperty("SexualOrientation").GetString() == person.SexualOrientation);
            //Assert.IsTrue(personRecord.NewValues.RootElement.GetProperty("NhsNumber").GetInt64() == person.NhsNumber);
            //Assert.IsTrue(personRecord.NewValues.RootElement.GetProperty("EmailAddress").GetString() == person.EmailAddress);
            //Assert.IsTrue(personRecord.NewValues.RootElement.GetProperty("PreferredMethodOfContact").GetString() == person.PreferredMethodOfContact);
            //Assert.IsTrue(personRecord.NewValues.RootElement.GetProperty("AgeContext").GetString() == person.AgeContext);
            //Assert.IsTrue(personRecord.NewValues.RootElement.GetProperty("CreatedBy").GetString() == person.CreatedBy);
            //Assert.IsNotNull(personRecord.NewValues.RootElement.GetProperty("CreatedAt").GetDateTime());

            ////address record
            //var addressRecord = DatabaseContext.Audits.First(x => x.TableName == "dm_addresses" && x.NewValues.RootElement.GetProperty("PersonId").GetInt64() == person.Id && x.EntityState == "Added");
            //Assert.IsNotNull(addressRecord.KeyValues.RootElement.GetProperty("PersonAddressId").GetInt64());

            //Assert.IsTrue(addressRecord.NewValues.RootElement.GetProperty("Uprn").GetInt64() == person.Addresses.First().Uprn);
            //Assert.IsTrue(addressRecord.NewValues.RootElement.GetProperty("EndDate").GetString() == null);
            //Assert.IsTrue(addressRecord.NewValues.RootElement.GetProperty("PersonId").GetInt64() == person.Addresses.First().PersonId);
            //Assert.IsTrue(addressRecord.NewValues.RootElement.GetProperty("PostCode").GetString() == person.Addresses.First().PostCode);
            //Assert.IsTrue(personRecord.NewValues.RootElement.GetProperty("CreatedBy").GetString() == person.CreatedBy);
            //Assert.IsNotNull(personRecord.NewValues.RootElement.GetProperty("CreatedAt").GetDateTime());
            //Assert.IsTrue(addressRecord.NewValues.RootElement.GetProperty("AddressLines").GetString() == person.Addresses.First().AddressLines);
            //Assert.IsTrue(addressRecord.NewValues.RootElement.GetProperty("LastModifiedAt").GetString() == null);
            //Assert.IsTrue(addressRecord.NewValues.RootElement.GetProperty("LastModifiedBy").GetString() == null);
            //Assert.IsTrue(addressRecord.NewValues.RootElement.GetProperty("IsDisplayAddress").GetString() == person.Addresses.First().IsDisplayAddress);
            //Assert.IsTrue(addressRecord.NewValues.RootElement.GetProperty("DataIsFromDmPersonsBackup").GetString() == person.Addresses.First().DataIsFromDmPersonsBackup);

            ////other names
            //var otherNamesRecordOne = DatabaseContext.Audits.First(x => x.TableName == "sccv_person_other_name" && x.NewValues.RootElement.GetProperty("FirstName").GetString() == otherNameFirstOne && x.EntityState == "Added");
            //Assert.IsNotNull(otherNamesRecordOne.KeyValues.RootElement.GetProperty("Id").GetInt64());

            //Assert.AreEqual(otherNamesRecordOne.NewValues.RootElement.GetProperty("FirstName").GetString(), otherNameFirstOne);
            //Assert.AreEqual(otherNamesRecordOne.NewValues.RootElement.GetProperty("LastName").GetString(), otherNameLastOne);
            //Assert.AreEqual(otherNamesRecordOne.NewValues.RootElement.GetProperty("PersonId").GetInt64(), person.Id);
            //Assert.IsTrue(personRecord.NewValues.RootElement.GetProperty("CreatedBy").GetString() == person.CreatedBy);
            //Assert.IsNotNull(personRecord.NewValues.RootElement.GetProperty("CreatedAt").GetDateTime());
            //Assert.IsTrue(addressRecord.NewValues.RootElement.GetProperty("LastModifiedAt").GetString() == null);
            //Assert.IsTrue(addressRecord.NewValues.RootElement.GetProperty("LastModifiedBy").GetString() == null);

            //var otherNamesRecordTwo = DatabaseContext.Audits.First(x => x.TableName == "sccv_person_other_name" && x.NewValues.RootElement.GetProperty("FirstName").GetString() == otherNameFirstTwo && x.EntityState == "Added");
            //Assert.IsNotNull(otherNamesRecordTwo.KeyValues.RootElement.GetProperty("Id").GetInt64());

            //Assert.AreEqual(otherNamesRecordTwo.NewValues.RootElement.GetProperty("FirstName").GetString(), otherNameFirstTwo);
            //Assert.AreEqual(otherNamesRecordTwo.NewValues.RootElement.GetProperty("LastName").GetString(), otherNameLastTwo);
            //Assert.AreEqual(otherNamesRecordTwo.NewValues.RootElement.GetProperty("PersonId").GetInt64(), person.Id);
            //Assert.IsTrue(personRecord.NewValues.RootElement.GetProperty("CreatedBy").GetString() == person.CreatedBy);
            //Assert.IsNotNull(personRecord.NewValues.RootElement.GetProperty("CreatedAt").GetDateTime());
            //Assert.IsTrue(addressRecord.NewValues.RootElement.GetProperty("LastModifiedAt").GetString() == null);
            //Assert.IsTrue(addressRecord.NewValues.RootElement.GetProperty("LastModifiedBy").GetString() == null);

            ////phone numbers
            //var phoneNumberRecordOne = DatabaseContext.Audits.First(x => x.TableName == "dm_telephone_numbers" && x.NewValues.RootElement.GetProperty("Number").GetString() == phoneNumberOne.ToString() && x.EntityState == "Added");

            //Assert.AreEqual(phoneNumberRecordOne.NewValues.RootElement.GetProperty("Type").GetString(), phoneNumberTypeOne);
            //Assert.AreEqual(phoneNumberRecordOne.NewValues.RootElement.GetProperty("Number").GetString(), phoneNumberOne.ToString());
            //Assert.AreEqual(phoneNumberRecordOne.NewValues.RootElement.GetProperty("PersonId").GetInt64(), person.Id);

            //Assert.IsTrue(phoneNumberRecordOne.NewValues.RootElement.GetProperty("CreatedBy").GetString() == person.CreatedBy);
            //Assert.IsNotNull(phoneNumberRecordOne.NewValues.RootElement.GetProperty("CreatedAt").GetDateTime());
            //Assert.IsTrue(phoneNumberRecordOne.NewValues.RootElement.GetProperty("LastModifiedAt").GetString() == null);
            //Assert.IsTrue(phoneNumberRecordOne.NewValues.RootElement.GetProperty("LastModifiedBy").GetString() == null);

            //var phoneNumberRecordTwo = DatabaseContext.Audits.First(x => x.TableName == "dm_telephone_numbers" && x.NewValues.RootElement.GetProperty("Number").GetString() == phoneNumberTwo.ToString() && x.EntityState == "Added");

            //Assert.AreEqual(phoneNumberRecordTwo.NewValues.RootElement.GetProperty("Type").GetString(), phoneNumberTypeTwo);
            //Assert.AreEqual(phoneNumberRecordTwo.NewValues.RootElement.GetProperty("Number").GetString(), phoneNumberTwo.ToString());
            //Assert.AreEqual(phoneNumberRecordTwo.NewValues.RootElement.GetProperty("PersonId").GetInt64(), person.Id);

            //Assert.IsTrue(phoneNumberRecordTwo.NewValues.RootElement.GetProperty("CreatedBy").GetString() == person.CreatedBy);
            //Assert.IsNotNull(phoneNumberRecordTwo.NewValues.RootElement.GetProperty("CreatedAt").GetDateTime());
            //Assert.IsTrue(phoneNumberRecordTwo.NewValues.RootElement.GetProperty("LastModifiedAt").GetString() == null);
            //Assert.IsTrue(phoneNumberRecordTwo.NewValues.RootElement.GetProperty("LastModifiedBy").GetString() == null);
        }
    }
}

