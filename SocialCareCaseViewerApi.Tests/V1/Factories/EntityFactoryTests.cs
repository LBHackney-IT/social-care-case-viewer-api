using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using AutoFixture;
using Bogus;
using FluentAssertions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using SocialCareCaseViewerApi.Tests.V1.Helpers;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Boundary.Response;
using SocialCareCaseViewerApi.V1.Domain;
using SocialCareCaseViewerApi.V1.Factories;
using SocialCareCaseViewerApi.V1.Infrastructure;
using dbAddress = SocialCareCaseViewerApi.V1.Infrastructure.Address;
using dbPhoneNumber = SocialCareCaseViewerApi.V1.Infrastructure.PhoneNumber;
using dbTeam = SocialCareCaseViewerApi.V1.Infrastructure.Team;
using dbWarningNote = SocialCareCaseViewerApi.V1.Infrastructure.WarningNote;
using dbWorker = SocialCareCaseViewerApi.V1.Infrastructure.Worker;
using PhoneNumber = SocialCareCaseViewerApi.V1.Domain.PhoneNumber;
using Team = SocialCareCaseViewerApi.V1.Domain.Team;
using WarningNote = SocialCareCaseViewerApi.V1.Domain.WarningNote;
using Worker = SocialCareCaseViewerApi.V1.Domain.Worker;
using DomainCaseSubmission = SocialCareCaseViewerApi.V1.Domain.CaseSubmission;

namespace SocialCareCaseViewerApi.Tests.V1.Factories
{
    [TestFixture]
    public class EntityFactoryTests
    {
        private Faker _faker;
        private Fixture _fixture;

        [SetUp]
        public void SetUp()
        {
            _faker = new Faker();
            _fixture = new Fixture();
        }

        [Test]
        public void CanMapWorkerFromInfrastructureToDomainWithoutTeamDetails()
        {
            var email = _faker.Internet.Email();
            var firstName = _faker.Name.FirstName();
            var lastName = _faker.Name.LastName();
            var id = 1;
            var role = _faker.Random.Word();
            int allocationCount = 1;
            var contextFlag = "A";
            var createdBy = _faker.Internet.Email();
            var dateStart = DateTime.Now;

            var dbWorker = new dbWorker()
            {
                Email = email,
                FirstName = firstName,
                LastName = lastName,
                Id = id,
                Role = role,
                ContextFlag = contextFlag,
                CreatedBy = createdBy,
                DateStart = dateStart,
                Allocations = new List<AllocationSet>() {
                    new AllocationSet() { Id = 1, PersonId = 2, CaseStatus = "Open" },
                    new AllocationSet() { Id = 2, PersonId = 3, CaseStatus = "Closed" }
                }
            };

            var expectedResponse = new Worker()
            {
                FirstName = firstName,
                LastName = lastName,
                Id = id,
                AllocationCount = allocationCount,
                Email = email,
                Role = role,
                ContextFlag = contextFlag,
                CreatedBy = createdBy,
                DateStart = dateStart,
                Teams = null
            };

            dbWorker.ToDomain(false).Should().BeEquivalentTo(expectedResponse);
        }

        [Test]
        public void CanMapWorkerFromInfrastructureToDomainWithTeamDetails()
        {
            var email = _faker.Internet.Email();
            var firstName = _faker.Name.FirstName();
            var lastName = _faker.Name.LastName();
            var id = 1;
            var role = _faker.Random.Word();
            int allocationCount = 1; //open allocations
            var contextFlag = "A";
            var createdBy = _faker.Internet.Email();
            var dateStart = DateTime.Now;

            var dbWorker = new dbWorker()
            {
                Email = email,
                FirstName = firstName,
                LastName = lastName,
                Id = id,
                Role = role,
                ContextFlag = contextFlag,
                CreatedBy = createdBy,
                DateStart = dateStart,
                Allocations = new List<AllocationSet>() {
                    new AllocationSet() { Id = 1, PersonId = 2, CaseStatus = "Closed" },
                    new AllocationSet() { Id = 2, PersonId = 3, CaseStatus = "Open" }
                },
                WorkerTeams = new List<WorkerTeam>()
                {
                    new WorkerTeam() { Id = 1 , Team = new dbTeam() { Id = 1, Name = "Team 1", Context = "C" } },
                    new WorkerTeam() { Id = 2 , Team = new dbTeam() { Id = 2, Name = "Team 2", Context = "C" } },
                }
            };

            var expectedResponse = new Worker()
            {
                FirstName = firstName,
                LastName = lastName,
                Id = id,
                AllocationCount = allocationCount,
                Email = email,
                Role = role,
                ContextFlag = contextFlag,
                CreatedBy = createdBy,
                DateStart = dateStart,
                Teams = new List<Team>()
                {
                    new Team() { Id = 1, Name = "Team 1", Context = "C"},
                    new Team() { Id = 2, Name = "Team 2", Context = "C"}
                }
            };

            dbWorker.ToDomain(true).Should().BeEquivalentTo(expectedResponse);
        }

