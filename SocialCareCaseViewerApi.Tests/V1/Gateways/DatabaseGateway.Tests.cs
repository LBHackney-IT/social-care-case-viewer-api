using System;
using System.Collections.Generic;
using System.Linq;
using AutoFixture;
using Bogus;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SocialCareCaseViewerApi.Tests.V1.Helpers;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Boundary.Response;
using SocialCareCaseViewerApi.V1.Domain;
using SocialCareCaseViewerApi.V1.Exceptions;
using SocialCareCaseViewerApi.V1.Gateways;
using SocialCareCaseViewerApi.V1.Infrastructure;
using Allocation = SocialCareCaseViewerApi.V1.Infrastructure.AllocationSet;
using Person = SocialCareCaseViewerApi.V1.Infrastructure.Person;
using PhoneNumber = SocialCareCaseViewerApi.V1.Domain.PhoneNumber;
using PhoneNumberInfrastructure = SocialCareCaseViewerApi.V1.Infrastructure.PhoneNumber;
using Team = SocialCareCaseViewerApi.V1.Infrastructure.Team;
using WarningNote = SocialCareCaseViewerApi.V1.Infrastructure.WarningNote;
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
        public void GetWorkerByWorkerIdReturnsWorker()
        {
            var worker = SaveWorkerToDatabase(DatabaseGatewayHelper.CreateWorkerDatabaseEntity());

            var response = _classUnderTest.GetWorkerByWorkerId(worker.Id);

            response.Should().BeEquivalentTo(worker);
        }

        [Test]
        public void GetWorkerByWorkerIdReturnsNullWhenIdNotPresent()
        {
            const int workerId = 123;
            const int nonExistentWorkerId = 321;

            SaveWorkerToDatabase(DatabaseGatewayHelper.CreateWorkerDatabaseEntity(id: workerId));
            var response = _classUnderTest.GetWorkerByWorkerId(nonExistentWorkerId);

            response.Should().BeNull();
        }

        [Test]
        public void GetWorkerByWorkerEmailReturnsWorker()
        {
            var worker = SaveWorkerToDatabase(DatabaseGatewayHelper.CreateWorkerDatabaseEntity());
            var response = _classUnderTest.GetWorkerByEmail(worker.Email);

            response.Should().BeEquivalentTo(worker);
        }

        [Test]
        public void GetWorkerByWorkerEmailReturnsNullWhenEmailNotPresent()
        {
            const string workerEmail = "realEmail@example.com";
            const string nonExistentWorkerEmail = "nonExistentEmail@example.com";

            SaveWorkerToDatabase(DatabaseGatewayHelper.CreateWorkerDatabaseEntity(email: workerEmail));
            var response = _classUnderTest.GetWorkerByEmail(nonExistentWorkerEmail);

            response.Should().BeNull();
        }

        [Test]
        public void GetTeamByTeamIdReturnsListOfTeamsWithWorkers()
        {
            var team = SaveTeamToDatabase(DatabaseGatewayHelper.CreateTeamDatabaseEntity(workerTeams: new List<WorkerTeam>()));

            var response = _classUnderTest.GetTeamsByTeamId(team.Id);

            response.Should().BeEquivalentTo(new List<Team> { team });
        }

        [Test]
        public void GetTeamByTeamIdAndGetAssociatedWorkers()
        {
            var workerOne = SaveWorkerToDatabase(DatabaseGatewayHelper.CreateWorkerDatabaseEntity(id: 1, email: "worker-one-test-email@example.com"));
            var workerTwo = SaveWorkerToDatabase(DatabaseGatewayHelper.CreateWorkerDatabaseEntity(id: 2, email: "worker-two-test-email@example.com"));
            var workerTeamOne =
                SaveWorkerTeamToDatabase(DatabaseGatewayHelper.CreateWorkerTeamDatabaseEntity(id: 1, workerId: workerOne.Id, worker: workerOne));
            var workerTeamTwo =
                SaveWorkerTeamToDatabase(DatabaseGatewayHelper.CreateWorkerTeamDatabaseEntity(id: 2, workerId: workerTwo.Id, worker: workerTwo));
            var workerTeams = new List<WorkerTeam> { workerTeamOne, workerTeamTwo };
            var team = SaveTeamToDatabase(DatabaseGatewayHelper.CreateTeamDatabaseEntity(workerTeams: workerTeams));

            var responseTeams = _classUnderTest.GetTeamsByTeamId(team.Id);
            var responseTeam = responseTeams.Find(rTeam => rTeam.Id == team.Id);

            responseTeam?.WorkerTeams.Count.Should().Be(2);

            var responseWorkerTeams = responseTeam?.WorkerTeams.ToList();
            var workerOneResponse = responseWorkerTeams?.Find(workerTeam => workerTeam.Worker.Id == workerOne.Id)?.Worker;
            var workerTwoResponse = responseWorkerTeams?.Find(workerTeam => workerTeam.Worker.Id == workerTwo.Id)?.Worker;

            workerOneResponse.Should().BeEquivalentTo(workerOne);
            workerTwoResponse.Should().BeEquivalentTo(workerTwo);
        }

        [Test]
        public void CreatingAnAllocationShouldInsertIntoTheDatabase()
        {
            var (request, worker, createdByWorker, person, team) = TestHelpers.CreateAllocationRequest();
            DatabaseContext.Teams.Add(team);
            DatabaseContext.Persons.Add(person);
            DatabaseContext.Workers.Add(worker);
            DatabaseContext.Workers.Add(createdByWorker);
            DatabaseContext.SaveChanges();

            var response = _classUnderTest.CreateAllocation(request);
            var query = DatabaseContext.Allocations;
            var insertedRecord = query.First(x => x.Id == response.AllocationId);

            Assert.AreEqual(insertedRecord.PersonId, request.MosaicId);
            Assert.AreEqual(insertedRecord.WorkerId, worker.Id);
            Assert.AreEqual(insertedRecord.CreatedBy, createdByWorker.Email);
            Assert.IsNotNull(insertedRecord.CreatedAt);
            Assert.IsNull(insertedRecord.LastModifiedAt);
            Assert.IsNull(insertedRecord.LastModifiedBy);
        }

        // [Test]
        // //deallocation
        // public void UpdatingAllocationShouldUpdateTheRecordInTheDatabase()
        // {
        //     int workerId = 555666;
        //     long mosaicId = 5000555;
        //     string deAllocatedByEmail = _faker.Internet.Email();
        //     string workerEmail = _faker.Internet.Email();
        //     string deallocationReason = "Test reason";
        //     int teamId = 1000;
        //     string personName = $"{_faker.Name.FirstName()} {_faker.Name.LastName()}";

        //     DateTime allocationStartDate = DateTime.Now.AddDays(-60);

        //     //add test data
        //     //TODO: add helper for setting up test data
        //     Worker worker = new Worker()
        //     {
        //         Email = workerEmail,
        //         Id = workerId,
        //         FirstName = _faker.Name.FirstName(),
        //         LastName = _faker.Name.LastName()
        //     };

        //     Worker deAllocatedByWorker = new Worker()
        //     {
        //         Email = deAllocatedByEmail,
        //         Id = _faker.Random.Number(),
        //         FirstName = _faker.Name.FirstName(),
        //         LastName = _faker.Name.LastName()
        //     };

        //     Team team = new Team()
        //     {
        //         Context = "A",
        //         Id = teamId,
        //         Name = "Test team"
        //     };

        //     Person person = new Person()
        //     {
        //         Id = mosaicId,
        //         FullName = personName
        //     };

        //     string createdByEmail = _faker.Internet.Email();

        //     Allocation allocation = new Allocation()
        //     {
        //         AllocationEndDate = null,
        //         AllocationStartDate = allocationStartDate,
        //         CreatedAt = allocationStartDate,
        //         CaseStatus = "Open",
        //         CaseClosureDate = null,
        //         LastModifiedAt = null,
        //         CreatedBy = createdByEmail,
        //         LastModifiedBy = null,
        //         PersonId = mosaicId,
        //         TeamId = teamId,
        //         WorkerId = workerId
        //     };

        //     DatabaseContext.Add(allocation);
        //     DatabaseContext.SaveChanges();

        //     var request = new UpdateAllocationRequest()
        //     {
        //         CreatedBy = deAllocatedByEmail,
        //         DeallocationReason = deallocationReason,
        //         Id = allocation.Id
        //     };

        //     DatabaseContext.Workers.Add(worker);
        //     DatabaseContext.Workers.Add(deAllocatedByWorker);
        //     DatabaseContext.Teams.Add(team);
        //     DatabaseContext.Persons.Add(person);

        //     DatabaseContext.SaveChanges();

        //     //TODO: add process data gw tests
        //     _mockProcessDataGateway.Setup(x => x.InsertCaseNoteDocument(It.IsAny<CaseNotesDocument>())).Returns(Task.FromResult(_faker.Random.Guid().ToString()));

        //     var response = _classUnderTest.UpdateAllocation(request);

        //     var query = DatabaseContext.Allocations;

        //     var updatedRecord = query.First(x => x.Id == allocation.Id);

        //     Assert.IsNotNull(updatedRecord);
        //     Assert.IsNotNull(updatedRecord.AllocationEndDate);
        //     Assert.AreEqual("Closed", updatedRecord.CaseStatus);
        //     Assert.AreEqual(workerId, updatedRecord.WorkerId);
        //     Assert.AreEqual(teamId, updatedRecord.TeamId);
        //     Assert.IsNotNull(updatedRecord.CaseClosureDate);

        //     //audit properties
        //     Assert.AreEqual(updatedRecord.CreatedBy, createdByEmail);
        //     Assert.IsNotNull(updatedRecord.CreatedAt);
        //     Assert.AreEqual(updatedRecord.CreatedAt, allocation.CreatedAt);
        //     Assert.IsNotNull(updatedRecord.LastModifiedAt);
        //     Assert.AreEqual(updatedRecord.LastModifiedBy, deAllocatedByEmail);

        //     //audit record
        //     var auditRecord = DatabaseContext.Audits.First(x => x.KeyValues.RootElement.GetProperty("Id").GetString() == updatedRecord.Id.ToString() && x.EntityState == "Modified");

        //     //key value
        //     Assert.AreEqual(auditRecord.KeyValues.RootElement.GetProperty("Id").GetInt64(), allocation.Id);

        //     //old values
        //     Assert.AreEqual(auditRecord.OldValues.RootElement.GetProperty("CaseStatus").GetString(), "Open");
        //     Assert.IsTrue(auditRecord.OldValues.RootElement.GetProperty("LastModifiedBy").GetString() == null);
        //     Assert.IsTrue(auditRecord.OldValues.RootElement.GetProperty("CaseClosureDate").GetString() == null);
        //     Assert.IsTrue(auditRecord.OldValues.RootElement.GetProperty("AllocationEndDate").GetString() == null);

        //     //new values
        //     Assert.AreEqual(auditRecord.NewValues.RootElement.GetProperty("CaseStatus").GetString(), "Closed");
        //     Assert.IsTrue(auditRecord.NewValues.RootElement.GetProperty("LastModifiedBy").GetString() == deAllocatedByEmail);
        //     Assert.IsNotNull(auditRecord.NewValues.RootElement.GetProperty("CaseClosureDate").GetDateTime());
        //     Assert.IsNotNull(auditRecord.NewValues.RootElement.GetProperty("AllocationEndDate").GetDateTime());
        // }

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

            const string gender = "M";

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

        #region Warning Notes
        [Test]
        public void PostWarningNoteShouldInsertIntoTheDatabase()
        {
            Person stubbedPerson = CreateStubbedPerson();
            stubbedPerson = SavePersonToDatabase(stubbedPerson);

            var request = _fixture.Build<PostWarningNoteRequest>()
                            .With(x => x.PersonId, stubbedPerson.Id)
                            .Create();

            var response = _classUnderTest.PostWarningNote(request);

            var query = DatabaseContext.WarningNotes;

            var insertedRecord = query.FirstOrDefault(x => x.Id == response.WarningNoteId);

            insertedRecord.Should().NotBeNull();
            insertedRecord.PersonId.Should().Be(request.PersonId);
            insertedRecord.StartDate.Should().Be(request.StartDate);
            insertedRecord.EndDate.Should().Be(request.EndDate);
            insertedRecord.DisclosedWithIndividual.Should().Be(request.DisclosedWithIndividual);
            insertedRecord.DisclosedDetails.Should().Be(request.DisclosedDetails);
            insertedRecord.Notes.Should().Be(request.Notes);
            insertedRecord.NoteType.Should().Be(request.NoteType);
            insertedRecord.Status.Should().Be("open");
            insertedRecord.DisclosedDate.Should().Be(request.DisclosedDate);
            insertedRecord.DisclosedHow.Should().Be(request.DisclosedHow);
            insertedRecord.WarningNarrative.Should().Be(request.WarningNarrative);
            insertedRecord.ManagerName.Should().Be(request.ManagerName);
            insertedRecord.DiscussedWithManagerDate.Should().Be(request.DiscussedWithManagerDate);

            //audit properties
            insertedRecord.CreatedAt.Should().NotBeNull();
            insertedRecord.CreatedAt.Should().NotBe(DateTime.MinValue);
            insertedRecord.CreatedBy.Should().Be(request.CreatedBy);
            insertedRecord.LastModifiedAt.Should().BeNull();
            insertedRecord.LastModifiedBy.Should().BeNull();
        }

        [Test]
        public void PostWarningNoteShouldThrowAnErrorIfThePersonIdIsNotInThePersonTable()
        {
            var request = _fixture.Create<PostWarningNoteRequest>();

            Action act = () => _classUnderTest.PostWarningNote(request);

            act.Should().Throw<PostWarningNoteException>()
                        .WithMessage($"Person with given id ({request.PersonId}) not found");
        }

        [Test]
        public void PostWarningNoteShouldCallInsertCaseNoteMethod()
        {
            Person stubbedPerson = CreateStubbedPerson();
            stubbedPerson = SavePersonToDatabase(stubbedPerson);

            var request = _fixture.Build<PostWarningNoteRequest>()
                            .With(x => x.PersonId, stubbedPerson.Id)
                            .Create();

            _mockProcessDataGateway.Setup(
                    x => x.InsertCaseNoteDocument(
                        It.IsAny<CaseNotesDocument>()))
                        .ReturnsAsync("CaseNoteId")
                        .Verifiable();

            var response = _classUnderTest.PostWarningNote(request);

            _mockProcessDataGateway.Verify();
            response.CaseNoteId.Should().NotBeNull();
            response.CaseNoteId.Should().Be("CaseNoteId");
        }

        // [Test]
        // public void PostWarningNoteThrowAnExceptionIfTheCaseNoteIsNotInserted()
        // {
        //     Person person = new Person()
        //     {
        //         FullName = "full name"
        //     };
        //     DatabaseContext.Persons.Add(person);
        //     DatabaseContext.SaveChanges();

        //     var request = _fixture.Build<PostWarningNoteRequest>()
        //                     .With(x => x.PersonId, person.Id)
        //                     .Create();

        //     _mockProcessDataGateway.Setup(
        //             x => x.InsertCaseNoteDocument(
        //                 It.IsAny<CaseNotesDocument>()))
        //                 .Throws(new Exception("error message"));

        //     Action act = () => _classUnderTest.PostWarningNote(request);

        //     act.Should().Throw<PostWarningNoteException>()
        //                 .WithMessage("Unable to create a case note. Allocation not created: error message");
        // }
        [Test]
        public void GetWarningNotesReturnsTheExpectedWarningNote()
        {
            WarningNote warningNote = new WarningNote()
            {
                PersonId = 12345
            };
            WarningNote wrongWarningNote = new WarningNote()
            {
                PersonId = 67890
            };
            DatabaseContext.WarningNotes.Add(warningNote);
            DatabaseContext.WarningNotes.Add(wrongWarningNote);
            DatabaseContext.SaveChanges();

            var request = _fixture.Build<GetWarningNoteRequest>()
                            .With(x => x.PersonId, warningNote.PersonId)
                            .Create();

            var response = _classUnderTest.GetWarningNotes(request);

            response.Should().ContainSingle();
            response.Should().ContainEquivalentOf(warningNote);
            response.Should().NotContain(wrongWarningNote);
        }

        [Test]
        public void GetWarningNotesReturnsAnExceptionIfTheWarningNoteDoesNotExist()
        {
            WarningNote warningNote = new WarningNote()
            {
                PersonId = 12345
            };
            DatabaseContext.WarningNotes.Add(warningNote);
            DatabaseContext.SaveChanges();

            var request = new GetWarningNoteRequest()
            {
                PersonId = 67890
            };

            Action act = () => _classUnderTest.GetWarningNotes(request);

            act.Should().Throw<DocumentNotFoundException>()
                .WithMessage($"No warning notes found relating to person id {request.PersonId}");
        }

        [Test]
        public void PatchWarningNoteUpdatesAnExistingRecordInTheDatabase()
        {
            Person stubbedPerson = CreateStubbedPerson();
            stubbedPerson = SavePersonToDatabase(stubbedPerson);

            Worker stubbedWorker = CreateStubbedWorker();
            SaveWorkerToDatabase(stubbedWorker);

            WarningNote stubbedWarningNote = CreateStubbedWarningNote(stubbedPerson);

            //clone the stub to compare the values later
            WarningNote stubbedWarningNoteOriginal = (WarningNote) stubbedWarningNote.Clone();

            stubbedWarningNote = SaveWarningNoteToDatabase(stubbedWarningNote);

            //grab the given id to "simulate" existing record
            stubbedWarningNoteOriginal.Id = stubbedWarningNote.Id;

            PatchWarningNoteRequest request = _fixture.Build<PatchWarningNoteRequest>()
                                                .With(x => x.WarningNoteId, stubbedWarningNote.Id)
                                                .With(x => x.ReviewedBy, stubbedWorker.Email)
                                                .With(x => x.Status, "closed")
                                                .Create();

            _classUnderTest.PatchWarningNote(request);
            var query = DatabaseContext.WarningNotes;
            var updatedRecord = query.First(x => x.Id == request.WarningNoteId);

            updatedRecord.EndDate.Should().Be(request.EndedDate);
            updatedRecord.LastReviewDate.Should().Be(request.ReviewDate);
            updatedRecord.NextReviewDate.Should().BeNull();
            updatedRecord.Status.Should().Be("closed");
            updatedRecord.Status.Should().NotBe(stubbedWarningNoteOriginal.Status);
            updatedRecord.LastModifiedBy.Should().Be(request.ReviewedBy);
            updatedRecord.LastModifiedBy.Should().NotBe(stubbedWarningNoteOriginal.LastModifiedBy);

            updatedRecord.Id.Should().Be(stubbedWarningNoteOriginal.Id);
            updatedRecord.PersonId.Should().Be(stubbedWarningNoteOriginal.PersonId);
            updatedRecord.StartDate.Should().Be(stubbedWarningNoteOriginal.StartDate);
            updatedRecord.DisclosedWithIndividual.Should().Be(stubbedWarningNoteOriginal.DisclosedWithIndividual);
            updatedRecord.DisclosedDetails.Should().Be(stubbedWarningNoteOriginal.DisclosedDetails);
            updatedRecord.Notes.Should().Be(stubbedWarningNoteOriginal.Notes);
            updatedRecord.NoteType.Should().Be(stubbedWarningNoteOriginal.NoteType);
            updatedRecord.DisclosedDate.Should().Be(stubbedWarningNoteOriginal.DisclosedDate);
            updatedRecord.DisclosedHow.Should().Be(stubbedWarningNoteOriginal.DisclosedHow);
            updatedRecord.WarningNarrative.Should().Be(stubbedWarningNoteOriginal.WarningNarrative);
            updatedRecord.ManagerName.Should().Be(stubbedWarningNoteOriginal.ManagerName);
            updatedRecord.DiscussedWithManagerDate.Should().Be(stubbedWarningNoteOriginal.DiscussedWithManagerDate);
            updatedRecord.CreatedBy.Should().Be(stubbedWarningNoteOriginal.CreatedBy);
        }

        [Test]
        public void PatchWarningNoteDoesNotChangeTheStatusEndDateOrChangeNextReviewDateToNullIfRequestPropertyIsOpen()
        {
            Person stubbedPerson = CreateStubbedPerson();
            stubbedPerson = SavePersonToDatabase(stubbedPerson);

            Worker stubbedWorker = CreateStubbedWorker();
            SaveWorkerToDatabase(stubbedWorker);

            WarningNote stubbedWarningNote = CreateStubbedWarningNote(stubbedPerson);

            //clone the stub to compare the values later
            WarningNote stubbedWarningNoteOriginal = (WarningNote) stubbedWarningNote.Clone();

            stubbedWarningNote = SaveWarningNoteToDatabase(stubbedWarningNote);

            //grab the given id to "simulate" existing record
            stubbedWarningNoteOriginal.Id = stubbedWarningNote.Id;

            PatchWarningNoteRequest request = _fixture.Build<PatchWarningNoteRequest>()
                                                .With(x => x.WarningNoteId, stubbedWarningNote.Id)
                                                .With(x => x.ReviewedBy, stubbedWorker.Email)
                                                .With(x => x.Status, "open")
                                                .Create();

            _classUnderTest.PatchWarningNote(request);
            var query = DatabaseContext.WarningNotes;
            var updatedRecord = query.First(x => x.Id == request.WarningNoteId);

            updatedRecord.Status.Should().NotBeNull();
            updatedRecord.Status.Should().Be(stubbedWarningNoteOriginal.Status);
            updatedRecord.EndDate.Should().Be(stubbedWarningNoteOriginal.EndDate);
            updatedRecord.NextReviewDate.Should().NotBeNull();
            updatedRecord.NextReviewDate.Should().Be(request.NextReviewDate);
        }

        [Test]
        public void PatchWarningNoteClosesTheWarningNoteIfRequestStatusIsClosed()
        {
            Person stubbedPerson = CreateStubbedPerson();
            stubbedPerson = SavePersonToDatabase(stubbedPerson);

            Worker stubbedWorker = CreateStubbedWorker();
            SaveWorkerToDatabase(stubbedWorker);

            WarningNote stubbedWarningNote = CreateStubbedWarningNote(stubbedPerson);

            //clone the stub to compare the values later
            WarningNote stubbedWarningNoteOriginal = (WarningNote) stubbedWarningNote.Clone();

            stubbedWarningNote = SaveWarningNoteToDatabase(stubbedWarningNote);

            //grab the given id to "simulate" existing record
            stubbedWarningNoteOriginal.Id = stubbedWarningNote.Id;

            PatchWarningNoteRequest request = _fixture.Build<PatchWarningNoteRequest>()
                                                .With(x => x.WarningNoteId, stubbedWarningNote.Id)
                                                .With(x => x.ReviewedBy, stubbedWorker.Email)
                                                .With(x => x.Status, "closed")
                                                .Create();

            _classUnderTest.PatchWarningNote(request);
            var query = DatabaseContext.WarningNotes;
            var updatedRecord = query.First(x => x.Id == request.WarningNoteId);

            updatedRecord.Status.Should().Be("closed");
            updatedRecord.EndDate.Should().Be(request.EndedDate);
            updatedRecord.NextReviewDate.Should().BeNull();
        }

        [Test]
        public void PatchWarningNoteThrowsAnExceptionWhenTheWarningNoteIsNotInTheWarningNoteTable()
        {
            PatchWarningNoteRequest request = new PatchWarningNoteRequest();

            Action act = () => _classUnderTest.PatchWarningNote(request);

            act.Should().Throw<PatchWarningNoteException>()
                        .WithMessage($"Warning Note with given id ({request.WarningNoteId}) not found");
        }

        [Test]
        public void PatchWarningNoteThrowsAndExceptionWhenTheWarningNoteIsAlreadyClosed()
        {
            Person stubbedPerson = CreateStubbedPerson();
            stubbedPerson = SavePersonToDatabase(stubbedPerson);

            WarningNote stubbedWarningNote = CreateStubbedWarningNote(stubbedPerson);
            stubbedWarningNote.Status = "closed";
            stubbedWarningNote = SaveWarningNoteToDatabase(stubbedWarningNote);

            PatchWarningNoteRequest request = _fixture.Build<PatchWarningNoteRequest>()
                                    .With(x => x.WarningNoteId, stubbedWarningNote.Id)
                                    .Create();

            Action act = () => _classUnderTest.PatchWarningNote(request);

            act.Should().Throw<PatchWarningNoteException>()
                        .WithMessage($"Warning Note with given id ({request.WarningNoteId}) has already been closed");
        }

        [Test]
        public void PatchWarningNoteThrowsAnExceptionWhenNoPersonPresentInDatabase()
        {
            WarningNote stubbedWarningNote = _fixture.Build<WarningNote>()
                                                .Without(x => x.Id)
                                                .Without(x => x.Person).Create();

            stubbedWarningNote = SaveWarningNoteToDatabase(stubbedWarningNote);

            PatchWarningNoteRequest request = _fixture.Build<PatchWarningNoteRequest>()
                                                .With(x => x.WarningNoteId, stubbedWarningNote.Id)
                                                .Create();

            Action act = () => _classUnderTest.PatchWarningNote(request);

            act.Should().Throw<PatchWarningNoteException>()
                        .WithMessage($"Person not found");
        }

        [Test]
        public void PatchWarningNoteAddsAWarningNoteReviewToTheDatabase()
        {
            Person stubbedPerson = CreateStubbedPerson();
            stubbedPerson = SavePersonToDatabase(stubbedPerson);

            Worker stubbedWorker = CreateStubbedWorker();
            SaveWorkerToDatabase(stubbedWorker);

            WarningNote stubbedWarningNote = CreateStubbedWarningNote(stubbedPerson);
            stubbedWarningNote = SaveWarningNoteToDatabase(stubbedWarningNote);

            PatchWarningNoteRequest request = _fixture.Build<PatchWarningNoteRequest>()
                                                .With(x => x.WarningNoteId, stubbedWarningNote.Id)
                                                .With(x => x.ReviewedBy, stubbedWorker.Email)
                                                .Create();

            _classUnderTest.PatchWarningNote(request);
            var query = DatabaseContext.WarningNoteReview;
            var insertedRecord = query.FirstOrDefault(
                                    x => x.WarningNoteId == request.WarningNoteId);

            insertedRecord.WarningNoteId.Should().Be(request.WarningNoteId);
            insertedRecord.ReviewDate.Should().Be(request.ReviewDate);
            insertedRecord.Notes.Should().Be(request.ReviewNotes);
            insertedRecord.ManagerName.Should().Be(request.ManagerName);
            insertedRecord.DiscussedWithManagerDate.Should().Be(request.DiscussedWithManagerDate);
            insertedRecord.CreatedBy.Should().Be(request.ReviewedBy);
            insertedRecord.LastModifiedBy.Should().Be(request.ReviewedBy);
        }

        [Test]
        public void PatchWarningNoteThrowsAnExceptionWhenReviewerIsNotPresentInWorkerTable()
        {
            Person stubbedPerson = CreateStubbedPerson();
            stubbedPerson = SavePersonToDatabase(stubbedPerson);

            WarningNote stubbedWarningNote = CreateStubbedWarningNote(stubbedPerson);
            stubbedWarningNote = SaveWarningNoteToDatabase(stubbedWarningNote);

            PatchWarningNoteRequest request = _fixture.Build<PatchWarningNoteRequest>()
                                                .With(x => x.WarningNoteId, stubbedWarningNote.Id)
                                                .Create();

            Action act = () => _classUnderTest.PatchWarningNote(request);

            act.Should().Throw<PatchWarningNoteException>()
                        .WithMessage($"Worker ({request.ReviewedBy}) not found");
        }

        [Test]
        public void PatchWarningNoteThrowsAnExceptionWhenTheWarningNoteReviewIsNotInsertedIntoTheDatabase()
        {
            Person stubbedPerson = CreateStubbedPerson();
            stubbedPerson = SavePersonToDatabase(stubbedPerson);

            Worker stubbedWorker = CreateStubbedWorker();
            SaveWorkerToDatabase(stubbedWorker);

            WarningNote stubbedWarningNote = CreateStubbedWarningNote(stubbedPerson);
            stubbedWarningNote = SaveWarningNoteToDatabase(stubbedWarningNote);

            PatchWarningNoteRequest request = _fixture.Build<PatchWarningNoteRequest>()
                                                .With(x => x.WarningNoteId, stubbedWarningNote.Id)
                                                .With(x => x.ReviewedBy, stubbedWorker.Email)
                                                .Create();
        }
        #endregion
        private Person CreateStubbedPerson()
        {
            return _fixture.Build<Person>()
                        .Without(x => x.Id)
                        .Without(x => x.Addresses)
                        .Without(x => x.OtherNames)
                        .Without(x => x.PhoneNumbers)
                        .Without(x => x.Allocations)
                        .Without(x => x.WarningNotes)
                        .Create();
        }

        private Person SavePersonToDatabase(Person person)
        {
            DatabaseContext.Persons.Add(person);
            DatabaseContext.SaveChanges();
            return person;
        }

        private WarningNote CreateStubbedWarningNote(Person person)
        {
            return _fixture.Build<WarningNote>()
                        .Without(x => x.Id)
                        .Without(x => x.Person)
                        .With(x => x.PersonId, person.Id)
                        .Create();
        }

        private WarningNote SaveWarningNoteToDatabase(WarningNote warningNote)
        {
            DatabaseContext.WarningNotes.Add(warningNote);
            DatabaseContext.SaveChanges();
            return warningNote;
        }

        private Worker CreateStubbedWorker()
        {
            return _fixture.Build<Worker>()
                    .Without(x => x.Id)
                    .Without(x => x.WorkerTeams)
                    .Without(x => x.Allocations)
                    .With(x => x.Email, _faker.Internet.Email())
                    .Create();
        }

        private Worker SaveWorkerToDatabase(Worker worker)
        {
            DatabaseContext.Workers.Add(worker);
            DatabaseContext.SaveChanges();
            return worker;
        }

        private WorkerTeam SaveWorkerTeamToDatabase(WorkerTeam workerTeam)
        {
            DatabaseContext.WorkerTeams.Add(workerTeam);
            DatabaseContext.SaveChanges();
            return workerTeam;
        }

        private Team SaveTeamToDatabase(Team team)
        {
            DatabaseContext.Teams.Add(team);
            DatabaseContext.SaveChanges();
            return team;
        }
    }
}

