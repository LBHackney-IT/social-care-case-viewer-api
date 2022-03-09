using Bogus;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using SocialCareCaseViewerApi.Tests.V1.Helpers;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Factories;
using SocialCareCaseViewerApi.V1.Gateways;
using System;
using System.Collections.Generic;
using System.Linq;
using Person = SocialCareCaseViewerApi.V1.Infrastructure.Person;

//#nullable enable
namespace SocialCareCaseViewerApi.Tests.V1.Gateways.Search
{
    [TestFixture]
    public class GetPersonRecordsBySearchQueryTests : DatabaseTests
    {
        private SearchGateway _searchGateway = null!;

        [SetUp]
        public void Setup()
        {
            _searchGateway = new SearchGateway(DatabaseContext);
            DatabaseContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        }

        [Test]
        public void GetPersonRecordsReturnsEmptyListWhenNoMatchingRecordsFound()
        {
            var query = new PersonSearchRequest();

            var (result, _, _) = _searchGateway.GetPersonRecordsBySearchQuery(query);

            result.Should().BeEmpty();
        }

        [Test]
        public void GetPersonRecordsReturnsMatchingRecords()
        {
            var person1 = DatabaseGatewayHelper.CreatePersonDatabaseEntity(firstName: "ciasom", lastName: "tessellate");
            var person2 = DatabaseGatewayHelper.CreatePersonDatabaseEntity(firstName: "firsttwo", lastName: "lasttwo");
            var person3 = DatabaseGatewayHelper.CreatePersonDatabaseEntity(firstName: "firstthree", lastName: "lastthree");

            DatabaseContext.Persons.Add(person1);
            DatabaseContext.Persons.Add(person2);
            DatabaseContext.Persons.Add(person3);
            DatabaseContext.SaveChanges();

            var query = new PersonSearchRequest() { Name = person1.FirstName };

            var (result, _, _) = _searchGateway.GetPersonRecordsBySearchQuery(query);

            result.Count.Should().Be(1);
        }

        [Test]
        public void IfThereAreNoResidentsReturnsAnEmptyList()
        {
            var query = new PersonSearchRequest();

            var (listOfPersons, _, _) = _searchGateway.GetPersonRecordsBySearchQuery(query);

            listOfPersons.Should().BeEmpty();
        }

        [Test]
        public void IfThereAreResidentsWillReturnThem()
        {
            var person1 = DatabaseGatewayHelper.CreatePersonDatabaseEntity(firstName: "ciasom");
            var person2 = DatabaseGatewayHelper.CreatePersonDatabaseEntity(firstName: "ciasom");
            var person3 = DatabaseGatewayHelper.CreatePersonDatabaseEntity(firstName: "ciasom");

            DatabaseContext.Persons.AddRange(new List<Person>() { person1, person2, person3 });
            DatabaseContext.SaveChanges();

            var query = new PersonSearchRequest() { Name = "ciasom" };

            var (listOfPersons, _, _) = _searchGateway.GetPersonRecordsBySearchQuery(query);

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

            var query = new PersonSearchRequest() { Name = person.FirstName };

            var (listOfPersons, _, _) = _searchGateway.GetPersonRecordsBySearchQuery(query);

            listOfPersons.First().AddressList.Should().BeNull();
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

            var query = new PersonSearchRequest() { Name = person.FirstName };

            var (listOfPersons, _, _) = _searchGateway.GetPersonRecordsBySearchQuery(query);

            listOfPersons
                .Where(p => p.MosaicId.Equals(person.Id.ToString()))
                .First()
                .AddressList
                .Should().ContainEquivalentOf(address.ToResponse());
        }

