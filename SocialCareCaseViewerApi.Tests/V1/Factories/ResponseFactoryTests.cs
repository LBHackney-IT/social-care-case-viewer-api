using FluentAssertions;
using MongoDB.Bson;
using NUnit.Framework;
using SocialCareCaseViewerApi.Tests.V1.Helpers;
using SocialCareCaseViewerApi.V1.Boundary.Response;
using SocialCareCaseViewerApi.V1.Domain;
using SocialCareCaseViewerApi.V1.Factories;
using SocialCareCaseViewerApi.V1.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using Address = SocialCareCaseViewerApi.V1.Infrastructure.Address;
using AddressResponse = SocialCareCaseViewerApi.V1.Boundary.Response.Address;
using Person = SocialCareCaseViewerApi.V1.Infrastructure.Person;
using PhoneNumber = SocialCareCaseViewerApi.V1.Infrastructure.PhoneNumber;
using PhoneNumberDomain = SocialCareCaseViewerApi.V1.Domain.PhoneNumber;
using ResidentInformationResponse = SocialCareCaseViewerApi.V1.Boundary.Response.ResidentInformation;
using WarningNoteReview = SocialCareCaseViewerApi.V1.Infrastructure.WarningNoteReview;

namespace SocialCareCaseViewerApi.Tests.V1.Factories
{
    public class ResponseFactoryTests
    {
        [SetUp]
        public void SetUp()
        {
            Environment.SetEnvironmentVariable("SOCIAL_CARE_FIX_HISTORIC_CASE_NOTE_RESPONSE", "true");
        }

        [Test]
        public void CanMapResidentAndAddressFromDomainToResponse()
        {
            var caseNote = TestHelpers.CreateCaseNote();

            var person = TestHelpers.CreatePerson();
            var address = TestHelpers.CreateAddress(person.Id);

            var phoneNumber1 = TestHelpers.CreatePhoneNumber(person.Id);
            var phoneNumber2 = TestHelpers.CreatePhoneNumber(person.Id);

            var otherName1 = TestHelpers.CreatePersonOtherName(person.Id);
            var otherName2 = TestHelpers.CreatePersonOtherName(person.Id);

            var names = new List<PersonOtherName>
            {
                otherName1,
                otherName2
            };

            var phoneNumbers = new List<PhoneNumber>()
            {
                phoneNumber1,
                phoneNumber2
            };

            var residentResponse = TestHelpers.CreateAddNewResidentResponse(person.Id);

            var expectedResponse = new AddNewResidentResponse
            {
                Id = person.Id,
                AddressId = address.AddressId,
                OtherNameIds = new List<int>() { otherName1.Id, otherName2.Id },
                PhoneNumberIds = new List<int> { phoneNumber1.Id, phoneNumber2.Id },
                CaseNoteId = caseNote.CaseNoteId,
                CaseNoteErrorMessage = residentResponse.CaseNoteErrorMessage
            };

            person.ToResponse(address, names, phoneNumbers, caseNote.CaseNoteId, residentResponse.CaseNoteErrorMessage).Should().BeEquivalentTo(expectedResponse);
        }

        [Test]
        public void HistoricalCaseNotesToDomainReturnsCaseNoteMappedToDomain()
        {
            var historicalCaseNote = TestHelpers.CreateCaseNote();
            var expectedDocument = new BsonDocument(
            new List<BsonElement> {
                    new BsonElement("_id", historicalCaseNote.CaseNoteId),
                    new BsonElement("mosaic_id", historicalCaseNote.MosaicId),
                    new BsonElement("worker_email", historicalCaseNote.CreatedByEmail),
                    new BsonElement("form_name_overall", "Historical_Case_Note"),
                    new BsonElement("form_name", historicalCaseNote.NoteType),
                    new BsonElement("title", historicalCaseNote.CaseNoteTitle),
                    new BsonElement("timestamp", historicalCaseNote.CreatedOn.ToString("dd/MM/yyyy H:mm:ss")),
                    new BsonElement("is_historical", true)
            });

            var result = ResponseFactory.HistoricalCaseNotesToDomain(historicalCaseNote);

            result.Should().BeEquivalentTo(expectedDocument);
        }

