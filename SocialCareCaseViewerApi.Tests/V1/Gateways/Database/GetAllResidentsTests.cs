using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using SocialCareCaseViewerApi.Tests.V1.Helpers;
using SocialCareCaseViewerApi.V1.Factories;
using SocialCareCaseViewerApi.V1.Gateways;
using SocialCareCaseViewerApi.V1.Helpers;
using SocialCareCaseViewerApi.V1.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SocialCareCaseViewerApi.Tests.V1.Gateways.Database
{
    [TestFixture]
    public class GetAllResidentsTests : DatabaseTests
    {
        private DatabaseGateway _classUnderTest;
        private Mock<IProcessDataGateway> _mockProcessDataGateway;
        private readonly Mock<ISystemTime> _mockSystemTime = new Mock<ISystemTime>();

        [SetUp]
        public void Setup()
        {
            _mockProcessDataGateway = new Mock<IProcessDataGateway>();

            _classUnderTest = new DatabaseGateway(DatabaseContext, _mockProcessDataGateway.Object,
                _mockSystemTime.Object);

            DatabaseContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        }

        [Test]
        public void IfThereAreNoResidentsReturnsAnEmptyList()
        {
            var (listOfPersons, _) = _classUnderTest.GetResidentsBySearchCriteria(cursor: 0, limit: 20);

            listOfPersons.Should().BeEmpty();
        }

        [Test]
        public void IfThereAreResidentsWillReturnThem()
        {
            var person1 = DatabaseGatewayHelper.CreatePersonDatabaseEntity();
            var person2 = DatabaseGatewayHelper.CreatePersonDatabaseEntity();
            var person3 = DatabaseGatewayHelper.CreatePersonDatabaseEntity();

            DatabaseContext.Persons.AddRange(new List<Person>() { person1, person2, person3 });
            DatabaseContext.SaveChanges();

            var (listOfPersons, _) = _classUnderTest.GetResidentsBySearchCriteria(cursor: 0, limit: 20);

            listOfPersons.Should().ContainEquivalentOf(DatabaseContext.Persons.First(x => x.Id == person1.Id).ToResidentInformationResponse());
            listOfPersons.Should().ContainEquivalentOf(DatabaseContext.Persons.First(x => x.Id == person2.Id).ToResidentInformationResponse());
            listOfPersons.Should().ContainEquivalentOf(DatabaseContext.Persons.First(x => x.Id == person3.Id).ToResidentInformationResponse());
        }

        [Test]
        public void IfThereAreNoResidentAddressesWillReturnNullForAddresses()
        {
            var person = DatabaseGatewayHelper.CreatePersonDatabaseEntity();
            DatabaseContext.Persons.Add(person);
            DatabaseContext.SaveChanges();

            var (listOfPersons, _) = _classUnderTest.GetResidentsBySearchCriteria(cursor: 0, limit: 20, id: person.Id);

            listOfPersons.First().AddressList.Should().BeNull();
        }

        [Test]
        public void IfThereAreNoResidentPhoneNumbersWillReturnNullForPhoneNumbers()
        {
            var person = DatabaseGatewayHelper.CreatePersonDatabaseEntity();
            DatabaseContext.Persons.Add(person);
            DatabaseContext.SaveChanges();

            var (listOfPersons, _) = _classUnderTest.GetResidentsBySearchCriteria(cursor: 0, limit: 20, id: person.Id);

            listOfPersons.First().PhoneNumber.Should().BeNull();
        }

        [Test]
        public void IfThereAreResidentsWithGivenMosaicIdThenReturnsThem()
        {
            var person1 = DatabaseGatewayHelper.CreatePersonDatabaseEntity();
            var person2 = DatabaseGatewayHelper.CreatePersonDatabaseEntity();
            var person3 = DatabaseGatewayHelper.CreatePersonDatabaseEntity();

            DatabaseContext.Persons.AddRange(new List<Person> { person1, person2, person3 });
            DatabaseContext.SaveChanges();

            var (listOfPersons, _) = _classUnderTest.GetResidentsBySearchCriteria(cursor: 0, limit: 20, id: person2.Id);

            listOfPersons.Count.Should().Be(1);
            listOfPersons.First().MosaicId.Should().Be(person2.Id.ToString());
        }

        [Test]
        public void IfThereAreResidentAddressesWillReturnThem()
        {
            var person = DatabaseGatewayHelper.CreatePersonDatabaseEntity();
            DatabaseContext.Persons.Add(person);
            DatabaseContext.SaveChanges();

            var address = DatabaseGatewayHelper.CreateAddressDatabaseEntity(personId: person.Id);
            DatabaseContext.Addresses.Add(address);
            DatabaseContext.SaveChanges();

            var (listOfPersons, _) = _classUnderTest.GetResidentsBySearchCriteria(cursor: 0, limit: 20);

            listOfPersons
                .Where(p => p.MosaicId.Equals(person.Id.ToString()))
                .First()
                .AddressList
                .Should().ContainEquivalentOf(address.ToResponse());
        }

        [Test]
        public void IfThereAreResidentPhoneNumbersWillReturnThem()
        {
            var person = DatabaseGatewayHelper.CreatePersonDatabaseEntity();
            DatabaseContext.Persons.Add(person);
            DatabaseContext.SaveChanges();

            var phoneNumber = DatabaseGatewayHelper.CreatePhoneNumberEntity(personId: person.Id);

            DatabaseContext.PhoneNumbers.Add(phoneNumber);
            DatabaseContext.SaveChanges();

            var (listOfPersons, _) = _classUnderTest.GetResidentsBySearchCriteria(0, 20);

            listOfPersons
                .Where(p => p.MosaicId.Equals(phoneNumber.Person.Id.ToString()))
                .First()
                .PhoneNumber
                .Should().ContainEquivalentOf(phoneNumber.ToResponse());
        }

        [Test]
        public void WithFirstNameQueryParameterReturnsMatchingResident()
        {
            var person1 = DatabaseGatewayHelper.CreatePersonDatabaseEntity(firstName: "ciasom");
            var person2 = DatabaseGatewayHelper.CreatePersonDatabaseEntity(firstName: "Ciasom");

            DatabaseContext.Persons.Add(person1);
            DatabaseContext.Persons.Add(person2);
            DatabaseContext.SaveChanges();

            var (listOfPersons, _) = _classUnderTest.GetResidentsBySearchCriteria(cursor: 0, limit: 20, firstName: "ciasom");

            listOfPersons.Count.Should().Be(2);
            listOfPersons.Should().ContainEquivalentOf(DatabaseContext.Persons.First(x => x.Id == person1.Id).ToResidentInformationResponse());
            listOfPersons.Should().ContainEquivalentOf(DatabaseContext.Persons.First(x => x.Id == person2.Id).ToResidentInformationResponse());
        }

        [Test]
        public void WildcardSearchWithFirstNameQueryParameterReturnsMatchingResident()
        {
            var person1 = DatabaseGatewayHelper.CreatePersonDatabaseEntity(firstName: "ciasom");
            var person2 = DatabaseGatewayHelper.CreatePersonDatabaseEntity(firstName: "Ciasom");

            DatabaseContext.Persons.Add(person1);
            DatabaseContext.Persons.Add(person2);
            DatabaseContext.SaveChanges();

            var (listOfPersons, _) = _classUnderTest.GetResidentsBySearchCriteria(cursor: 0, limit: 20, firstName: "iaso");

            listOfPersons.Count.Should().Be(2);
            listOfPersons.Should().ContainEquivalentOf(DatabaseContext.Persons.First(x => x.Id == person1.Id).ToResidentInformationResponse());
            listOfPersons.Should().ContainEquivalentOf(DatabaseContext.Persons.First(x => x.Id == person2.Id).ToResidentInformationResponse());
        }

        [Test]
        public void WithLastNameQueryParameterReturnsMatchingResident()
        {
            var person1 = DatabaseGatewayHelper.CreatePersonDatabaseEntity(lastName: "tessellate");
            var person2 = DatabaseGatewayHelper.CreatePersonDatabaseEntity(lastName: "Tessellate");

            DatabaseContext.Persons.Add(person1);
            DatabaseContext.Persons.Add(person2);
            DatabaseContext.SaveChanges();

            var (listOfPersons, _) = _classUnderTest.GetResidentsBySearchCriteria(cursor: 0, limit: 20, lastName: "tessellate");

            listOfPersons.Count.Should().Be(2);
            listOfPersons.Should().ContainEquivalentOf(DatabaseContext.Persons.First(x => x.Id == person1.Id).ToResidentInformationResponse());
            listOfPersons.Should().ContainEquivalentOf(DatabaseContext.Persons.First(x => x.Id == person2.Id).ToResidentInformationResponse());
        }

        [Test]
        public void WildcardSearchWithLastNameQueryParameterReturnsMatchingResident()
        {
            var person1 = DatabaseGatewayHelper.CreatePersonDatabaseEntity(lastName: "tessellate");
            var person2 = DatabaseGatewayHelper.CreatePersonDatabaseEntity(lastName: "Tessellate");

            DatabaseContext.Persons.Add(person1);
            DatabaseContext.Persons.Add(person2);
            DatabaseContext.SaveChanges();

            var (listOfPersons, _) = _classUnderTest.GetResidentsBySearchCriteria(cursor: 0, limit: 20, lastName: "sell");
            listOfPersons.Count.Should().Be(2);
            listOfPersons.Should().ContainEquivalentOf(DatabaseContext.Persons.First(x => x.Id == person1.Id).ToResidentInformationResponse());
            listOfPersons.Should().ContainEquivalentOf(DatabaseContext.Persons.First(x => x.Id == person2.Id).ToResidentInformationResponse());
        }

        [Test]
        public void WithNameQueryParametersReturnsMatchingResident()
        {
            var person = DatabaseGatewayHelper.CreatePersonDatabaseEntity(firstName: "ciasom", lastName: "Tessellate");

            DatabaseContext.Persons.Add(person);
            DatabaseContext.SaveChanges();

            var (listOfPersons, _) = _classUnderTest.GetResidentsBySearchCriteria(cursor: 0, limit: 20, firstName: "ciasom", lastName: "Tessellate");
            listOfPersons.Count.Should().Be(1);
            listOfPersons.First().MosaicId.Should().Be(person.Id.ToString());
        }

        [Test]
        public void WildcardSearchWithNameQueryParametersReturnsMatchingResident()
        {
            var person = DatabaseGatewayHelper.CreatePersonDatabaseEntity(firstName: "ciasom", lastName: "Tessellate");

            DatabaseContext.Persons.Add(person);
            DatabaseContext.SaveChanges();

            var (listOfPersons, _) = _classUnderTest.GetResidentsBySearchCriteria(cursor: 0, limit: 20, firstName: "ciasom", lastName: "ssellat");
            listOfPersons.Count.Should().Be(1);
            listOfPersons.First().MosaicId.Should().Be(person.Id.ToString());
        }

        [Test]
        public void GetAllResidentsWithPostCodeQueryParameterReturnsOnlyResidentsWithPostcodeInDisplayAddress()
        {
            var person1 = DatabaseGatewayHelper.CreatePersonDatabaseEntity();
            var person2 = DatabaseGatewayHelper.CreatePersonDatabaseEntity();
            DatabaseContext.Persons.Add(person1);
            DatabaseContext.Persons.Add(person2);
            DatabaseContext.SaveChanges();

            var address1 = DatabaseGatewayHelper.CreateAddressDatabaseEntity(personId: person1.Id, postCode: "E83 AS");
            var address2 = DatabaseGatewayHelper.CreateAddressDatabaseEntity(personId: person2.Id, postCode: "E83 AS", isDisplayAddress: "N");

            DatabaseContext.Addresses.Add(address1);
            DatabaseContext.Addresses.Add(address2);
            DatabaseContext.SaveChanges();

            var (listOfPersons, _) = _classUnderTest.GetResidentsBySearchCriteria(cursor: 0, limit: 20, postcode: "E83 AS");

            listOfPersons.Count.Should().Be(1);
            listOfPersons.First().MosaicId.Should().Be(person1.Id.ToString());
        }

        [Test]
        public void GetAllResidentsWithPostCodeQueryParameterReturnsEmptyListWhenNoMatchingResidentsWithPostcodeInDisplayAddressAreFound()
        {
            var person = DatabaseGatewayHelper.CreatePersonDatabaseEntity();
            DatabaseContext.Persons.Add(person);
            DatabaseContext.SaveChanges();

            var address = DatabaseGatewayHelper.CreateAddressDatabaseEntity(personId: person.Id, postCode: "E83 AS", isDisplayAddress: "N");

            DatabaseContext.Addresses.Add(address);
            DatabaseContext.SaveChanges();

            var (listOfPersons, _) = _classUnderTest.GetResidentsBySearchCriteria(cursor: 0, limit: 20, postcode: "E83 AS");

            listOfPersons.Should().BeEmpty();
        }

        [Test]
        public void GetAllResidentsWithPostCodeQueryParameterReturnsMatchingResident()
        {
            var person = DatabaseGatewayHelper.CreatePersonDatabaseEntity();

            DatabaseContext.Persons.Add(person);
            DatabaseContext.SaveChanges();

            var address1 = DatabaseGatewayHelper.CreateAddressDatabaseEntity(personId: person.Id, postCode: "E8 1DY");
            var address2 = DatabaseGatewayHelper.CreateAddressDatabaseEntity(personId: person.Id, postCode: "E8 5TG");

            DatabaseContext.Addresses.Add(address1);
            DatabaseContext.Addresses.Add(address2);
            DatabaseContext.SaveChanges();

            var (listOfPersons, _) = _classUnderTest.GetResidentsBySearchCriteria(cursor: 0, limit: 20, postcode: "E8 1DY");

            listOfPersons.Count.Should().Be(1);

            listOfPersons
                .First(p => p.MosaicId.Equals(address1.Person.Id.ToString()))
                .AddressList
                .Should().ContainEquivalentOf(address1.ToDomain());
        }

        [Test]
        public void WithPostCodeQueryParameterWhenAPersonHasTwoAddressesReturnsOneRecordWithAListOfAddresses()
        {
            var person1 = DatabaseGatewayHelper.CreatePersonDatabaseEntity();
            var person2 = DatabaseGatewayHelper.CreatePersonDatabaseEntity();

            DatabaseContext.Persons.Add(person1);
            DatabaseContext.Persons.Add(person2);
            DatabaseContext.SaveChanges();

            var address = DatabaseGatewayHelper.CreateAddressDatabaseEntity(personId: person1.Id, postCode: "E8 1DY");
            var address2 = DatabaseGatewayHelper.CreateAddressDatabaseEntity(personId: person1.Id, postCode: "E8 1DY");

            DatabaseContext.Addresses.Add(address);
            DatabaseContext.Addresses.Add(address2);
            DatabaseContext.SaveChanges();

            var (listOfPersons, _) = _classUnderTest.GetResidentsBySearchCriteria(cursor: 0, limit: 20, postcode: "E8 1DY");
            listOfPersons.Count.Should().Be(1);
            listOfPersons
                .First(p => p.MosaicId.Equals(person1.Id.ToString()))
                .AddressList.Count
                .Should().Be(2);
        }

        [Test]
        public void WithPostCodeQueryParameterReturnsPhoneNumberWithResidentInformation()
        {
            var person = DatabaseGatewayHelper.CreatePersonDatabaseEntity();

            DatabaseContext.Persons.Add(person);
            DatabaseContext.SaveChanges();

            var address = DatabaseGatewayHelper.CreateAddressDatabaseEntity(personId: person.Id, postCode: "E8 1DY");
            var phoneNumber = DatabaseGatewayHelper.CreatePhoneNumberEntity(person.Id);

            DatabaseContext.Addresses.Add(address);
            DatabaseContext.PhoneNumbers.Add(phoneNumber);
            DatabaseContext.SaveChanges();

            var (listOfPersons, _) = _classUnderTest.GetResidentsBySearchCriteria(cursor: 0, limit: 20, postcode: "E8 1DY");

            var personUnderTest = listOfPersons
                .First(p => p.MosaicId.Equals(person.Id.ToString()));

            personUnderTest.PhoneNumber.Should().ContainEquivalentOf(phoneNumber.ToResponse());
        }

        [Test]
        public void WithBirthdayQueryParameterReturnsMatchingResidents()
        {
            var person1 = DatabaseGatewayHelper.CreatePersonDatabaseEntity();
            var person2 = DatabaseGatewayHelper.CreatePersonDatabaseEntity();
            var person3 = DatabaseGatewayHelper.CreatePersonDatabaseEntity();

            person3.DateOfBirth = person1.DateOfBirth;

            DatabaseContext.Persons.AddRange(new List<Person> { person1, person2, person3 });
            DatabaseContext.SaveChanges();

            var (listOfPersons, _) = _classUnderTest.GetResidentsBySearchCriteria(cursor: 0, limit: 20, dateOfBirth: person1.DateOfBirth.Value.ToString("yyyy-MM-dd")); //Format passed by FE

            listOfPersons.Count.Should().Be(2);
            listOfPersons.Should().ContainEquivalentOf(DatabaseContext.Persons.First(x => x.Id == person1.Id).ToResidentInformationResponse());
            listOfPersons.Should().ContainEquivalentOf(DatabaseContext.Persons.First(x => x.Id == person3.Id).ToResidentInformationResponse());
        }

        [Test]
        public void WithContextFlagQueryParameterReturnsMatchingResidents()
        {
            var person1 = DatabaseGatewayHelper.CreatePersonDatabaseEntity();
            person1.AgeContext = "A";
            var person2 = DatabaseGatewayHelper.CreatePersonDatabaseEntity();
            person2.AgeContext = "C";
            var person3 = DatabaseGatewayHelper.CreatePersonDatabaseEntity();
            person3.AgeContext = "A";

            DatabaseContext.Persons.AddRange(new List<Person> { person1, person2, person3 });
            DatabaseContext.SaveChanges();

            var (listOfPersons, _) = _classUnderTest.GetResidentsBySearchCriteria(cursor: 0, limit: 20, contextFlag: "A");

            listOfPersons.Count.Should().Be(2);
            listOfPersons.Should().ContainEquivalentOf(DatabaseContext.Persons.First(x => x.Id == person1.Id).ToResidentInformationResponse());
            listOfPersons.Should().ContainEquivalentOf(DatabaseContext.Persons.First(x => x.Id == person3.Id).ToResidentInformationResponse());
        }

        [Test]
        public void WithNameAndPostCodeQueryParameterReturnsMatchingResident()
        {
            var person1 = DatabaseGatewayHelper.CreatePersonDatabaseEntity(firstName: "ciasom");

            var person2 = DatabaseGatewayHelper.CreatePersonDatabaseEntity(firstName: "wrong name");

            var person3 = DatabaseGatewayHelper.CreatePersonDatabaseEntity(firstName: "ciasom");

            DatabaseContext.Persons.Add(person1);
            DatabaseContext.Persons.Add(person2);
            DatabaseContext.Persons.Add(person3);
            DatabaseContext.SaveChanges();

            var address = DatabaseGatewayHelper.CreateAddressDatabaseEntity(personId: person1.Id, postCode: "E8 1DY");
            var address2 = DatabaseGatewayHelper.CreateAddressDatabaseEntity(personId: person2.Id, postCode: "E8 1DY");
            var address3 = DatabaseGatewayHelper.CreateAddressDatabaseEntity(personId: person2.Id, postCode: "E8 5RT");

            DatabaseContext.Addresses.Add(address);
            DatabaseContext.Addresses.Add(address2);
            DatabaseContext.Addresses.Add(address3);
            DatabaseContext.SaveChanges();

            var (listOfPersons, _) = _classUnderTest.GetResidentsBySearchCriteria(cursor: 0, limit: 20, firstName: "ciasom", postcode: "E8 1DY");

            listOfPersons.Count.Should().Be(1);
            listOfPersons.First().MosaicId.Should().Be(person1.Id.ToString());
            listOfPersons.First().AddressList.Should().ContainEquivalentOf(address.ToResponse());
        }

        [TestCase("E81DY")]
        [TestCase("e8 1DY")]
        public void WithPostCodeQueryParameterIgnoresFormatting(string postcode)
        {
            var person = DatabaseGatewayHelper.CreatePersonDatabaseEntity();

            DatabaseContext.Persons.Add(person);
            DatabaseContext.SaveChanges();

            var address = DatabaseGatewayHelper.CreateAddressDatabaseEntity(personId: person.Id, postCode: postcode);

            DatabaseContext.Addresses.Add(address);
            DatabaseContext.SaveChanges();

            var (listOfPersons, _) = _classUnderTest.GetResidentsBySearchCriteria(cursor: 0, limit: 20, postcode: "E8 1DY");

            listOfPersons.Count.Should().Be(1);
            listOfPersons.First().MosaicId.Should().Be(person.Id.ToString());
            listOfPersons.First().AddressList.Should().ContainEquivalentOf(address.ToResponse());
        }

        [TestCase("1 My Street")]
        [TestCase("My Street")]
        [TestCase("1 My Street, Hackney, London")]
        [TestCase("Hackney")]
        public void WithAddressQueryParameterReturnsMatchingResident(string addressQuery)
        {
            var person1 = DatabaseGatewayHelper.CreatePersonDatabaseEntity();
            var person2 = DatabaseGatewayHelper.CreatePersonDatabaseEntity();

            DatabaseContext.Persons.Add(person1);
            DatabaseContext.Persons.Add(person2);
            DatabaseContext.SaveChanges();

            var address = DatabaseGatewayHelper.CreateAddressDatabaseEntity(personId: person1.Id, address: "1 My Street, Hackney, London");
            var address2 = DatabaseGatewayHelper.CreateAddressDatabaseEntity(personId: person2.Id, address: "5 Another Street, Lambeth, London");

            DatabaseContext.Addresses.Add(address);
            DatabaseContext.Addresses.Add(address2);
            DatabaseContext.SaveChanges();

            var (listOfPersons, _) = _classUnderTest.GetResidentsBySearchCriteria(cursor: 0, limit: 20, address: addressQuery);

            listOfPersons.Count.Should().Be(1);

            listOfPersons
                .First(p => p.MosaicId.Equals(person1.Id.ToString()))
                .AddressList
                .Should().ContainEquivalentOf(address.ToDomain());
        }

        [Test]
        public void OnlyReturnsTheMatchingResidentsWithinLimit()
        {
            var person1 = DatabaseGatewayHelper.CreatePersonDatabaseEntity(personId: 1);
            var person2 = DatabaseGatewayHelper.CreatePersonDatabaseEntity(personId: 2);
            var person3 = DatabaseGatewayHelper.CreatePersonDatabaseEntity(personId: 3);
            var person4 = DatabaseGatewayHelper.CreatePersonDatabaseEntity(personId: 4);
            var person5 = DatabaseGatewayHelper.CreatePersonDatabaseEntity(personId: 5);

            DatabaseContext.Persons.AddRange(new List<Person>() { person1, person2, person3, person4, person5 });
            DatabaseContext.SaveChanges();

            var (listOfPersons, _) = _classUnderTest.GetResidentsBySearchCriteria(cursor: 0, limit: 3);

            listOfPersons.Count.Should().Be(3);

            listOfPersons
                .Select(p => p.MosaicId)
                .Should().BeEquivalentTo(new List<string> { "1", "2", "3" });
        }


        [Test]
        public void OnlyReturnsTheMatchingResidentsForGivenLimitAndCursor()
        {
            var person1 = DatabaseGatewayHelper.CreatePersonDatabaseEntity(personId: 1);
            var person2 = DatabaseGatewayHelper.CreatePersonDatabaseEntity(personId: 2);
            var person3 = DatabaseGatewayHelper.CreatePersonDatabaseEntity(personId: 3);
            var person4 = DatabaseGatewayHelper.CreatePersonDatabaseEntity(personId: 4);
            var person5 = DatabaseGatewayHelper.CreatePersonDatabaseEntity(personId: 5);

            DatabaseContext.Persons.AddRange(new List<Person>() { person1, person2, person3, person4, person5 });
            DatabaseContext.SaveChanges();

            var (listOfPersons, _) = _classUnderTest.GetResidentsBySearchCriteria(cursor: 2, limit: 3);

            listOfPersons.Count.Should().Be(3);
            listOfPersons
                .Select(p => p.MosaicId)
                .Should().BeEquivalentTo(new List<string> { "3", "4", "5" });
        }

        [Test]
        public void ReturnsResidentsUprn()
        {
            var person = DatabaseGatewayHelper.CreatePersonDatabaseEntity();

            DatabaseContext.Persons.Add(person);
            DatabaseContext.SaveChanges();

            var address = DatabaseGatewayHelper.CreateAddressDatabaseEntity(personId: person.Id);

            DatabaseContext.Addresses.Add(address);
            DatabaseContext.SaveChanges();


            var (listOfPersons, _) = _classUnderTest.GetResidentsBySearchCriteria(0, 20);

            listOfPersons
                .First(p => p.MosaicId.Equals(person.Id.ToString()))
                .Uprn
                .Should().Be(address.Uprn.ToString());
        }

        [Test]
        public void IfThereAreMultipleAddressesReturnsTheUprnForTheCurrentAddress()
        {
            var person = DatabaseGatewayHelper.CreatePersonDatabaseEntity();

            DatabaseContext.Persons.Add(person);
            DatabaseContext.SaveChanges();

            var oldAddress = DatabaseGatewayHelper.CreateAddressDatabaseEntity(personId: person.Id, endDate: DateTime.Now);
            var currentAddress = DatabaseGatewayHelper.CreateAddressDatabaseEntity(personId: person.Id);

            DatabaseContext.Addresses.Add(oldAddress);
            DatabaseContext.Addresses.Add(currentAddress);
            DatabaseContext.SaveChanges();

            var (response, _) = _classUnderTest.GetResidentsBySearchCriteria(0, 20);
            response.First().Uprn.Should().Be(currentAddress.Uprn.ToString());
        }

        [Test]
        public void DoesNotReturnPersonRecordsMarkedForDeletion()
        {
            var person1 = DatabaseGatewayHelper.CreatePersonDatabaseEntity();
            var person2 = DatabaseGatewayHelper.CreatePersonDatabaseEntity(markedForDeletion: true);
            var person3 = DatabaseGatewayHelper.CreatePersonDatabaseEntity();

            DatabaseContext.Persons.AddRange(new List<Person> { person1, person2, person3 });
            DatabaseContext.SaveChanges();

            var (response, _) = _classUnderTest.GetResidentsBySearchCriteria(cursor: 0, limit: 20);

            response.Count.Should().Be(2);

            response.Any(x => x.MosaicId == person1.Id.ToString()).Should().BeTrue();
            response.Any(x => x.MosaicId == person2.Id.ToString()).Should().BeFalse();
            response.Any(x => x.MosaicId == person3.Id.ToString()).Should().BeTrue();
        }

        [Test]
        public void ReturnsTotalPersonCountForGivenFilterIgnoringPagingAndCursor()
        {
            var person1 = DatabaseGatewayHelper.CreatePersonDatabaseEntity(firstName: "foo");
            var person2 = DatabaseGatewayHelper.CreatePersonDatabaseEntity(firstName: "foobar");
            var person3 = DatabaseGatewayHelper.CreatePersonDatabaseEntity(firstName: "foobarbar");

            DatabaseContext.Persons.AddRange(new List<Person> { person1, person2, person3 });
            DatabaseContext.SaveChanges();

            var (_, totalCount) = _classUnderTest.GetResidentsBySearchCriteria(cursor: 0, limit: 1, firstName: "foo");

            totalCount.Should().Be(3);
        }
    }
}