        [Test]
        public void WithFirstnameInNameQueryParameterReturnsMatchingResident()
        {
            var person1 = DatabaseGatewayHelper.CreatePersonDatabaseEntity(firstName: "ciasom");
            var person2 = DatabaseGatewayHelper.CreatePersonDatabaseEntity(firstName: "Ciasom");
            var person3 = DatabaseGatewayHelper.CreatePersonDatabaseEntity(firstName: "Other");

            DatabaseContext.Persons.Add(person1);
            DatabaseContext.Persons.Add(person2);
            DatabaseContext.Persons.Add(person3);
            DatabaseContext.SaveChanges();

            var query = new PersonSearchRequest() { Name = "ciasom" };

            var (listOfPersons, _, _) = _searchGateway.GetPersonRecordsBySearchQuery(query);

            listOfPersons.Count.Should().Be(2);
            listOfPersons.Should().ContainEquivalentOf(DatabaseContext.Persons.First(x => x.Id == person1.Id).ToResidentInformationResponse());
            listOfPersons.Should().ContainEquivalentOf(DatabaseContext.Persons.First(x => x.Id == person2.Id).ToResidentInformationResponse());
        }

        [Test]
        public void WildcardSearchWithFirstNameInNameQueryParameterReturnsMatchingResident()
        {
            var person1 = DatabaseGatewayHelper.CreatePersonDatabaseEntity(firstName: "ciasom");
            var person2 = DatabaseGatewayHelper.CreatePersonDatabaseEntity(firstName: "Ciasom");

            DatabaseContext.Persons.Add(person1);
            DatabaseContext.Persons.Add(person2);
            DatabaseContext.SaveChanges();

            var query = new PersonSearchRequest() { Name = "iaso" };

            var (listOfPersons, _, _) = _searchGateway.GetPersonRecordsBySearchQuery(query);

            listOfPersons.Count.Should().Be(2);
            listOfPersons.Should().ContainEquivalentOf(DatabaseContext.Persons.First(x => x.Id == person1.Id).ToResidentInformationResponse());
            listOfPersons.Should().ContainEquivalentOf(DatabaseContext.Persons.First(x => x.Id == person2.Id).ToResidentInformationResponse());
        }

        [Test]
        public void WithLastNameInNameQueryParameterReturnsMatchingResident()
        {
            var person1 = DatabaseGatewayHelper.CreatePersonDatabaseEntity(lastName: "tessellate");
            var person2 = DatabaseGatewayHelper.CreatePersonDatabaseEntity(lastName: "Tessellate");

            DatabaseContext.Persons.Add(person1);
            DatabaseContext.Persons.Add(person2);
            DatabaseContext.SaveChanges();

            var query = new PersonSearchRequest() { Name = "tessellate" };

            var (listOfPersons, _, _) = _searchGateway.GetPersonRecordsBySearchQuery(query);

            listOfPersons.Count.Should().Be(2);
            listOfPersons.Should().ContainEquivalentOf(DatabaseContext.Persons.First(x => x.Id == person1.Id).ToResidentInformationResponse());
            listOfPersons.Should().ContainEquivalentOf(DatabaseContext.Persons.First(x => x.Id == person2.Id).ToResidentInformationResponse());
        }

        [Test]
        public void WildcardSearchWithLastNameInNameQueryParameterReturnsMatchingResident()
        {
            var person1 = DatabaseGatewayHelper.CreatePersonDatabaseEntity(lastName: "tessellate");
            var person2 = DatabaseGatewayHelper.CreatePersonDatabaseEntity(lastName: "Tessellate");

            DatabaseContext.Persons.Add(person1);
            DatabaseContext.Persons.Add(person2);
            DatabaseContext.SaveChanges();

            var query = new PersonSearchRequest() { Name = "sell" };

            var (listOfPersons, _, _) = _searchGateway.GetPersonRecordsBySearchQuery(query);
            listOfPersons.Count.Should().Be(2);
            listOfPersons.Should().ContainEquivalentOf(DatabaseContext.Persons.First(x => x.Id == person1.Id).ToResidentInformationResponse());
            listOfPersons.Should().ContainEquivalentOf(DatabaseContext.Persons.First(x => x.Id == person2.Id).ToResidentInformationResponse());
        }