        [Test]
        public void HistoricalCaseNotesToDomainReturnsNoteTypeForFormNameIfFeatureFlagIsOn()
        {
            Environment.SetEnvironmentVariable("SOCIAL_CARE_FIX_HISTORIC_CASE_NOTE_RESPONSE", "true");
            var historicalCaseNote = TestHelpers.CreateCaseNote();

            var result = ResponseFactory.HistoricalCaseNotesToDomain(historicalCaseNote);

            result.GetValue("form_name").AsString.Should().BeEquivalentTo(historicalCaseNote.NoteType);
        }

        [Test]
        public void HistoricalCaseNotesToDomainReturnsTitleForFormNameIfFeatureFlagIsFalse()
        {
            Environment.SetEnvironmentVariable("SOCIAL_CARE_FIX_HISTORIC_CASE_NOTE_RESPONSE", "false");
            var historicalCaseNote = TestHelpers.CreateCaseNote();

            var result = ResponseFactory.HistoricalCaseNotesToDomain(historicalCaseNote);

            result.GetValue("form_name").AsString.Should().BeEquivalentTo(historicalCaseNote.CaseNoteTitle);
        }

        [Test]
        public void HistoricalCaseNotesToDomainReturnsTitleForFormNameIfFeatureFlagIsAnEmptyString()
        {
            Environment.SetEnvironmentVariable("SOCIAL_CARE_FIX_HISTORIC_CASE_NOTE_RESPONSE", "");
            var historicalCaseNote = TestHelpers.CreateCaseNote();

            var result = ResponseFactory.HistoricalCaseNotesToDomain(historicalCaseNote);

            result.GetValue("form_name").AsString.Should().BeEquivalentTo(historicalCaseNote.CaseNoteTitle);
        }

        [Test]
        public void HistoricalCaseNotesToDomainReturnsTitleForFormNameIfFeatureFlagIsNull()
        {
            Environment.SetEnvironmentVariable("SOCIAL_CARE_FIX_HISTORIC_CASE_NOTE_RESPONSE", null);
            var historicalCaseNote = TestHelpers.CreateCaseNote();

            var result = ResponseFactory.HistoricalCaseNotesToDomain(historicalCaseNote);

            result.GetValue("form_name").AsString.Should().BeEquivalentTo(historicalCaseNote.CaseNoteTitle);
        }

        [Test]
        public void HistoricalCaseNotesToDomainReturnsCaseNoteAsAStringWhenNoteTypeIsNull()
        {
            var historicalCaseNote = TestHelpers.CreateCaseNote();
            historicalCaseNote.NoteType = null;

            var result = ResponseFactory.HistoricalCaseNotesToDomain(historicalCaseNote);

            result.GetValue("form_name").AsString.Should().BeEquivalentTo("Case note");
        }

        [Test]
        public void HistoricalCaseNotesToDomainReturnsCaseNoteAsAStringWhenNoteTypeIsAnEmptyString()
        {
            var historicalCaseNote = TestHelpers.CreateCaseNote(noteType: "");

            var result = ResponseFactory.HistoricalCaseNotesToDomain(historicalCaseNote);

            result.GetValue("form_name").AsString.Should().BeEquivalentTo("Case note");
        }

        [Test]
        public void HistoricalCaseNotesToDomainReturnsFormNameWithoutBracketsAndWhitespaceWhenNoteTypeHasASCInBrackets()
        {
            var historicalCaseNote = TestHelpers.CreateCaseNote(noteType: "Case Summary (ASC)");

            var result = ResponseFactory.HistoricalCaseNotesToDomain(historicalCaseNote);

            result.GetValue("form_name").AsString.Should().BeEquivalentTo("Case Summary");
        }

        [Test]
        public void HistoricalCaseNotesToDomainReturnsFormNameWithoutBracketsAndWhitespaceWhenNoteTypeHasYOTInBrackets()
        {
            var historicalCaseNote = TestHelpers.CreateCaseNote(noteType: "Home Visit (YOT)");

            var result = ResponseFactory.HistoricalCaseNotesToDomain(historicalCaseNote);

            result.GetValue("form_name").AsString.Should().BeEquivalentTo("Home Visit");
        }

        [Test]
        public void HistoricalCaseNotesToDomainReturnsFormNameWithoutBracketsAndWhitespaceWhenNoteTypeHasYHInBrackets()
        {
            var historicalCaseNote = TestHelpers.CreateCaseNote(noteType: "Manager's Decisions (YH)");

            var result = ResponseFactory.HistoricalCaseNotesToDomain(historicalCaseNote);

            result.GetValue("form_name").AsString.Should().BeEquivalentTo("Manager's Decisions");
        }

