using System;
using System.Collections.Generic;
using FluentAssertions;
using MongoDB.Bson;
using NUnit.Framework;
using SocialCareCaseViewerApi.Tests.V1.Helpers;
using SocialCareCaseViewerApi.V1.Boundary.Response;
using SocialCareCaseViewerApi.V1.Domain;
using SocialCareCaseViewerApi.V1.Factories;
using SocialCareCaseViewerApi.V1.Infrastructure;
using Address = SocialCareCaseViewerApi.V1.Infrastructure.Address;
using PhoneNumber = SocialCareCaseViewerApi.V1.Infrastructure.PhoneNumber;

namespace SocialCareCaseViewerApi.Tests.V1.Factories
{
    public class ResponseFactoryTests
    {
        [Test]
        public void CanMapResidentAndAddressFromDomainToResponse()
        {
            var testDateOfBirth = DateTime.Now;
            const string caseNoteId = "1234ghjut";
            const string caseNoteErrorMessage = "Error";

            Person person = new Person
            {
                Id = 123,
                Title = "Mx",
                FirstName = "Ciascom",
                LastName = "Tesselate",
                Gender = "x",
                Nationality = "British",
                NhsNumber = 456,
                DateOfBirth = testDateOfBirth,
                AgeContext = "b"
            };

            Address newAddress = new Address() { AddressId = 345 };

            List<PersonOtherName> names = new List<PersonOtherName>
            {
                new PersonOtherName() { Id = 1 },
                new PersonOtherName() { Id = 2 }
            };

            List<PhoneNumber> phoneNumbers = new List<PhoneNumber>()
            {
                new PhoneNumber() { Id = 1 },
                new PhoneNumber() { Id = 2 },
            };

            var expectedResponse = new AddNewResidentResponse
            {
                PersonId = 123,
                AddressId = newAddress.AddressId,
                OtherNameIds = new List<int>() { 1, 2 },
                PhoneNumberIds = new List<int> { 1, 2 },
                CaseNoteId = caseNoteId,
                CaseNoteErrorMessage = caseNoteErrorMessage
            };

            person.ToResponse(newAddress, names, phoneNumbers, caseNoteId, caseNoteErrorMessage).Should().BeEquivalentTo(expectedResponse);
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
                    new BsonElement("form_name", historicalCaseNote.NoteType),
                    new BsonElement("title", historicalCaseNote.CaseNoteTitle),
                    new BsonElement("timestamp", historicalCaseNote.CreatedOn.ToString("dd/MM/yyyy H:mm:ss")),
                    new BsonElement("is_historical", true)
            });

            var result = ResponseFactory.HistoricalCaseNotesToDomain(historicalCaseNote);

            result.Should().BeEquivalentTo(expectedDocument);
        }

        [Test]
        public void WhenNoteTypeIsNullReturnsCaseNoteAsAString()
        {
            var historicalCaseNote = TestHelpers.CreateCaseNote();
            historicalCaseNote.NoteType = null;

            var result = ResponseFactory.HistoricalCaseNotesToDomain(historicalCaseNote);

            result.GetValue("form_name").AsString.Should().BeEquivalentTo("Case note");
        }

        [Test]
        public void WhenNoteTypeHasASCInBracketsReturnsFormNameWithoutThemAndWhitespace()
        {
            var historicalCaseNote = TestHelpers.CreateCaseNote();
            historicalCaseNote.NoteType = "Case Summary (ASC)";

            var result = ResponseFactory.HistoricalCaseNotesToDomain(historicalCaseNote);

            result.GetValue("form_name").AsString.Should().BeEquivalentTo("Case Summary");
        }

        [Test]
        public void WhenNoteTypeHasYOTInBracketsReturnsFormNameWithoutThemAndWhitespace()
        {
            var historicalCaseNote = TestHelpers.CreateCaseNote();
            historicalCaseNote.NoteType = "Home Visit (YOT)";

            var result = ResponseFactory.HistoricalCaseNotesToDomain(historicalCaseNote);

            result.GetValue("form_name").AsString.Should().BeEquivalentTo("Home Visit");
        }

        [Test]
        public void WhenNoteTypeHasYHInBracketsReturnsFormNameWithoutThemAndWhitespace()
        {
            var historicalCaseNote = TestHelpers.CreateCaseNote();
            historicalCaseNote.NoteType = "Manager's Decisions (YH)";

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
            var historicalCaseNote = TestHelpers.CreateCaseNote();
            historicalCaseNote.CreatedOn = new DateTime(2021, 3, 1, 15, 30, 0);

            var expectedCaseNoteResponse = new CaseNoteResponse()
            {
                RecordId = historicalCaseNote.CaseNoteId,
                PersonId = historicalCaseNote.MosaicId,
                Title = historicalCaseNote.CaseNoteTitle,
                Content = historicalCaseNote.CaseNoteContent,
                DateOfEvent = "2021-03-01T15:30:00",
                OfficerName = historicalCaseNote.CreatedByName,
                OfficerEmail = historicalCaseNote.CreatedByEmail,
                FormName = historicalCaseNote.NoteType
            };

            var result = ResponseFactory.ToResponse(historicalCaseNote);

            result.Should().BeEquivalentTo(expectedCaseNoteResponse);
        }
    }
}