        [Test]
        public void WithFullnameInNameQueryParametersReturnsMatchingResident()
        {
            var person = DatabaseGatewayHelper.CreatePersonDatabaseEntity(firstName: "ciasom", lastName: "Tessellate");

            DatabaseContext.Persons.Add(person);
            DatabaseContext.SaveChanges();

            var query = new PersonSearchRequest() { Name = "ciasom Tessellate" };

            var (listOfPersons, _, _) = _searchGateway.GetPersonRecordsBySearchQuery(query);
            listOfPersons.Count.Should().Be(1);
            listOfPersons.First().MosaicId.Should().Be(person.Id.ToString());
        }

        [Test]
        public void WildcardFullNameSearchReturnsMatchingResident()
        {
            var person = DatabaseGatewayHelper.CreatePersonDatabaseEntity(firstName: "ciasom", lastName: "Tessellate");

            DatabaseContext.Persons.Add(person);
            DatabaseContext.SaveChanges();

            var query = new PersonSearchRequest() { Name = "ciasom ssellat" };

            var (listOfPersons, _, _) = _searchGateway.GetPersonRecordsBySearchQuery(query);
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

            var query = new PersonSearchRequest() { Postcode = "E83 AS" };

            var (listOfPersons, _, _) = _searchGateway.GetPersonRecordsBySearchQuery(query);

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

            var query = new PersonSearchRequest() { Postcode = "E83 AS" };

            var (listOfPersons, _, _) = _searchGateway.GetPersonRecordsBySearchQuery(query);

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

            var query = new PersonSearchRequest() { Postcode = "E8 1DY", Name = person.FirstName };

            var (listOfPersons, _, _) = _searchGateway.GetPersonRecordsBySearchQuery(query);

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

            var query = new PersonSearchRequest() { Postcode = "E8 1DY" };

            var (listOfPersons, _, _) = _searchGateway.GetPersonRecordsBySearchQuery(query);
            listOfPersons.Count.Should().Be(1);
            listOfPersons
                .First(p => p.MosaicId.Equals(person1.Id.ToString()))
                .AddressList.Count
                .Should().Be(2);
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

            var query = new PersonSearchRequest() { DateOfBirth = person1.DateOfBirth.Value.ToString("yyyy-MM-dd") };

            var (listOfPersons, _, _) = _searchGateway.GetPersonRecordsBySearchQuery(query); //Format passed by FE

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

            var query = new PersonSearchRequest() { Name = "ciasom", Postcode = "E8 1DY" };

            var (listOfPersons, _, _) = _searchGateway.GetPersonRecordsBySearchQuery(query);

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

            var query = new PersonSearchRequest() { Postcode = "E8 1DY" };

            var (listOfPersons, _, _) = _searchGateway.GetPersonRecordsBySearchQuery(query);

            listOfPersons.Count.Should().Be(1);
            listOfPersons.First().MosaicId.Should().Be(person.Id.ToString());
            listOfPersons.First().AddressList.Should().ContainEquivalentOf(address.ToResponse());
        }

        [Test]
        public void OnlyReturnsTheMatchingResidentsForGivenCursor()
        {
            var person1 = DatabaseGatewayHelper.CreatePersonDatabaseEntity(firstName: "ciasom", personId: 1);
            var person2 = DatabaseGatewayHelper.CreatePersonDatabaseEntity(firstName: "ciasom", personId: 2);
            var person3 = DatabaseGatewayHelper.CreatePersonDatabaseEntity(firstName: "ciasom", personId: 3);
            var person4 = DatabaseGatewayHelper.CreatePersonDatabaseEntity(firstName: "ciasom", personId: 4);
            var person5 = DatabaseGatewayHelper.CreatePersonDatabaseEntity(firstName: "ciasom", personId: 5);

            DatabaseContext.Persons.AddRange(new List<Person>() { person1, person2, person3, person4, person5 });
            DatabaseContext.SaveChanges();

            var query = new PersonSearchRequest() { Name = "ciasom", Cursor = 2 };

            var (listOfPersons, _, _) = _searchGateway.GetPersonRecordsBySearchQuery(query);

            listOfPersons.Count.Should().Be(3);
            listOfPersons
                .Select(p => p.MosaicId)
                .Should().BeEquivalentTo(new List<string> { "3", "4", "5" });
        }