        [Test]
        public void CanMapVisitToBsonDocument()
        {
            var visit = TestHelpers.CreateVisit();
            var expectedDocument = new BsonDocument(
            new List<BsonElement> {
                    new BsonElement("_id", visit.VisitId.ToString()),
                    new BsonElement("mosaic_id", visit.PersonId.ToString()),
                    new BsonElement("worker_email", visit.CreatedByEmail),
                    new BsonElement("form_name_overall", "Historical_Visit"),
                    new BsonElement("form_name", $"Historical Visit - {visit.VisitType}"),
                    new BsonElement("timestamp", visit.ActualDateTime?.ToString("dd/MM/yyyy H:mm:ss") ??
                                                 visit.PlannedDateTime?.ToString("dd/MM/yyyy H:mm:ss")),
                    new BsonElement("is_historical", true)
            });

            var result = ResponseFactory.HistoricalVisitsToDomain(visit);

            result.Should().BeEquivalentTo(expectedDocument);
        }

        [Test]
        public void ToResponseForCaseNoteReturnsCaseNoteMappedToCaseNoteResponse()
        {
            var historicalCaseNote = TestHelpers.CreateCaseNote(createdOn: new DateTime(2021, 3, 1, 15, 30, 0));
            var expectedCaseNoteResponse = TestHelpers.CreateCaseNoteResponse(historicalCaseNote);

            var result = ResponseFactory.ToResponse(historicalCaseNote);

            result.Should().BeEquivalentTo(expectedCaseNoteResponse);
        }

        [Test]
        public void ToResponseForCaseNoteReturnsCaseNoteAsAStringForFormNameWhenNoteTypeIsNull()
        {
            var historicalCaseNote = TestHelpers.CreateCaseNote();
            historicalCaseNote.NoteType = null;

            var result = ResponseFactory.ToResponse(historicalCaseNote);

            result.FormName.Should().Be("Case note");
        }

        [Test]
        public void ToResponseForCaseNoteReturnsCaseNoteAsAStringForFormNameWhenNoteTypeIsAnEmptyString()
        {
            var historicalCaseNote = TestHelpers.CreateCaseNote(noteType: "");

            var result = ResponseFactory.ToResponse(historicalCaseNote);

            result.FormName.Should().Be("Case note");
        }

        [Test]
        public void ToResponseForCaseNoteReturnsNoteTypeWithoutBracketsAndWhitespaceForFormName()
        {
            var historicalCaseNote = TestHelpers.CreateCaseNote(noteType: "Manager's Decisions (YH)");

            var result = ResponseFactory.ToResponse(historicalCaseNote);

            result.FormName.Should().Be("Manager's Decisions");
        }

        [Test]
        public void CanMapDomainWorkerToResponse()
        {
            var domainWorker = TestHelpers.CreateWorker().ToDomain(true);
            var expectedResponse = new WorkerResponse
            {
                Id = domainWorker.Id,
                Role = domainWorker.Role,
                Email = domainWorker.Email,
                Teams = domainWorker.Teams,
                AllocationCount = domainWorker.AllocationCount,
                ContextFlag = domainWorker.ContextFlag,
                CreatedBy = domainWorker.CreatedBy,
                DateStart = domainWorker.DateStart?.ToString("s") ?? "",
                FirstName = domainWorker.FirstName,
                LastName = domainWorker.LastName
            };

            var response = domainWorker.ToResponse();

            response.Should().BeEquivalentTo(expectedResponse);
        }

