using System;
using System.Collections.Generic;
using System.Linq;
using Bogus;
using FluentAssertions;
using MongoDB.Bson;
using NUnit.Framework;
using SocialCareCaseViewerApi.Tests.V1.Helpers;
using SocialCareCaseViewerApi.V1.Boundary.Response;
using SocialCareCaseViewerApi.V1.Domain;
using SocialCareCaseViewerApi.V1.Factories;
using SocialCareCaseViewerApi.V1.Infrastructure;
using Address = SocialCareCaseViewerApi.V1.Infrastructure.Address;
using Person = SocialCareCaseViewerApi.V1.Infrastructure.Person;
using PhoneNumber = SocialCareCaseViewerApi.V1.Infrastructure.PhoneNumber;
using PhoneNumberDomain = SocialCareCaseViewerApi.V1.Domain.PhoneNumber;

namespace SocialCareCaseViewerApi.Tests.V1.Factories
{
    public class ResponseFactoryTests
    {
        private Faker _faker;

        public ResponseFactoryTests()
        {
            _faker = new Faker();
        }

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
            string mosaicId = "123";
            string email = "first.last@domain.com";
            DateTime createdOn = DateTime.Now;
            string noteType = "Historical note";
            string noteTitle = "My title";

            CaseNote historicalCaseNote = new CaseNote()
            {
                CaseNoteId = caseNoteId,
                MosaicId = mosaicId,
                CreatedByEmail = email,
                NoteType = noteType,
                CreatedOn = createdOn,
                CaseNoteTitle = noteTitle
            };

            var expectedDocument = new BsonDocument(
                        new List<BsonElement>
                        {
                                new BsonElement("_id", caseNoteId),
                                new BsonElement("mosaic_id", mosaicId),
                                new BsonElement("worker_email", email),
                                new BsonElement("form_name_overall", "Historical_Case_Note"),
                                new BsonElement("form_name", noteTitle),
                                new BsonElement("timestamp", createdOn.ToString("dd/MM/yyyy H:mm:ss")),
                                new BsonElement("is_historical", true)
                        });

            List<CaseNote> notes = new List<CaseNote>() { historicalCaseNote };

            var result = ResponseFactory.HistoricalCaseNotesToDomain(notes);

            Assert.AreEqual(expectedDocument.GetElement("_id"), result.First().GetElement("_id"));
            Assert.AreEqual(expectedDocument.GetElement("mosaic_id"), result.First().GetElement("mosaic_id"));
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
                EmailAddress = person.EmailAddress,
                DateOfBirth = person.DateOfBirth.Value,
                DateOfDeath = person.DateOfDeath.Value,
                Address = addressDomain,
                SexualOrientation = person.SexualOrientation,
                ContextFlag = person.AgeContext,
                CreatedBy = person.CreatedBy,
                Ethinicity = person.Ethnicity,
                FirstLanguage = person.FirstLanguage,
                FirstName = person.FirstName,
                Gender = person.Gender,
                LastName = person.LastName,
                NhsNumber = person.NhsNumber.Value,
                PersonId = person.Id,
                PreferredMethodOfContact = person.PreferredMethodOfContact,
                Religion = person.Religion,
                Restricted = person.Restricted,
                Title = person.Title,
                OtherNames = new List<OtherName>() { personOtherName1, personOtherName2 },
                PhoneNumbers = new List<PhoneNumberDomain>() {  phoneNumberDomain1, phoneNumberDomain2 }
            };

            var result = ResponseFactory.ToResponse(person);

            result.Should().BeEquivalentTo(expectedResponse);
        }
    }
}
