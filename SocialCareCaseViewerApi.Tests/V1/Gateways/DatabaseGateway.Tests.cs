using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
using dbAddress = SocialCareCaseViewerApi.V1.Infrastructure.Address;

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

        [Test]
        public void UpdatingAllocationShouldUpdateTheRecordInTheDatabase()
        {
            var allocationStartDate = DateTime.Now.AddDays(-60);
            var (request, worker, deAllocatedByWorker, person, team) = TestHelpers.CreateUpdateAllocationRequest();

            var allocation = new Allocation
            {
                Id = request.Id,
                AllocationEndDate = null,
                AllocationStartDate = allocationStartDate,
                CreatedAt = allocationStartDate,
                CaseStatus = "Open",
                CaseClosureDate = null,
                LastModifiedAt = null,
                CreatedBy = deAllocatedByWorker.Email,
                LastModifiedBy = null,
                PersonId = person.Id,
                TeamId = team.Id,
                WorkerId = worker.Id
            };

            DatabaseContext.Allocations.Add(allocation);
            DatabaseContext.Workers.Add(worker);
            DatabaseContext.Workers.Add(deAllocatedByWorker);
            DatabaseContext.Teams.Add(team);
            DatabaseContext.Persons.Add(person);
            DatabaseContext.SaveChanges();

            _mockProcessDataGateway.Setup(x => x.InsertCaseNoteDocument(It.IsAny<CaseNotesDocument>())).Returns(Task.FromResult(_faker.Random.Guid().ToString()));

            _classUnderTest.UpdateAllocation(request);

            var query = DatabaseContext.Allocations;
            var updatedRecord = query.First(x => x.Id == allocation.Id);

            Assert.AreEqual("Closed", updatedRecord.CaseStatus);
            Assert.AreEqual(worker.Id, updatedRecord.WorkerId);
            Assert.AreEqual(team.Id, updatedRecord.TeamId);
            Assert.AreEqual(request.DeallocationDate, updatedRecord.CaseClosureDate);
            Assert.AreEqual(request.DeallocationDate, updatedRecord.AllocationEndDate);
            Assert.AreEqual(updatedRecord.CreatedBy, deAllocatedByWorker.Email);
            Assert.AreEqual(updatedRecord.CreatedAt, allocation.CreatedAt);
            Assert.AreEqual(updatedRecord.LastModifiedBy, deAllocatedByWorker.Email);
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

            const string gender = "M";
            const string restricted = "Y";

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
                CreatedBy = createdBy,
                Restricted = restricted
            };

            AddNewResidentResponse response = _classUnderTest.AddNewResident(request);

            Assert.IsNotNull(response.Id);

            Assert.IsNotNull(response.OtherNameIds);
            Assert.AreEqual(2, response.OtherNameIds.Count);

            Assert.IsNotNull(response.AddressId);

            Assert.IsNotNull(response.PhoneNumberIds);
            Assert.AreEqual(2, response.PhoneNumberIds.Count);

            //check that person was created with correct values
            var person = DatabaseContext.Persons.FirstOrDefault(x => x.Id == response.Id);

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
            Assert.AreEqual(restricted, person.Restricted);

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
        }

        [Test]
        public void CreatePersonRequestWithoutRestrictedValueSetShouldCreateRecordWithRestrictedValueSetToN()
        {
            AddNewResidentRequest request = new AddNewResidentRequest()
            {
                FirstName = _faker.Person.FirstName,
                LastName = _faker.Person.LastName,
                ContextFlag = "A",
                CreatedBy = _faker.Internet.Email().ToString()
            };

            var response = _classUnderTest.AddNewResident(request);

            Person person = DatabaseContext.Persons.First(x => x.Id == response.Id);

            response.Should().NotBeNull();
            person.Restricted.Should().Be("N");
        }

        #region Warning Notes
        [Test]
        public void PostWarningNoteShouldInsertIntoTheDatabase()
        {
            var stubbedPerson = TestHelpers.CreatePerson();
            DatabaseContext.Persons.Add(stubbedPerson);
            DatabaseContext.SaveChanges();

            var request = _fixture.Build<PostWarningNoteRequest>()
                            .With(x => x.PersonId, stubbedPerson.Id)
                            .Create();

            var response = _classUnderTest.PostWarningNote(request);

            var query = DatabaseContext.WarningNotes;

            var insertedRecord = query.FirstOrDefault(x => x.Id == response.WarningNoteId);

            insertedRecord.Should().NotBeNull();

            insertedRecord.PersonId.Should().Be(request.PersonId);
            insertedRecord.Status.Should().Be("open");

            insertedRecord.StartDate.Should().Be(request.StartDate);
            insertedRecord.EndDate.Should().Be(request.EndDate);

            insertedRecord.ReviewDate.Should().Be(request.ReviewDate);
            insertedRecord.NextReviewDate.Should().Be(request.NextReviewDate);

            insertedRecord.DisclosedWithIndividual.Should().Be(request.DisclosedWithIndividual);
            insertedRecord.DisclosedDetails.Should().Be(request.DisclosedDetails);
            insertedRecord.DisclosedDate.Should().Be(request.DisclosedDate);
            insertedRecord.DisclosedHow.Should().Be(request.DisclosedHow);

            insertedRecord.Notes.Should().Be(request.Notes);
            insertedRecord.NoteType.Should().Be(request.NoteType);
            insertedRecord.WarningNarrative.Should().Be(request.WarningNarrative);

            insertedRecord.ManagerName.Should().Be(request.ManagerName);
            insertedRecord.DiscussedWithManagerDate.Should().Be(request.DiscussedWithManagerDate);

            insertedRecord.CreatedBy.Should().Be(request.CreatedBy);

            //audit properties
            insertedRecord.CreatedAt.Should().NotBeNull();
            insertedRecord.CreatedAt.Should().NotBe(DateTime.MinValue);

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
            Person stubbedPerson = TestHelpers.CreatePerson();
            DatabaseContext.Persons.Add(stubbedPerson);
            DatabaseContext.SaveChanges();

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
            var testPersonId = _faker.Random.Int();

            var warningNote = TestHelpers.CreateWarningNote(testPersonId);

            DatabaseContext.WarningNotes.Add(warningNote);
            DatabaseContext.SaveChanges();

            var response = _classUnderTest.GetWarningNotes(testPersonId);
            var retrievedWarningNote = response.FirstOrDefault();

            response.Should().ContainSingle();

            retrievedWarningNote.Id.Should().Be(warningNote.Id);
            retrievedWarningNote.Status.Should().Be("open");

            retrievedWarningNote.PersonId.Should().Be(warningNote.PersonId);

            retrievedWarningNote.WarningNarrative.Should().Be(warningNote.WarningNarrative);

            retrievedWarningNote.StartDate.Should().Be(warningNote.StartDate);
            retrievedWarningNote.EndDate.Should().Be(warningNote.EndDate);

            retrievedWarningNote.ReviewDate.Should().Be(warningNote.ReviewDate);
            retrievedWarningNote.NextReviewDate.Should().Be(warningNote.NextReviewDate);

            retrievedWarningNote.DisclosedWithIndividual.Should().Be(warningNote.DisclosedWithIndividual);
            retrievedWarningNote.DisclosedDetails.Should().Be(warningNote.DisclosedDetails);
            retrievedWarningNote.DisclosedDate.Should().Be(warningNote.DisclosedDate);
            retrievedWarningNote.DisclosedHow.Should().Be(warningNote.DisclosedHow);

            retrievedWarningNote.Notes.Should().Be(warningNote.Notes);
            retrievedWarningNote.NoteType.Should().Be(warningNote.NoteType);

            retrievedWarningNote.ManagerName.Should().Be(warningNote.ManagerName);
            retrievedWarningNote.DiscussedWithManagerDate.Should().Be(warningNote.DiscussedWithManagerDate);

            retrievedWarningNote.CreatedBy.Should().Be(warningNote.CreatedBy);
        }

        [Test]
        public void GetWarningNotesReturnsAListOfWarningNotesForASpecificPerson()
        {
            var testPersonId = _faker.Random.Int();

            var firstWarningNote = TestHelpers.CreateWarningNote(testPersonId);
            var secondWarningNote = TestHelpers.CreateWarningNote(testPersonId);

            var differentWarningNote = TestHelpers.CreateWarningNote();

            DatabaseContext.WarningNotes.Add(firstWarningNote);
            DatabaseContext.WarningNotes.Add(secondWarningNote);
            DatabaseContext.WarningNotes.Add(differentWarningNote);
            DatabaseContext.SaveChanges();

            var response = _classUnderTest.GetWarningNotes(testPersonId);

            response.Count().Should().Be(2);
            response.Should().ContainEquivalentOf(firstWarningNote);
            response.Should().ContainEquivalentOf(secondWarningNote);
            response.Should().NotContain(differentWarningNote);
        }

        [Test]
        public void GetWarningNotesReturnsAnExceptionIfTheWarningNoteDoesNotExist()
        {
            var testPersonId = _faker.Random.Int();
            var differentPersonId = _faker.Random.Int();

            var warningNote = new WarningNote
            {
                PersonId = testPersonId
            };
            DatabaseContext.WarningNotes.Add(warningNote);
            DatabaseContext.SaveChanges();

            Action act = () => _classUnderTest.GetWarningNotes(differentPersonId);

            act.Should().Throw<DocumentNotFoundException>()
                .WithMessage($"No warning notes found relating to person id {differentPersonId}");
        }

        [Test]
        public void PatchWarningNoteUpdatesAnExistingRecordInTheDatabase()
        {
            var (request, person, worker, warningNote) = TestHelpers.CreatePatchWarningNoteRequest(requestStatus: "closed");

            //clone the stub to compare the values later
            WarningNote stubbedWarningNote = (WarningNote) warningNote.Clone();

            DatabaseContext.Persons.Add(person);
            DatabaseContext.Workers.Add(worker);
            DatabaseContext.WarningNotes.Add(warningNote);
            DatabaseContext.SaveChanges();

            _classUnderTest.PatchWarningNote(request);
            var query = DatabaseContext.WarningNotes;
            var updatedRecord = query.First(x => x.Id == request.WarningNoteId);

            updatedRecord.EndDate.Should().Be(request.EndedDate);
            updatedRecord.LastReviewDate.Should().Be(request.ReviewDate);
            updatedRecord.NextReviewDate.Should().BeNull();
            updatedRecord.Status.Should().Be("closed");
            updatedRecord.Status.Should().NotBe(stubbedWarningNote.Status);
            updatedRecord.LastModifiedBy.Should().Be(request.ReviewedBy);
            updatedRecord.LastModifiedBy.Should().NotBe(stubbedWarningNote.LastModifiedBy);

            updatedRecord.Id.Should().Be(stubbedWarningNote.Id);
            updatedRecord.PersonId.Should().Be(stubbedWarningNote.PersonId);
            updatedRecord.StartDate.Should().Be(stubbedWarningNote.StartDate);
            updatedRecord.DisclosedWithIndividual.Should().Be(stubbedWarningNote.DisclosedWithIndividual);
            updatedRecord.DisclosedDetails.Should().Be(stubbedWarningNote.DisclosedDetails);
            updatedRecord.Notes.Should().Be(stubbedWarningNote.Notes);
            updatedRecord.NoteType.Should().Be(stubbedWarningNote.NoteType);
            updatedRecord.DisclosedDate.Should().Be(stubbedWarningNote.DisclosedDate);
            updatedRecord.DisclosedHow.Should().Be(stubbedWarningNote.DisclosedHow);
            updatedRecord.WarningNarrative.Should().Be(stubbedWarningNote.WarningNarrative);
            updatedRecord.ManagerName.Should().Be(stubbedWarningNote.ManagerName);
            updatedRecord.DiscussedWithManagerDate.Should().Be(stubbedWarningNote.DiscussedWithManagerDate);
            updatedRecord.CreatedBy.Should().Be(stubbedWarningNote.CreatedBy);
        }

        [Test]
        public void PatchWarningNoteDoesNotChangeTheStatusEndDateOrChangeNextReviewDateToNullIfRequestPropertyIsOpen()
        {
            var (request, person, worker, warningNote) = TestHelpers.CreatePatchWarningNoteRequest(requestStatus: "open");

            //clone the stub to compare the values later
            WarningNote stubbedWarningNote = (WarningNote) warningNote.Clone();

            DatabaseContext.Persons.Add(person);
            DatabaseContext.Workers.Add(worker);
            DatabaseContext.WarningNotes.Add(warningNote);
            DatabaseContext.SaveChanges();

            _classUnderTest.PatchWarningNote(request);
            var query = DatabaseContext.WarningNotes;
            var updatedRecord = query.First(x => x.Id == request.WarningNoteId);

            updatedRecord.Status.Should().NotBeNull();
            updatedRecord.Status.Should().Be(stubbedWarningNote.Status);
            updatedRecord.EndDate.Should().Be(stubbedWarningNote.EndDate);
            updatedRecord.NextReviewDate.Should().NotBeNull();
            updatedRecord.NextReviewDate.Should().Be(request.NextReviewDate);
        }

        [Test]
        public void PatchWarningNoteClosesTheWarningNoteIfRequestStatusIsClosed()
        {
            var (request, person, worker, warningNote) = TestHelpers.CreatePatchWarningNoteRequest(requestStatus: "closed");

            //clone the stub to compare the values later
            WarningNote stubbedWarningNote = (WarningNote) warningNote.Clone();

            DatabaseContext.Persons.Add(person);
            DatabaseContext.Workers.Add(worker);
            DatabaseContext.WarningNotes.Add(warningNote);
            DatabaseContext.SaveChanges();

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
        public void PatchWarningNoteThrowsAnExceptionWhenTheWarningNoteIsAlreadyClosed()
        {
            var (request, person, worker, warningNote) = TestHelpers.CreatePatchWarningNoteRequest(startingStatus: "closed", requestStatus: "closed");

            //clone the stub to compare the values later
            WarningNote stubbedWarningNote = (WarningNote) warningNote.Clone();

            DatabaseContext.Persons.Add(person);
            DatabaseContext.Workers.Add(worker);
            DatabaseContext.WarningNotes.Add(warningNote);
            DatabaseContext.SaveChanges();

            Action act = () => _classUnderTest.PatchWarningNote(request);

            act.Should().Throw<PatchWarningNoteException>()
                        .WithMessage($"Warning Note with given id ({request.WarningNoteId}) has already been closed");
        }

        [Test]
        public void PatchWarningNoteThrowsAnExceptionWhenNoPersonPresentInDatabase()
        {
            WarningNote stubbedWarningNote = TestHelpers.CreateWarningNote();
            DatabaseContext.WarningNotes.Add(stubbedWarningNote);
            DatabaseContext.SaveChanges();

            PatchWarningNoteRequest request = _fixture.Build<PatchWarningNoteRequest>()
                                                .With(x => x.WarningNoteId, stubbedWarningNote.Id)
                                                .Create();

            Action act = () => _classUnderTest.PatchWarningNote(request);

            act.Should().Throw<PatchWarningNoteException>()
                        .WithMessage($"Person not found");
        }

        [Test]
        public void PatchWarningNoteThrowsAnExceptionWhenReviewerIsNotPresentInWorkerTable()
        {
            Person stubbedPerson = TestHelpers.CreatePerson();
            WarningNote stubbedWarningNote = TestHelpers.CreateWarningNote(personId: stubbedPerson.Id);

            DatabaseContext.Persons.Add(stubbedPerson);
            DatabaseContext.WarningNotes.Add(stubbedWarningNote);
            DatabaseContext.SaveChanges();

            PatchWarningNoteRequest request = _fixture.Build<PatchWarningNoteRequest>()
                                                .With(x => x.WarningNoteId, stubbedWarningNote.Id)
                                                .Create();

            Action act = () => _classUnderTest.PatchWarningNote(request);

            act.Should().Throw<PatchWarningNoteException>()
                        .WithMessage($"Worker ({request.ReviewedBy}) not found");
        }

        [Test]
        public void PatchWarningNoteAddsAWarningNoteReviewToTheDatabase()
        {
            var (request, person, worker, warningNote) = TestHelpers.CreatePatchWarningNoteRequest();

            //clone the stub to compare the values later
            WarningNote stubbedWarningNote = (WarningNote) warningNote.Clone();

            DatabaseContext.Persons.Add(person);
            DatabaseContext.Workers.Add(worker);
            DatabaseContext.WarningNotes.Add(warningNote);
            DatabaseContext.SaveChanges();


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
        #endregion

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

        [Test]
        public void GetPersonDetailsByIdReturnsPerson()
        {
            var person = SavePersonToDatabase(DatabaseGatewayHelper.CreatePersonDatabaseEntity());

            //add all necessary related entities using person id
            SaveAddressToDatabase(DatabaseGatewayHelper.CreateAddressDatabaseEntity(person.Id));
            SavePhoneNumberToDataBase(DatabaseGatewayHelper.CreatePhoneNumberEntity(person.Id));
            SavePersonOtherNameToDatabase(DatabaseGatewayHelper.CreatePersonOtherNameDatabaseEntity(person.Id));

            var response = _classUnderTest.GetPersonDetailsById(person.Id);

            response.Should().BeEquivalentTo(person);
        }

        [Test]
        public void UpdatePersonThrowsUpdatePersonExceptionWhenPersonNotFound()
        {
            Action act = () => _classUnderTest.UpdatePerson(new UpdatePersonRequest() { Id = _faker.Random.Long() });

            act.Should().Throw<UpdatePersonException>().WithMessage("Person not found");
        }

        [Test]
        public void UpdatePersonSetsCorrectValuesForPersonEntity()
        {
            Person person = SavePersonToDatabase(DatabaseGatewayHelper.CreatePersonDatabaseEntity());

            UpdatePersonRequest request = GetValidUpdatePersonRequest(person.Id);

            _classUnderTest.UpdatePerson(request);

            person.AgeContext.Should().Be(request.ContextFlag);
            person.DateOfBirth.Should().Be(request.DateOfBirth);
            person.DateOfDeath.Should().Be(request.DateOfDeath);
            person.EmailAddress.Should().Be(request.EmailAddress);
            person.Ethnicity.Should().Be(request.Ethnicity);
            person.FirstLanguage.Should().Be(request.FirstLanguage);
            person.FirstName.Should().Be(request.FirstName);
            person.FullName.Should().Be($"{request.FirstName} {request.LastName}");
            person.Gender.Should().Be(request.Gender);
            person.LastModifiedBy.Should().Be(request.CreatedBy);
            person.LastName.Should().Be(request.LastName);
            person.NhsNumber.Should().Be(request.NhsNumber);
            person.PreferredMethodOfContact.Should().Be(request.PreferredMethodOfContact);
            person.Religion.Should().Be(request.Religion);
            person.Restricted.Should().Be(request.Restricted);
            person.SexualOrientation.Should().Be(request.SexualOrientation);
            person.Title.Should().Be(request.Title);
        }

        [Test]
        public void UpdatePersonSetsNewDisplayAddressWhenOneDoesntExist()
        {
            Person person = SavePersonToDatabase(DatabaseGatewayHelper.CreatePersonDatabaseEntity());

            UpdatePersonRequest request = GetValidUpdatePersonRequest(person.Id);

            _classUnderTest.UpdatePerson(request);

            person.Addresses.Count.Should().Be(1);
            person.Addresses.First().IsDisplayAddress.Should().Be("Y");
            person.Addresses.First(x => x.IsDisplayAddress == "Y").AddressLines.Should().Be(request.Address.Address);
            person.Addresses.First(x => x.IsDisplayAddress == "Y").PostCode.Should().Be(request.Address.Postcode);
            person.Addresses.First(x => x.IsDisplayAddress == "Y").Uprn.Should().Be(request.Address.Uprn);
            person.Addresses.First(x => x.IsDisplayAddress == "Y").CreatedBy.Should().Be(request.CreatedBy);
            person.Addresses.First(x => x.IsDisplayAddress == "Y").CreatedAt.Should().NotBeNull();
            person.Addresses.First().StartDate.Should().NotBeNull();
        }

        [Test]
        public void UpdatePersonUpdatesTheCurrentDisplayAddressToBeHistrocalAddress()
        {
            Person person = SavePersonToDatabase(DatabaseGatewayHelper.CreatePersonDatabaseEntity());

            dbAddress displayAddress = SaveAddressToDatabase(DatabaseGatewayHelper.CreateAddressDatabaseEntity(person.Id, "Y"));

            UpdatePersonRequest request = GetValidUpdatePersonRequest(person.Id);

            _classUnderTest.UpdatePerson(request);

            person.Addresses.Count.Should().Be(2);

            displayAddress.IsDisplayAddress.Should().Be("N");
            displayAddress.LastModifiedBy.Should().Be(request.CreatedBy);
            displayAddress.EndDate.Should().NotBeNull();
        }

        [Test]
        public void UpdatePersonDoesntUpdateCurrentDisplayAddressIfAddressHasntChanged()
        {
            Person person = SavePersonToDatabase(DatabaseGatewayHelper.CreatePersonDatabaseEntity());

            dbAddress displayAddress = SaveAddressToDatabase(DatabaseGatewayHelper.CreateAddressDatabaseEntity(person.Id, "Y"));

            UpdatePersonRequest request = GetValidUpdatePersonRequest(person.Id);

            request.Address.Address = displayAddress.AddressLines;
            request.Address.Postcode = displayAddress.PostCode;
            request.Address.Uprn = displayAddress.Uprn;

            _classUnderTest.UpdatePerson(request);

            person.Addresses.First().AddressLines.Should().Be(request.Address.Address);
            person.Addresses.First().PostCode.Should().Be(request.Address.Postcode);
            person.Addresses.First().Uprn.Should().Be(request.Address.Uprn);

            person.Addresses.First().IsDisplayAddress.Should().Be("Y");
            person.Addresses.First().EndDate.Should().BeNull();
        }

        [Test]
        public void UpdatePersonSetsTheCurrentDisplayAddressNotToBeDisplayAddressWhenAddressIsNotProvided()
        {
            Person person = SavePersonToDatabase(DatabaseGatewayHelper.CreatePersonDatabaseEntity());

            SaveAddressToDatabase(DatabaseGatewayHelper.CreateAddressDatabaseEntity(person.Id, "Y"));

            UpdatePersonRequest request = GetValidUpdatePersonRequest(person.Id);

            request.Address = null;

            _classUnderTest.UpdatePerson(request);

            person.Addresses.First().IsDisplayAddress.Should().Be("N");
            person.Addresses.First().LastModifiedBy.Should().Be(request.CreatedBy);
            person.Addresses.First().EndDate.Should().NotBeNull();
        }

        [Test]
        public void UpdatePersonReplacesPhoneNumbers()
        {
            Person person = SavePersonToDatabase(DatabaseGatewayHelper.CreatePersonDatabaseEntity());
            PhoneNumberInfrastructure phoneNumber = SavePhoneNumberToDataBase(DatabaseGatewayHelper.CreatePhoneNumberEntity(person.Id));

            UpdatePersonRequest request = GetValidUpdatePersonRequest(person.Id);

            _classUnderTest.UpdatePerson(request);

            PhoneNumber number1 = request.PhoneNumbers.First();
            PhoneNumber number2 = request.PhoneNumbers.Last();

            person.PhoneNumbers.First(x => x.Number == number1.Number).Type.Should().Be(number1.Type);
            person.PhoneNumbers.First(x => x.Number == number2.Number).Type.Should().Be(number2.Type);

            person.PhoneNumbers.First().CreatedBy.Should().Be(request.CreatedBy);
            person.PhoneNumbers.First().CreatedAt.Should().NotBeNull();

            person.PhoneNumbers.Last().CreatedBy.Should().Be(request.CreatedBy);
            person.PhoneNumbers.Last().CreatedAt.Should().NotBeNull();

            person.PhoneNumbers.Count.Should().Be(request.PhoneNumbers.Count);
        }

        [Test]
        public void UpdatePersonRemovesPhoneNumbersIfNewOnesAreNotProvided()
        {
            Person person = SavePersonToDatabase(DatabaseGatewayHelper.CreatePersonDatabaseEntity());
            PhoneNumberInfrastructure phoneNumber = SavePhoneNumberToDataBase(DatabaseGatewayHelper.CreatePhoneNumberEntity(person.Id));

            UpdatePersonRequest request = GetValidUpdatePersonRequest(person.Id);
            request.PhoneNumbers = null;

            _classUnderTest.UpdatePerson(request);

            person.PhoneNumbers.Count.Should().Be(0);
        }

        [Test]
        public void UpdatePersonReplacesOtherNames()
        {
            Person person = SavePersonToDatabase(DatabaseGatewayHelper.CreatePersonDatabaseEntity());
            SavePersonOtherNameToDatabase(DatabaseGatewayHelper.CreatePersonOtherNameDatabaseEntity(person.Id));

            UpdatePersonRequest request = GetValidUpdatePersonRequest(person.Id);

            _classUnderTest.UpdatePerson(request);

            OtherName name1 = request.OtherNames.First();
            OtherName name2 = request.OtherNames.Last();

            person.OtherNames.First(x => x.FirstName == name1.FirstName).LastName.Should().Be(name1.LastName);
            person.OtherNames.First(x => x.FirstName == name2.FirstName).LastName.Should().Be(name2.LastName);

            person.OtherNames.First().CreatedBy.Should().Be(request.CreatedBy);
            person.OtherNames.First().CreatedAt.Should().NotBeNull();

            person.OtherNames.Last().CreatedBy.Should().Be(request.CreatedBy);
            person.OtherNames.Last().CreatedAt.Should().NotBeNull();

            person.OtherNames.Count.Should().Be(request.PhoneNumbers.Count);
        }

        [Test]
        public void UpdatePersonRemovesOtherNamesIfNewOnesAreNotProvided()
        {
            Person person = SavePersonToDatabase(DatabaseGatewayHelper.CreatePersonDatabaseEntity());
            SavePersonOtherNameToDatabase(DatabaseGatewayHelper.CreatePersonOtherNameDatabaseEntity(person.Id));

            UpdatePersonRequest request = GetValidUpdatePersonRequest(person.Id);
            request.OtherNames = null;

            _classUnderTest.UpdatePerson(request);

            person.OtherNames.Count.Should().Be(0);
        }


        private Person SavePersonToDatabase(Person person)
        {
            DatabaseContext.Persons.Add(person);
            DatabaseContext.SaveChanges();
            return person;
        }

        private dbAddress SaveAddressToDatabase(dbAddress address)
        {
            DatabaseContext.Addresses.Add(address);
            DatabaseContext.SaveChanges();
            return address;
        }

        private PhoneNumberInfrastructure SavePhoneNumberToDataBase(PhoneNumberInfrastructure phoneNumber)
        {
            DatabaseContext.PhoneNumbers.Add(phoneNumber);
            DatabaseContext.SaveChanges();
            return phoneNumber;
        }

        private PersonOtherName SavePersonOtherNameToDatabase(PersonOtherName name)
        {
            DatabaseContext.PersonOtherNames.Add(name);
            DatabaseContext.SaveChanges();
            return name;
        }

        private static UpdatePersonRequest GetValidUpdatePersonRequest(long personId)
        {
            AddressDomain address = new Faker<AddressDomain>()
               .RuleFor(a => a.Address, f => f.Address.StreetAddress())
               .RuleFor(a => a.Postcode, f => f.Address.ZipCode())
               .RuleFor(a => a.Uprn, f => f.Random.Int());

            List<PhoneNumber> phoneNumbers = new List<PhoneNumber>()
            {
                new Faker<PhoneNumber>()
                     .RuleFor(p => p.Number, f => f.Phone.PhoneNumber())
                     .RuleFor(p => p.Type, f => f.Random.Word()),

                new Faker<PhoneNumber>()
                     .RuleFor(p => p.Number, f => f.Phone.PhoneNumber())
                     .RuleFor(p => p.Type, f => f.Random.Word())
            };

            List<OtherName> otherNames = new List<OtherName>()
            {
                new Faker<OtherName>()
                    .RuleFor(o => o.FirstName, f => f.Person.FirstName)
                    .RuleFor(o => o.LastName, f => f.Person.LastName),

                new Faker<OtherName>()
                    .RuleFor(o => o.FirstName, f => f.Person.FirstName)
                    .RuleFor(o => o.LastName, f => f.Person.LastName)
            };

            return new Faker<UpdatePersonRequest>()
               .RuleFor(p => p.Id, personId)
               .RuleFor(p => p.FirstLanguage, f => f.Random.Word())
               .RuleFor(p => p.SexualOrientation, f => f.Random.Word())
               .RuleFor(p => p.ContextFlag, f => f.Random.String2(1))
               .RuleFor(p => p.EmailAddress, f => f.Internet.Email())
               .RuleFor(p => p.Ethnicity, f => f.Random.Word())
               .RuleFor(p => p.DateOfBirth, f => f.Date.Past())
               .RuleFor(p => p.DateOfDeath, f => f.Date.Past())
               .RuleFor(p => p.FirstName, f => f.Person.FirstName)
               .RuleFor(p => p.LastName, f => f.Person.LastName)
               .RuleFor(p => p.Title, f => f.Random.String2(2))
               .RuleFor(p => p.Gender, f => f.Random.String2(1))
               .RuleFor(p => p.NhsNumber, f => f.Random.Number())
               .RuleFor(p => p.PreferredMethodOfContact, f => f.Random.Word())
               .RuleFor(p => p.Religion, f => f.Random.Word())
               .RuleFor(p => p.Restricted, f => f.Random.String2(1))
               .RuleFor(p => p.CreatedBy, f => f.Internet.Email())
               .RuleFor(p => p.Address, address)
               .RuleFor(p => p.PhoneNumbers, phoneNumbers)
               .RuleFor(p => p.OtherNames, otherNames);
        }
    }
}
