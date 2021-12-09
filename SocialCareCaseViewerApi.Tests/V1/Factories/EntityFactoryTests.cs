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
using Person = SocialCareCaseViewerApi.V1.Infrastructure.Person;

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
            var dbWorker = TestHelpers.CreateWorker();

            var expectedResponse = new Worker()
            {
                FirstName = dbWorker.FirstName,
                LastName = dbWorker.LastName,
                Id = dbWorker.Id,
                AllocationCount = dbWorker.Allocations?.Count(allocation => allocation.CaseStatus.ToUpper() == "OPEN") ?? 0,
                Email = dbWorker.Email,
                Role = dbWorker.Role,
                ContextFlag = dbWorker.ContextFlag,
                CreatedBy = dbWorker.CreatedBy,
                DateStart = dbWorker.DateStart,
                Teams = new List<Team>()
            };

            dbWorker.ToDomain(false).Should().BeEquivalentTo(expectedResponse);
        }

        [Test]
        public void CanMapWorkerFromInfrastructureToDomainWithTeamDetails()
        {
            var workers = new List<dbWorker>
            {
                TestHelpers.CreateWorker(), TestHelpers.CreateWorker(hasAllocations: false, hasWorkerTeams: false)
            };

            var expectedResponse = new List<Worker>
            {
                new Worker
                {
                    Id = workers[0].Id,
                    Email = workers[0].Email,
                    Role = workers[0].Role,
                    ContextFlag = workers[0].ContextFlag,
                    CreatedBy = workers[0].CreatedBy,
                    FirstName = workers[0].FirstName,
                    LastName = workers[0].LastName,
                    DateStart = workers[0].DateStart,
                    AllocationCount =
                        workers[0].Allocations?.Count(allocation => allocation.CaseStatus.ToUpper() == "OPEN") ?? 0,
                    Teams = workers[0].WorkerTeams?.Select(x =>
                        new Team() {Id = x.Team.Id, Name = x.Team.Name, Context = x.Team.Context}).ToList() ?? new List<Team>()
                },
                new Worker
                {
                    Id = workers[1].Id,
                    Email = workers[1].Email,
                    Role = workers[1].Role,
                    ContextFlag = workers[1].ContextFlag,
                    CreatedBy = workers[1].CreatedBy,
                    FirstName = workers[1].FirstName,
                    LastName = workers[1].LastName,
                    DateStart = workers[1].DateStart,
                    AllocationCount = 0,
                    Teams = new List<Team>()
                },
            };

            workers.Select(w => w.ToDomain(true)).Should().BeEquivalentTo(expectedResponse);
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
            var databaseCaseSubmission1 = TestHelpers.CreateCaseSubmission(SubmissionState.InProgress, title: null, deleted: true);
            var domainCaseSubmission1 = new DomainCaseSubmission()
            {
                SubmissionId = databaseCaseSubmission1.SubmissionId.ToString(),
                FormId = databaseCaseSubmission1.FormId,
                Residents = databaseCaseSubmission1.Residents,
                Workers = databaseCaseSubmission1.Workers.Select(w => w.ToDomain(false)).ToList(),
                CreatedAt = databaseCaseSubmission1.CreatedAt,
                CreatedBy = databaseCaseSubmission1.CreatedBy.ToDomain(false),
                EditHistory = databaseCaseSubmission1.EditHistory.Select(e => new EditHistory<Worker>
                {
                    EditTime = e.EditTime,
                    Worker = e.Worker.ToDomain(false)
                }).ToList(),
                SubmissionState = "In progress",
                FormAnswers = databaseCaseSubmission1.FormAnswers,
                Title = null,
                LastEdited = databaseCaseSubmission1.EditHistory.Last().EditTime,
                CompletedSteps = 1,
                Deleted = true,
                DeletionDetails = databaseCaseSubmission1.DeletionDetails
            };

            var databaseCaseSubmission2 = TestHelpers.CreateCaseSubmission(SubmissionState.Submitted, title: "test-title", deleted: false);
            var domainCaseSubmission2 = new DomainCaseSubmission()
            {
                SubmissionId = databaseCaseSubmission2.SubmissionId.ToString(),
                FormId = databaseCaseSubmission2.FormId,
                Residents = databaseCaseSubmission2.Residents,
                Workers = databaseCaseSubmission2.Workers.Select(w => w.ToDomain(false)).ToList(),
                CreatedAt = databaseCaseSubmission2.CreatedAt,
                CreatedBy = databaseCaseSubmission2.CreatedBy.ToDomain(false),
                EditHistory = databaseCaseSubmission2.EditHistory.Select(e => new EditHistory<Worker>
                {
                    EditTime = e.EditTime,
                    Worker = e.Worker.ToDomain(false)
                }).ToList(),
                SubmissionState = "Submitted",
                FormAnswers = databaseCaseSubmission2.FormAnswers,
                Title = "test-title",
                LastEdited = databaseCaseSubmission2.EditHistory.Last().EditTime,
                CompletedSteps = 1,
                Deleted = false,
                DeletionDetails = null
            };

            databaseCaseSubmission1.ToDomain().Should().BeEquivalentTo(domainCaseSubmission1);
            databaseCaseSubmission2.ToDomain().Should().BeEquivalentTo(domainCaseSubmission2);
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
                Number = phoneNumber,
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

        [Test]
        public void CaseSubmissionToCareCaseDataReturnsAssociatedResidentInformation()
        {
            var residents = new List<Person> { TestHelpers.CreatePerson(), TestHelpers.CreatePerson() };
            var request = TestHelpers.CreateListCasesRequest(residents[0].Id);
            var submission = TestHelpers.CreateCaseSubmission(residents: residents);

            var response = submission.ToCareCaseData(request);

            response.PersonId.Should().Be(residents[0].Id);
            response.FirstName.Should().Be(residents[0].FirstName);
            response.LastName.Should().Be(residents[0].LastName);
        }

        [Test]
        public void CaseSubmissionToCareCaseDataReturnsFirstResidentInformationWhenNullMosaicId()
        {
            var residents = new List<Person> { TestHelpers.CreatePerson(), TestHelpers.CreatePerson() };
            var request = TestHelpers.CreateListCasesRequest();
            var submission = TestHelpers.CreateCaseSubmission(residents: residents);

            var response = submission.ToCareCaseData(request);

            response.PersonId.Should().Be(residents[0].Id);
            response.FirstName.Should().Be(residents[0].FirstName);
            response.LastName.Should().Be(residents[0].LastName);
        }

        [Test]
        public void CaseSubmissionToCareCaseDataReturnsOfficeEmailOfFirstWorkerAssociatedWithCaseSubmission()
        {
            var residents = new List<Person> { TestHelpers.CreatePerson() };
            var workers = new List<dbWorker> { TestHelpers.CreateWorker(), TestHelpers.CreateWorker() };
            var request = TestHelpers.CreateListCasesRequest(residents[0].Id);
            var submission = TestHelpers.CreateCaseSubmission(workers: workers, residents: residents);

            var response = submission.ToCareCaseData(request);

            response.OfficerEmail.Should().Be(workers[0].Email);
        }

        [Test]
        public void CaseSubmissionToCareCaseDataReturnsCaseFormTimeStampFromSubmittedAtIfNotNull()
        {
            var residents = new List<Person> { TestHelpers.CreatePerson() };
            var submittedAt = new DateTime(2021, 07, 20, 14, 40, 30);
            var request = TestHelpers.CreateListCasesRequest(residents[0].Id);
            var submission = TestHelpers.CreateCaseSubmission(submittedAt: submittedAt, residents: residents);

            var response = submission.ToCareCaseData(request);

            response.CaseFormTimestamp.Should().Be("2021-07-20");
        }

        [Test]
        public void CaseSubmissionToCareCaseDataReturnsCaseFormTimeStampAsDateTimeNowIfSubmittedAtNull()
        {
            var residents = new List<Person> { TestHelpers.CreatePerson() };
            var request = TestHelpers.CreateListCasesRequest(residents[0].Id);
            var submission = TestHelpers.CreateCaseSubmission(residents: residents, submittedAt: null);

            var response = submission.ToCareCaseData(request);

            response.CaseFormTimestamp.Should().Be(DateTime.Now.ToString("yyyy-MM-dd"));
        }

        [Test]
        public void CaseSubmissionToCareCaseDataReturnsDateOfEventIfNotNull()
        {
            var residents = new List<Person> { TestHelpers.CreatePerson() };
            var dateOfEvent = new DateTime(2021, 07, 19, 14, 40, 30);
            var request = TestHelpers.CreateListCasesRequest(residents[0].Id);
            var submission = TestHelpers.CreateCaseSubmission(dateOfEvent: dateOfEvent, residents: residents);

            var response = submission.ToCareCaseData(request);

            response.DateOfEvent.Should().Be("2021-07-19T14:40:30.0000000");
        }

        [Test]
        public void CaseSubmissionToCareCaseDataReturnsDateOfEventAsCreatedAtIfDateOfEventNull()
        {
            var createdAt = new DateTime(2021, 07, 18, 14, 40, 30);
            var residents = new List<Person> { TestHelpers.CreatePerson() };
            var request = TestHelpers.CreateListCasesRequest(residents[0].Id);
            var submission = TestHelpers.CreateCaseSubmission(residents: residents, dateOfEvent: null, createdAt: createdAt);

            var response = submission.ToCareCaseData(request);

            response.DateOfEvent.Should().Be("2021-07-18T14:40:30.0000000");
        }

        [Test]
        public void CaseSubmissionToCareCaseDataReturnsDeletedSetToFalseByDefault()
        {
            var request = TestHelpers.CreateListCasesRequest();
            var submission = TestHelpers.CreateCaseSubmission();

            var response = submission.ToCareCaseData(request);

            response.Deleted.Should().BeFalse();
        }

        [Test]
        [TestCase(true)]
        [TestCase(false)]
        public void CaseSubmissionToCareCaseDataReturnsDeletedSetIfNotNull(bool deleted)
        {
            var request = TestHelpers.CreateListCasesRequest();
            var submission = TestHelpers.CreateCaseSubmission(deleted: deleted);

            var response = submission.ToCareCaseData(request);

            response.Deleted.Should().Be(deleted);
        }

        [Test]
        public void CaseSubmissionToCareCaseDataResturnsDeletetionDetailsIfDeletedIsTrueAndIncludeDeletedRecordsInRequestIsTrue()
        {
            var request = TestHelpers.CreateListCasesRequest(includeDeletedRecords: true);
            var submission = TestHelpers.CreateCaseSubmission(deleted: true);

            var response = submission.ToCareCaseData(request);

            var expectedDeletionDetails = new DeletionDetails()
            {
                DeletedAt = submission.DeletionDetails.DeletedAt,
                DeletedBy = submission.DeletionDetails.DeletedBy,
                DeleteReason = submission.DeletionDetails.DeleteReason,
                DeleteRequestedBy = submission.DeletionDetails.DeleteRequestedBy
            };

            response.DeletionDetails.Should().BeEquivalentTo(expectedDeletionDetails);
        }

        [Test]
        public void CaseSubmissionToCareCaseDataReturnsNullForDeletionDetailsIfdIncludeDeletedRecordsInRequestIsFalse()
        {
            var request = TestHelpers.CreateListCasesRequest(includeDeletedRecords: false);
            var submission = TestHelpers.CreateCaseSubmission(deleted: true);

            var response = submission.ToCareCaseData(request);

            response.DeletionDetails.Should().BeNull();
        }


        [Test]
        public void ConvertMashReferralFromInfrastructureToDomain2()
        {
            var infrastructureReferral = TestHelpers.CreateMashReferral2();

            var domainReferral = infrastructureReferral.ToDomain();

            domainReferral.Should().BeEquivalentTo(new SocialCareCaseViewerApi.V1.Domain.MashReferral
            {
                Id = infrastructureReferral.Id,
                Referrer = infrastructureReferral.Referrer,
                Stage = infrastructureReferral.Stage,
                ReferralCreatedAt = infrastructureReferral.ReferralCreatedAt,
                FinalDecision = infrastructureReferral.FinalDecision,
                ContactDecisionCreatedAt = infrastructureReferral.ContactDecisionCreatedAt,
                ContactDecisionUrgentContactRequired = infrastructureReferral.ContactDecisionUrgentContactRequired,
                InitialDecision = infrastructureReferral.InitialDecision,
                InitialDecisionReferralCategory = infrastructureReferral.InitialDecisionReferralCategory,
                InitialDecisionCreatedAt = infrastructureReferral.InitialDecisionCreatedAt,
                InitialDecisionUrgentContactRequired = infrastructureReferral.InitialDecisionUrgentContactRequired,
                ScreeningDecision = infrastructureReferral.ScreeningDecision,
                ScreeningCreatedAt = infrastructureReferral.ScreeningCreatedAt,
                ScreeningUrgentContactRequired = infrastructureReferral.ScreeningUrgentContactRequired,
                FinalDecisionReferralCategory = infrastructureReferral.FinalDecisionReferralCategory,
                FinalDecisionCreatedAt = infrastructureReferral.FinalDecisionCreatedAt,
                FinalDecisionUrgentContactRequired = infrastructureReferral.FinalDecisionUrgentContactRequired,
                ReferralCategory = infrastructureReferral.ReferralCategory,
                RequestedSupport = infrastructureReferral.RequestedSupport,
                ReferralDocumentURI = infrastructureReferral.ReferralDocumentURI,
                MashResidents = infrastructureReferral.MashResidents.Select(x => x.ToDomain()).ToList()
            });
        }


        [Test]
        public void ConvertCaseStatusInfrastructureToDomain()
        {
            var infraStructureCaseStatus = TestHelpers.CreateCaseStatus();
            infraStructureCaseStatus.Answers = TestHelpers.CreateCaseStatusAnswers(caseStatusId: infraStructureCaseStatus.Id);

            var domainCaseStatus = infraStructureCaseStatus.ToDomain();

            domainCaseStatus.Should().BeEquivalentTo(new SocialCareCaseViewerApi.V1.Domain.CaseStatus()
            {
                EndDate = infraStructureCaseStatus.EndDate,
                Id = infraStructureCaseStatus.Id,
                StartDate = infraStructureCaseStatus.StartDate,
                Notes = infraStructureCaseStatus.Notes,
                Person = infraStructureCaseStatus.Person,
                Type = infraStructureCaseStatus.Type,
                Answers = infraStructureCaseStatus.Answers.Select(a => a.ToDomain()).ToList()
            });
        }

        [Test]
        public void ConvertCaseStatusAnswerInfrastructureToDomain()
        {
            var infraStructureCaseStatusAnswer = TestHelpers.CreateCaseStatusAnswers(min: 1, max: 1).FirstOrDefault();

            var domainCaseStatusAnswer = infraStructureCaseStatusAnswer.ToDomain();

            domainCaseStatusAnswer.Should().BeEquivalentTo(new SocialCareCaseViewerApi.V1.Domain.CaseStatusAnswer()
            {

                Option = infraStructureCaseStatusAnswer.Option,
                Value = infraStructureCaseStatusAnswer.Value,
                StartDate = infraStructureCaseStatusAnswer.StartDate,
                CreatedAt = infraStructureCaseStatusAnswer.CreatedAt.Value,
                GroupId = infraStructureCaseStatusAnswer.GroupId,
                EndDate = infraStructureCaseStatusAnswer.EndDate,
                DiscardedAt = infraStructureCaseStatusAnswer.DiscardedAt
            });
        }

        [Test]
        public void CanConvertCaseStatusAnswerInfrastructureToDomainWhenNullableValuesInInfrastructureObjectAreNull()
        {
            var infraStructureCaseStatusAnswer = TestHelpers.CreateCaseStatusAnswers(min: 1, max: 1).FirstOrDefault();
            infraStructureCaseStatusAnswer.Option = null;
            infraStructureCaseStatusAnswer.Value = null;
            infraStructureCaseStatusAnswer.DiscardedAt = null;
            infraStructureCaseStatusAnswer.EndDate = null;
            infraStructureCaseStatusAnswer.CreatedAt = null;
            infraStructureCaseStatusAnswer.LastModifiedAt = null;

            var domainCaseStatusAnswer = infraStructureCaseStatusAnswer.ToDomain();

            domainCaseStatusAnswer.Should().BeEquivalentTo(new SocialCareCaseViewerApi.V1.Domain.CaseStatusAnswer()
            {

                Option = infraStructureCaseStatusAnswer.Option,
                Value = infraStructureCaseStatusAnswer.Value,
                StartDate = infraStructureCaseStatusAnswer.StartDate,
                CreatedAt = infraStructureCaseStatusAnswer.CreatedAt,
                GroupId = infraStructureCaseStatusAnswer.GroupId,
                EndDate = infraStructureCaseStatusAnswer.EndDate,
                DiscardedAt = infraStructureCaseStatusAnswer.DiscardedAt
            });
        }
    }
}

