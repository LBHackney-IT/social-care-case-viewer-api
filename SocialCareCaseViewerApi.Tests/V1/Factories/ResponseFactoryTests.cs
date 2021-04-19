using System.Collections.Generic;
using FluentAssertions;
using MongoDB.Bson;
using NUnit.Framework;
using SocialCareCaseViewerApi.Tests.V1.Helpers;
using SocialCareCaseViewerApi.V1.Boundary.Response;
using SocialCareCaseViewerApi.V1.Factories;
using SocialCareCaseViewerApi.V1.Infrastructure;
using PhoneNumber = SocialCareCaseViewerApi.V1.Infrastructure.PhoneNumber;

namespace SocialCareCaseViewerApi.Tests.V1.Factories
{
    public class ResponseFactoryTests
    {
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
                PersonId = person.Id,
                AddressId = address.AddressId,
                OtherNameIds = new List<int>() { otherName1.Id, otherName2.Id },
                PhoneNumberIds = new List<int> { phoneNumber1.Id, phoneNumber2.Id },
                CaseNoteId = caseNote.CaseNoteId,
                CaseNoteErrorMessage = residentResponse.CaseNoteErrorMessage
            };

            person.ToResponse(address, names, phoneNumbers, caseNote.CaseNoteId, residentResponse.CaseNoteErrorMessage).Should().BeEquivalentTo(expectedResponse);
        }

        [Test]
        public void CanMapCaseNoteToBsonDocument()
        {
            var historicalCaseNote = TestHelpers.CreateCaseNote();
            var expectedDocument = new BsonDocument(
            new List<BsonElement> {
                    new BsonElement("_id", historicalCaseNote.CaseNoteId),
                    new BsonElement("mosaic_id", historicalCaseNote.MosaicId),
                    new BsonElement("worker_email", historicalCaseNote.CreatedByEmail),
                    new BsonElement("form_name_overall", "Historical_Case_Note"),
                    new BsonElement("form_name", historicalCaseNote.CaseNoteTitle),
                    new BsonElement("timestamp", historicalCaseNote.CreatedOn.ToString("dd/MM/yyyy H:mm:ss")),
                    new BsonElement("is_historical", true)
            });

            var result = ResponseFactory.HistoricalCaseNotesToDomain(historicalCaseNote);

            result.Should().BeEquivalentTo(expectedDocument);
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
        public void CanMapHistoricalCaseNoteToCaseNoteResponse()
        {
            var historicalCaseNote = TestHelpers.CreateCaseNote();
            var expectedCaseNoteResponse = new CaseNoteResponse()
            {
                RecordId = historicalCaseNote.CaseNoteId,
                PersonId = historicalCaseNote.MosaicId,
                Title = historicalCaseNote.CaseNoteTitle,
                Content = historicalCaseNote.CaseNoteContent,
                DateOfEvent = historicalCaseNote.CreatedOn.ToString("s"),
                OfficerName = historicalCaseNote.CreatedByName,
                OfficerEmail = historicalCaseNote.CreatedByEmail,
                FormName = historicalCaseNote.NoteType
            };

            var result = ResponseFactory.ToResponse(historicalCaseNote);

            result.Should().BeEquivalentTo(expectedCaseNoteResponse);
        }
    }
}