        [Test]
        public void ReturnsResidentsUprn()
        {
            var person = DatabaseGatewayHelper.CreatePersonDatabaseEntity(firstName: "ciasom");

            DatabaseContext.Persons.Add(person);
            DatabaseContext.SaveChanges();

            var address = DatabaseGatewayHelper.CreateAddressDatabaseEntity(personId: person.Id);

            DatabaseContext.Addresses.Add(address);
            DatabaseContext.SaveChanges();

            var query = new PersonSearchRequest() { Name = "ciasom" };

            var (listOfPersons, _, _) = _searchGateway.GetPersonRecordsBySearchQuery(query);

            listOfPersons
                .First(p => p.MosaicId.Equals(person.Id.ToString()))
                .Uprn
                .Should().Be(address.Uprn.ToString());
        }

        [Test]
        public void IfThereAreMultipleAddressesReturnsTheUprnForTheCurrentAddress()
        {
            var person = DatabaseGatewayHelper.CreatePersonDatabaseEntity(firstName: "foo");

            DatabaseContext.Persons.Add(person);
            DatabaseContext.SaveChanges();

            var oldAddress = DatabaseGatewayHelper.CreateAddressDatabaseEntity(personId: person.Id, endDate: DateTime.Now);
            var currentAddress = DatabaseGatewayHelper.CreateAddressDatabaseEntity(personId: person.Id);

            DatabaseContext.Addresses.Add(oldAddress);
            DatabaseContext.Addresses.Add(currentAddress);
            DatabaseContext.SaveChanges();

            var query = new PersonSearchRequest() { Name = "foo" };

            var (response, _, _) = _searchGateway.GetPersonRecordsBySearchQuery(query);
            response.First().Uprn.Should().Be(currentAddress.Uprn.ToString());
        }

        [Test]
        public void DoesNotReturnPersonRecordsMarkedForDeletion()
        {
            var person1 = DatabaseGatewayHelper.CreatePersonDatabaseEntity(firstName: "foo");
            var person2 = DatabaseGatewayHelper.CreatePersonDatabaseEntity(firstName: "foo", markedForDeletion: true);
            var person3 = DatabaseGatewayHelper.CreatePersonDatabaseEntity(firstName: "foo");

            DatabaseContext.Persons.AddRange(new List<Person> { person1, person2, person3 });
            DatabaseContext.SaveChanges();

            var query = new PersonSearchRequest() { Name = "foo" };

            var (response, _, _) = _searchGateway.GetPersonRecordsBySearchQuery(query);

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

            var query = new PersonSearchRequest() { Name = "foo" };

            var (_, totalCount, _) = _searchGateway.GetPersonRecordsBySearchQuery(query);

            totalCount.Should().Be(3);
        }

        [Test]
        public void GivenMoreThanTwentyResultsWhenNextCursorUnspecifiedNextCursorIsTwenty()
        {
            var personRecords = new List<Person>();

            for (int i = 0; i < 25; i++)
            {
                personRecords.Add(DatabaseGatewayHelper.CreatePersonDatabaseEntity(firstName: "foo"));
            }

            DatabaseContext.Persons.AddRange(personRecords);
            DatabaseContext.SaveChanges();

            var request = new PersonSearchRequest() { Name = "foo" };

            var (_, _, cursor) = _searchGateway.GetPersonRecordsBySearchQuery(request);

            cursor.Should().Be(20);
        }

        [Test]
        public void GivenLessThanTwentyResultsWhenNextCursorIsUnspecifiedNextCursorIsNull()
        {
            var personRecords = new List<Person>();

            for (int i = 0; i < 19; i++)
            {
                personRecords.Add(DatabaseGatewayHelper.CreatePersonDatabaseEntity(firstName: "foo"));
            }

            DatabaseContext.Persons.AddRange(personRecords);
            DatabaseContext.SaveChanges();

            var request = new PersonSearchRequest() { Name = "foo" };

            var (_, _, cursor) = _searchGateway.GetPersonRecordsBySearchQuery(request);

            cursor.Should().Be(null);
        }