        [Test]
        public void CanMapTeamFromInfrastructureToDomain()
        {
            var id = _faker.Random.Number();
            var name = _faker.Name.ToString();
            var context = "a";

            var dbTeam = new dbTeam()
            {
                Id = id,
                Name = name,
                Context = context
            };

            var expectedResponse = new Team()
            {
                Id = id,
                Name = name,
                Context = context
            };

            dbTeam.ToDomain().Should().BeEquivalentTo(expectedResponse);
        }

        [Test]
        public void CanMapWarningNoteFromDatabaseEntityToDomainObject()
        {
            long number = _faker.Random.Number();
            var dt = DateTime.Now;
            var text = _faker.Random.String();

            var dbWarningNote = new dbWarningNote
            {
                Id = number,
                PersonId = number,
                StartDate = dt,
                EndDate = dt,
                DisclosedWithIndividual = true,
                DisclosedDetails = text,
                Notes = text,
                ReviewDate = dt,
                NextReviewDate = dt,
                NoteType = text,
                Status = text,
                DisclosedDate = dt,
                DisclosedHow = text,
                WarningNarrative = text,
                ManagerName = text,
                DiscussedWithManagerDate = dt,
                CreatedBy = text
            };

            var expectedResponse = new WarningNote
            {
                Id = number,
                PersonId = number,
                StartDate = dt,
                EndDate = dt,
                DisclosedWithIndividual = true,
                DisclosedDetails = text,
                Notes = text,
                ReviewDate = dt,
                NextReviewDate = dt,
                NoteType = text,
                Status = text,
                DisclosedDate = dt,
                DisclosedHow = text,
                WarningNarrative = text,
                ManagerName = text,
                DiscussedWithManagerDate = dt,
                CreatedBy = text,
                WarningNoteReviews = new List<WarningNoteReview>()
            };

            var response = dbWarningNote.ToDomain();

            response.Should().BeOfType<WarningNote>();
            response.Should().BeEquivalentTo(expectedResponse);
        }

        [Test]
        public void CanMapPostWarningNoteRequestToDatabaseObject()
        {
            long number = _faker.Random.Number();
            var dt = DateTime.Now;
            var text = _faker.Random.String();

            var request = new PostWarningNoteRequest
            {
                PersonId = number,
                StartDate = dt,
                EndDate = dt,
                DisclosedWithIndividual = true,
                DisclosedDetails = text,
                Notes = text,
                ReviewDate = dt,
                NextReviewDate = dt,
                NoteType = text,
                DisclosedDate = dt,
                DisclosedHow = text,
                WarningNarrative = text,
                ManagerName = text,
                DiscussedWithManagerDate = dt,
                CreatedBy = text
            };

            var expectedResponse = new dbWarningNote
            {
                PersonId = number,
                StartDate = dt,
                EndDate = dt,
                DisclosedWithIndividual = true,
                DisclosedDetails = text,
                Notes = text,
                ReviewDate = dt,
                NextReviewDate = dt,
                NoteType = text,
                Status = "open",
                DisclosedDate = dt,
                DisclosedHow = text,
                WarningNarrative = text,
                ManagerName = text,
                DiscussedWithManagerDate = dt,
                CreatedBy = text
            };

            var response = request.ToDatabaseEntity();

            response.Should().BeOfType<dbWarningNote>();
            response.Should().BeEquivalentTo(expectedResponse);
        }

        [Test]
        public void CanMapPhoneNumberFromDatabaseEntityToDomainObject()
        {
            dbPhoneNumber phoneNumber = DatabaseGatewayHelper.CreatePhoneNumberEntity(_faker.Random.Long());

            var expectedResponse = new PhoneNumber()
            {
                Number = phoneNumber.Number,
                Type = phoneNumber.Type
            };

            phoneNumber.ToDomain().Should().BeEquivalentTo(expectedResponse);
        }

        [Test]
        public void CanMapPersonOtherNameFromDatabaseObjectToDomainObject()
        {
            PersonOtherName otherName = DatabaseGatewayHelper.CreatePersonOtherNameDatabaseEntity(_faker.Random.Long());

            var expectedResponse = new OtherName()
            {
                FirstName = otherName.FirstName,
                LastName = otherName.LastName
            };

            otherName.ToDomain().Should().BeEquivalentTo(expectedResponse);
        }

        [Test]
        public void CanMapCaseSubmissionFromDatabaseObjectToDomainObject()
        {
            var databaseCaseSubmission = TestHelpers.CreateCaseSubmission();

            var domainCaseSubmission = new DomainCaseSubmission()
            {
                FormId = databaseCaseSubmission.FormId,
                Residents = databaseCaseSubmission.Residents,
                Workers = databaseCaseSubmission.Workers.Select(w => w.ToDomain(false)).ToList(),
                CreatedAt = databaseCaseSubmission.CreatedAt,
                CreatedBy = databaseCaseSubmission.CreatedBy.ToDomain(false),
                EditHistory = databaseCaseSubmission.EditHistory.Select(e => new EditHistory<Worker>
                {
                    EditTime = e.EditTime,
                    Worker = e.Worker.ToDomain(false)
                }).ToList(),
                SubmissionState = databaseCaseSubmission.SubmissionState,
                FormAnswers = databaseCaseSubmission.FormAnswers,
            };

            databaseCaseSubmission.ToDomain().Should().BeEquivalentTo(domainCaseSubmission);
        }