        [Test]
        public void CanMapPersonDetailsToGetPersonResponse()
        {
            Person person = DatabaseGatewayHelper.CreatePersonDatabaseEntity();
            person.Id = 123;

            Address address = DatabaseGatewayHelper.CreateAddressDatabaseEntity(person.Id);

            //set to display address
            address.IsDisplayAddress = "Y";

            PhoneNumber phoneNumber1 = DatabaseGatewayHelper.CreatePhoneNumberEntity(person.Id);
            PhoneNumber phoneNumber2 = DatabaseGatewayHelper.CreatePhoneNumberEntity(person.Id);

            PersonOtherName otherName1 = DatabaseGatewayHelper.CreatePersonOtherNameDatabaseEntity(person.Id);
            PersonOtherName otherName2 = DatabaseGatewayHelper.CreatePersonOtherNameDatabaseEntity(person.Id);

            person.Addresses = new List<Address>
            {
                address
            };

            person.PhoneNumbers = new List<PhoneNumber>
            {
                phoneNumber1,
                phoneNumber2
            };

            person.OtherNames = new List<PersonOtherName>
            {
                otherName1,
                otherName2
            };

            AddressDomain addressDomain = new AddressDomain()
            {
                Address = address.AddressLines,
                Postcode = address.PostCode,
                Uprn = address.Uprn
            };

            OtherName personOtherName1 = new OtherName()
            {
                FirstName = otherName1.FirstName,
                LastName = otherName1.LastName
            };

            OtherName personOtherName2 = new OtherName()
            {
                FirstName = otherName2.FirstName,
                LastName = otherName2.LastName
            };

            PhoneNumberDomain phoneNumberDomain1 = new PhoneNumberDomain()
            {
                Number = phoneNumber1.Number,
                Type = phoneNumber1.Type
            };

            PhoneNumberDomain phoneNumberDomain2 = new PhoneNumberDomain()
            {
                Number = phoneNumber2.Number,
                Type = phoneNumber2.Type
            };

            var expectedResponse = new GetPersonResponse()
            {
                GenderAssignedAtBirth = person.GenderAssignedAtBirth,
                PreferredLanguage = person.PreferredLanguage,
                Pronoun = person.Pronoun,
                EmailAddress = person.EmailAddress,
                DateOfBirth = person.DateOfBirth.Value,
                DateOfDeath = person.DateOfDeath.Value,
                Address = addressDomain,
                SexualOrientation = person.SexualOrientation,
                ContextFlag = person.AgeContext,
                CreatedBy = person.CreatedBy,
                Ethnicity = person.Ethnicity,
                FirstLanguage = person.FirstLanguage,
                FirstName = person.FirstName,
                Gender = person.Gender,
                LastName = person.LastName,
                NhsNumber = person.NhsNumber.Value,
                Id = person.Id,
                PreferredMethodOfContact = person.PreferredMethodOfContact,
                Religion = person.Religion,
                Restricted = person.Restricted,
                Title = person.Title,
                OtherNames = new List<OtherName>() { personOtherName1, personOtherName2 },
                PhoneNumbers = new List<PhoneNumberDomain>() { phoneNumberDomain1, phoneNumberDomain2 }
            };

            var result = ResponseFactory.ToResponse(person);

            result.Should().BeEquivalentTo(expectedResponse);
        }

        [Test]
        public void MapWarningNoteToResponseReturnsAnObjectWithFormattedDates()
        {
            var dbWarningNote = TestHelpers.CreateWarningNote();
            var dbWarningNoteReview = TestHelpers.CreateWarningNoteReview(dbWarningNote.Id);
            var reviewList = new List<WarningNoteReview> { dbWarningNoteReview };

            var expectedWarningReviewResponse = new WarningNoteReviewResponse
            {
                Id = dbWarningNoteReview.Id,
                WarningNoteId = dbWarningNoteReview.WarningNoteId,
                ReviewDate = dbWarningNoteReview.ReviewDate?.ToString("s"),
                DisclosedWithIndividual = dbWarningNoteReview.DisclosedWithIndividual,
                ReviewNotes = dbWarningNoteReview.ReviewNotes,
                ManagerName = dbWarningNoteReview.ManagerName,
                DiscussedWithManagerDate = dbWarningNoteReview.DiscussedWithManagerDate?.ToString("s"),
                CreatedAt = dbWarningNoteReview.CreatedAt?.ToString("s"),
                CreatedBy = dbWarningNoteReview.CreatedBy,
                LastModifiedAt = dbWarningNoteReview.LastModifiedAt?.ToString("s"),
                LastModifiedBy = dbWarningNoteReview.LastModifiedBy
            };

            var expectedResponse = new WarningNoteResponse
            {
                Id = dbWarningNote.Id,
                PersonId = dbWarningNote.PersonId,
                StartDate = dbWarningNote.StartDate?.ToString("s"),
                EndDate = dbWarningNote.EndDate?.ToString("s"),
                DisclosedWithIndividual = dbWarningNote.DisclosedWithIndividual,
                DisclosedDetails = dbWarningNote.DisclosedDetails,
                Notes = dbWarningNote.Notes,
                ReviewDate = dbWarningNote.ReviewDate?.ToString("s"),
                NextReviewDate = dbWarningNote.NextReviewDate?.ToString("s"),
                NoteType = dbWarningNote.NoteType,
                Status = dbWarningNote.Status,
                DisclosedDate = dbWarningNote.DisclosedDate?.ToString("s"),
                DisclosedHow = dbWarningNote.DisclosedHow,
                WarningNarrative = dbWarningNote.WarningNarrative,
                ManagerName = dbWarningNote.ManagerName,
                DiscussedWithManagerDate = dbWarningNote.DiscussedWithManagerDate?.ToString("s"),
                CreatedBy = dbWarningNote.CreatedBy,
                WarningNoteReviews = new List<WarningNoteReviewResponse> { expectedWarningReviewResponse }
            };

            var domainWarningNote = dbWarningNote.ToDomain(reviewList);

            var result = domainWarningNote.ToResponse();

            result.Should().BeEquivalentTo(expectedResponse);
        }

