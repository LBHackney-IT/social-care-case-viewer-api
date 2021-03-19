using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using MongoDB.Bson;
using NUnit.Framework;
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
            string caseNoteId = "1234ghjut";
            string caseNoteErrorMessage = "Error";

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
            string caseNoteId = "1";
            string email = "first.last@domain.com";
            DateTime createdOn = DateTime.Now;
            string noteType = "Historical note";
            string noteTitle = "My title";

            CaseNote historicalCaseNote = new CaseNote()
            {
                CaseNoteId = caseNoteId,
                CreatedByEmail = email,
                NoteType = noteType,
                CreatedOn = createdOn,
                CaseNoteTitle = noteTitle
            };

            var expectedDocument = new BsonDocument(
                        new List<BsonElement>
                        {
                                new BsonElement("_id", caseNoteId),
                                new BsonElement("worker_email", email),
                                new BsonElement("form_name_overall", "Historical_Case_Note"),
                                new BsonElement("form_name", noteTitle),
                                new BsonElement("timestamp", createdOn.ToString("dd/MM/yyyy H:mm:ss")),
                                new BsonElement("is_historical", true)
                        });

            List<CaseNote> notes = new List<CaseNote>() { historicalCaseNote };

            var result = ResponseFactory.HistoricalCaseNotesToDomain(notes);

            Assert.AreEqual(expectedDocument.GetElement("_id"), result.First().GetElement("_id"));
            Assert.AreEqual(expectedDocument.GetElement("worker_email"), result.First().GetElement("worker_email"));
            Assert.AreEqual(expectedDocument.GetElement("form_name_overall"), result.First().GetElement("form_name_overall"));
            Assert.AreEqual(expectedDocument.GetElement("form_name"), result.First().GetElement("form_name"));
            Assert.AreEqual(expectedDocument.GetElement("timestamp"), result.First().GetElement("timestamp"));
            Assert.AreEqual(expectedDocument.GetElement("is_historical"), result.First().GetElement("is_historical"));
        }

        [Test]
        public void CanMapHistoricalVisitToBsonDocument()
        {
            string visitId = "1";
            string mosaicId = "1";
            string title = "Title";
            string content = "Content";
            string email = "first.last@domain.com";
            DateTime createdOn = DateTime.Now;

            Visit visit = new Visit()
            {
                MosaicId = mosaicId,
                Title = title,
                Content = content,
                CreatedByEmail = email,
                CreatedOn = createdOn,
                Id = visitId
            };

            var expectedDocument = new BsonDocument(
                        new List<BsonElement>
                        {
                                new BsonElement("_id", visitId),
                                new BsonElement("worker_email", email),
                                new BsonElement("form_name_overall", "Historical_Visit"),
                                new BsonElement("form_name", title),
                                new BsonElement("timestamp", createdOn.ToString("dd/MM/yyyy H:mm:ss")),
                                new BsonElement("is_historical", true)
                        });

            List<Visit> visits = new List<Visit>() { visit };

            var result = ResponseFactory.HistoricalVisitsToDomain(visits);

            Assert.AreEqual(expectedDocument.GetElement("_id"), result.First().GetElement("_id"));
            Assert.AreEqual(expectedDocument.GetElement("worker_email"), result.First().GetElement("worker_email"));
            Assert.AreEqual(expectedDocument.GetElement("form_name_overall"), result.First().GetElement("form_name_overall"));
            Assert.AreEqual(expectedDocument.GetElement("form_name"), result.First().GetElement("form_name"));
            Assert.AreEqual(expectedDocument.GetElement("timestamp"), result.First().GetElement("timestamp"));
            Assert.AreEqual(expectedDocument.GetElement("is_historical"), result.First().GetElement("is_historical"));
        }
    }
}