        [Test]
        public void GivenTwentyResultsWhenNextCursorIsUnspecifiedNextCursorIsNull()
        {
            var personRecords = new List<Person>();

            for (int i = 0; i < 20; i++)
            {
                personRecords.Add(DatabaseGatewayHelper.CreatePersonDatabaseEntity(firstName: "foo"));
            }

            DatabaseContext.Persons.AddRange(personRecords);
            DatabaseContext.SaveChanges();

            var request = new PersonSearchRequest() { Name = "foo" };

            var (_, _, cursor) = _searchGateway.GetPersonRecordsBySearchQuery(request);

            cursor.Should().Be(null);
        }

        [Test]
        public void GivenLessThanTwentyResultsWhenNextCursorIsTwentyNextCursorIsNull()
        {
            var personRecords = new List<Person>();

            for (int i = 0; i < 10; i++)
            {
                personRecords.Add(DatabaseGatewayHelper.CreatePersonDatabaseEntity(firstName: "foo"));
            }

            DatabaseContext.Persons.AddRange(personRecords);
            DatabaseContext.SaveChanges();

            var request = new PersonSearchRequest() { Name = "foo", Cursor = 20 };

            var (_, _, cursor) = _searchGateway.GetPersonRecordsBySearchQuery(request);

            cursor.Should().Be(null);
        }

        [Test]
        public void GivenMoreThanFortyResultsWhenNextCursorIsTwentyNextCursorIsForty()
        {
            var personRecords = new List<Person>();

            for (int i = 0; i < 50; i++)
            {
                personRecords.Add(DatabaseGatewayHelper.CreatePersonDatabaseEntity(firstName: "foo"));
            }

            DatabaseContext.Persons.AddRange(personRecords);
            DatabaseContext.SaveChanges();

            var request = new PersonSearchRequest() { Name = "foo", Cursor = 20 };

            var (_, _, cursor) = _searchGateway.GetPersonRecordsBySearchQuery(request);

            cursor.Should().Be(40);
        }

        [Test]
        public void GivenFiftyResultsWhenNextCursorIsFortyThenTenResultsAreReturnedAndTheCursorIsNull()
        {
            var personRecords = new List<Person>();

            for (int i = 0; i < 50; i++)
            {
                personRecords.Add(DatabaseGatewayHelper.CreatePersonDatabaseEntity(firstName: "foo"));
            }

            DatabaseContext.Persons.AddRange(personRecords);
            DatabaseContext.SaveChanges();

            var request = new PersonSearchRequest() { Name = "foo", Cursor = 40 };

            var (results, _, cursor) = _searchGateway.GetPersonRecordsBySearchQuery(request);

            cursor.Should().Be(null);
            results.Count.Should().Be(10);
        }

        //these names are designed to provide consistent scores and sorting
        [Test]
        public void GivenSeveralMatchingResultsTheyAreReturnedInAlphabeticalOrder()
        {
            var person1 = DatabaseGatewayHelper.CreatePersonDatabaseEntity(firstName: "Alice", lastName: "zzzzz");
            var person2 = DatabaseGatewayHelper.CreatePersonDatabaseEntity(firstName: "Danny", lastName: "zzzzz");
            var person3 = DatabaseGatewayHelper.CreatePersonDatabaseEntity(firstName: "Carly", lastName: "zzzzz");
            var person4 = DatabaseGatewayHelper.CreatePersonDatabaseEntity(firstName: "Bobby", lastName: "zzzzz");

            DatabaseContext.Persons.AddRange(person1, person2, person3, person4);
            DatabaseContext.SaveChanges();

            var request = new PersonSearchRequest() { Name = "zzzzz" };

            var (results, _, _) = _searchGateway.GetPersonRecordsBySearchQuery(request);

            var firstNames = results.Select(x => x.FirstName).ToList();

            var expextedFirstNames = new List<string>() { "Alice", "Bobby", "Carly", "Danny" };

            firstNames.Should().Equal(expextedFirstNames);
        }
    }
}