        [Test]
        public void CanMapTeamToTeamResponse()
        {
            var domainTeam = TestHelpers.CreateTeam().ToDomain();

            var expectedResponse = new TeamResponse
            {
                Id = domainTeam.Id,
                Context = domainTeam.Context,
                Name = domainTeam.Name
            };

            domainTeam.ToResponse().Should().BeEquivalentTo(expectedResponse);
        }

        [Test]
        public void CanMapDomainCaseSubmissionToResponse()
        {
            var domainCaseSubmission = TestHelpers.CreateCaseSubmission().ToDomain();

            var responseCaseSubmission = new CaseSubmissionResponse
            {
                SubmissionId = domainCaseSubmission.SubmissionId,
                FormId = domainCaseSubmission.FormId,
                Residents = domainCaseSubmission.Residents,
                Workers = domainCaseSubmission.Workers.Select(w => w.ToResponse()).ToList(),
                CreatedAt = domainCaseSubmission.CreatedAt.ToString("O"),
                CreatedBy = domainCaseSubmission.CreatedBy.ToResponse(),
                EditHistory = domainCaseSubmission.EditHistory.Select(e => new EditHistory<WorkerResponse>
                {
                    EditTime = e.EditTime,
                    Worker = e.Worker.ToResponse()
                }).ToList(),
                SubmissionState = domainCaseSubmission.SubmissionState,
                FormAnswers = domainCaseSubmission.FormAnswers,
                Title = domainCaseSubmission.Title,
                Deleted = domainCaseSubmission.Deleted,
                DeletionDetails = domainCaseSubmission.DeletionDetails
            };

            domainCaseSubmission.ToResponse().Should().BeEquivalentTo(responseCaseSubmission);
        }

        [Test]
        public void CanMapAPersonDatabaseRecordIntoResidentInformationResponseObject()
        {
            var personRecord = TestHelpers.CreatePerson();

            var response = personRecord.ToResidentInformationResponse();

            response.Should().BeEquivalentTo(new ResidentInformationResponse
            {
                MosaicId = personRecord.Id.ToString(),
                FirstName = personRecord.FirstName,
                LastName = personRecord.LastName,
                NhsNumber = personRecord.NhsNumber.ToString(),
                DateOfBirth = personRecord.DateOfBirth?.ToString("O"), //keep format for backwards compatibility
                AgeContext = personRecord.AgeContext,
                Nationality = personRecord.Nationality,
                Gender = personRecord.Gender,
                Restricted = personRecord.Restricted,
                AddressList = null,
                PhoneNumber = null,
                Uprn = response.Uprn
            });
        }

        [Test]
        public void CanMapAddressFromInfrastructureToResponse()
        {
            var dbAddress = TestHelpers.CreateAddress();

            var expectedResponse = new AddressResponse()
            {
                AddressLine1 = dbAddress.AddressLines,
                DisplayAddressFlag = dbAddress.IsDisplayAddress,
                EndDate = dbAddress.EndDate,
                PostCode = dbAddress.PostCode,
                AddressLine2 = null, //not used, left for backwards compatibility
                AddressLine3 = null, //not used, left for backwards compatibility
                ContactAddressFlag = null //not used, left for backwards compatibility
            };

            var response = dbAddress.ToResponse();

            response.Should().BeEquivalentTo(expectedResponse);
        }

        [Test]
        public void CanMapPhoneNumberFromInfrastructureToResponse()
        {
            var dbPhoneNumber = TestHelpers.CreatePhoneNumber();

            var expectedResponse = new Phone()
            {
                PhoneNumber = dbPhoneNumber.Number,
                PhoneType = dbPhoneNumber.Type
            };

            var response = dbPhoneNumber.ToResponse();

            response.Should().BeEquivalentTo(expectedResponse);
        }
    }
}