        [Test]
        public void CanMapCreateAllocationRequestDomainObjectToDatabaseEntity()
        {
            var personId = _faker.Random.Long();
            var createdBy = _faker.Internet.Email();
            var workerId = _faker.Random.Number();
            var dt = DateTime.Now;
            var caseStatus = "Open";

            var allocationRequest = new CreateAllocationRequest()
            {
                MosaicId = personId,
                CreatedBy = createdBy,
                AllocatedWorkerId = workerId
            };

            var expectedResponse = new AllocationSet()
            {
                PersonId = personId,
                WorkerId = workerId,
                AllocationStartDate = dt,
                CaseStatus = caseStatus,
                CreatedBy = createdBy
            };

            allocationRequest.ToEntity(workerId, dt, caseStatus).Should().BeEquivalentTo(expectedResponse);
        }

        [Test]
        public void CanMapOtherNameFromDomainToInfrastructure()
        {
            var firstName = _faker.Name.FirstName();
            var lastName = _faker.Name.LastName();
            var createdBy = _faker.Internet.Email();

            var domainOtherName = new OtherName()
            {
                FirstName = firstName,
                LastName = lastName
            };

            long personId = 123;

            var infrastructureOtherName = new PersonOtherName()
            {
                FirstName = firstName,
                LastName = lastName,
                PersonId = personId,
                CreatedBy = createdBy
            };

            domainOtherName.ToEntity(personId, createdBy).Should().BeEquivalentTo(infrastructureOtherName);
        }

        [Test]
        public void CanMapPhoneNumberFromDomainToInfrastructure()
        {
            string phoneNumber = "12345678";
            string phoneNumberType = "Mobile";
            long personId = 123;
            string createdBy = _faker.Internet.Email();

            var domainNumber = new PhoneNumber()
            {
                Number = phoneNumber,
                Type = phoneNumberType
            };

            var infrastructurePhoneNumber = new dbPhoneNumber()
            {
                Number = phoneNumber.ToString(),
                PersonId = personId,
                Type = phoneNumberType,
                CreatedBy = createdBy
            };

            domainNumber.ToEntity(personId, createdBy).Should().BeEquivalentTo(infrastructurePhoneNumber);
        }

        [Test]
        public void CanMapDbAddressToAddressDomain()
        {
            dbAddress address = DatabaseGatewayHelper.CreateAddressDatabaseEntity();

            AddressDomain expectedAddressDomain = new AddressDomain()
            {
                Address = address.AddressLines,
                Postcode = address.PostCode,
                Uprn = address.Uprn
            };

            EntityFactory.DbAddressToAddressDomain(address).Should().BeEquivalentTo(expectedAddressDomain);

        }

        [Test]
        public void CanMapCreateCaseNoteRequestToCaseNotesDocument()
        {
            CreateCaseNoteRequest request = _fixture
                .Build<CreateCaseNoteRequest>()
                .With(x => x.ContextFlag, _faker.Random.String2(1))
                .With(x => x.CaseFormData, "{\"prop_one\": \"value one\",  \"prop_two\": \"value two\"}")
                .Create();

            GenericCaseNote note = new GenericCaseNote()
            {
                DateOfBirth = request.DateOfBirth?.ToString("dd/MM/yyy"),
                DateOfEvent = request.DateOfEvent?.ToString(),
                FirstName = request.FirstName,
                LastName = request.LastName,
                FormName = request.FormName,
                FormNameOverall = request.FormNameOverall,
                WorkerEmail = request.WorkerEmail,
                MosaicId = request.PersonId.ToString()
            };

            var result = request.ToEntity();

            dynamic formData = JsonConvert.DeserializeObject(result.CaseFormData);

            //take the generated timestamp value and use it in the expected result
            note.Timestamp = formData["timestamp"];

            JObject coreProperties = JObject.Parse(JsonConvert.SerializeObject(note));

            coreProperties.Merge(JObject.Parse(request.CaseFormData));

            var expectedResult = new CaseNotesDocument()
            {
                CaseFormData = coreProperties.ToString()
            };

            result.Should().BeEquivalentTo(expectedResult);
        }

        [Test]
        public void CreateCaseNoteRequestToCaseNotesDocumentUsesCorrectDateTimeFormatForTimestamp()
        {
            CreateCaseNoteRequest request = _fixture
               .Build<CreateCaseNoteRequest>()
               .With(x => x.ContextFlag, _faker.Random.String2(1))
               .With(x => x.CaseFormData, "{\"prop_one\": \"value one\",  \"prop_two\": \"value two\"}")
               .Create();

            var result = request.ToEntity();

            dynamic formData = JsonConvert.DeserializeObject(result.CaseFormData);

            string timestamp = formData["timestamp"];

            (DateTime.TryParseExact(timestamp, "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date)).Should().BeTrue();
        }
    }
}

