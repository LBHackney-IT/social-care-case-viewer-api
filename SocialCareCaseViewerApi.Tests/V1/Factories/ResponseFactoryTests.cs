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
        public void CanMapHistoricalCaseNoteToBsonDocument()
        {
            var historicalCaseNote = TestHelper.CreateResidentHistoricRecordCaseNote();
            var expectedDocument = new BsonDocument(
            new List<BsonElement> {
                    new BsonElement("_id", historicalCaseNote.CaseNote.CaseNoteId),
                    new BsonElement("mosaic_id", historicalCaseNote.CaseNote.MosaicId),
                    new BsonElement("worker_email", historicalCaseNote.CaseNote.CreatedByEmail),
                    new BsonElement("form_name_overall", "Historical_Case_Note"),
                    new BsonElement("form_name", historicalCaseNote.CaseNoteTitle),
                    new BsonElement("timestamp", historicalCaseNote.DateOfEvent),
                    new BsonElement("is_historical", true)
            });

            var result = ResponseFactory.HistoricalCaseNotesToDomain(historicalCaseNote);

            result.Should().BeEquivalentTo(expectedDocument);
        }

        [Test]
        public void CanMapHistoricalVisitToBsonDocument()
        {
            var visit = TestHelper.CreateResidentHistoricRecordVisit();
            var expectedDocument = new BsonDocument(
            new List<BsonElement> {
                    new BsonElement("_id", visit.Visit.VisitId),
                    new BsonElement("worker_email", visit.Visit.CreatedByEmail),
                    new BsonElement("form_name_overall", "Historical_Visit"),
                    new BsonElement("form_name", "Historical Visit"),
                    new BsonElement("timestamp", visit.DateOfEvent),
                    new BsonElement("is_historical", true)
            });

            var result = ResponseFactory.HistoricalVisitsToDomain(visit);

            result.Should().BeEquivalentTo(expectedDocument);
        }

        [Test]
        public void CanMapHistoricalCaseNoteToCaseNoteResponse()
        {
            var historicalCaseNote = new CaseNote()
            {
                MosaicId = "1",
                CaseNoteId = "123",
                CaseNoteTitle = "Case Note Title",
                CaseNoteContent = "Some case note content.",
                CreatedByName = "John Smith",
                CreatedByEmail = "john.smith@email.com",
                NoteType = "Case Summary (ASC)",
                CreatedOn = new DateTime(2021, 3, 1, 15, 30, 0),
            };

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

            Assert.AreEqual(expectedCaseNoteResponse.RecordId, result.RecordId);
            Assert.AreEqual(expectedCaseNoteResponse.PersonId, result.PersonId);
            Assert.AreEqual(expectedCaseNoteResponse.Title, result.Title);
            Assert.AreEqual(expectedCaseNoteResponse.Content, result.Content);
            Assert.AreEqual(expectedCaseNoteResponse.DateOfEvent, result.DateOfEvent);
            Assert.AreEqual(expectedCaseNoteResponse.OfficerName, result.OfficerName);
            Assert.AreEqual(expectedCaseNoteResponse.OfficerEmail, result.OfficerEmail);
            Assert.AreEqual(expectedCaseNoteResponse.FormName, result.FormName);
        }
    }
}
