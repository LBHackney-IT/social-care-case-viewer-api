using System.Collections.Generic;
using Bogus;
using FluentAssertions;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using Newtonsoft.Json;
using NUnit.Framework;
using SocialCareCaseViewerApi.V1.Gateways;

#nullable enable
namespace SocialCareCaseViewerApi.Tests.V1.Gateways
{
    public class TestObjectForMongo
    {
        [JsonProperty("_id")]
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = null!;
        public string Property1 { get; set; } = null!;
        public List<TestObjectForMongo>? Property2 { get; set; }
        public TestObjectForMongo? Property3 { get; set; }
    }

    [TestFixture]
    public class MongoGatewayTests
    {
        private readonly IMongoGateway _mongoGateway = new MongoGateway();
        private readonly Faker _faker = new Faker();
        private TestObjectForMongo _testObjectForMongo = null!;

        [SetUp]
        public void Setup()
        {
            _testObjectForMongo = new TestObjectForMongo
            {
                Id = _faker.Random.String2(24, "0123456789abcdef"),
                Property1 = "test-property",
                Property2 = new List<TestObjectForMongo>
                {
                    new TestObjectForMongo
                    {
                        Id = _faker.Random.String2(24, "0123456789abcdef"),
                        Property1 = "test-property-list",
                        Property2 = null,
                        Property3 = null
                    }
                },
                Property3 = new TestObjectForMongo
                {
                    Id = _faker.Random.String2(24, "0123456789abcdef"),
                    Property1 = "test-property-embedded",
                    Property2 = null,
                    Property3 = null
                }
            };
        }

        [TearDown]
        public void ClearCollection()
        {
            _mongoGateway.DropCollection("test-collection-name");
        }

        [Test]
        public void GettingANonExistentRecordReturnsNull()
        {
            var retrievedObject =
                _mongoGateway.LoadRecordById<TestObjectForMongo>("test-collection-name", ObjectId.Parse(_testObjectForMongo.Id));

            retrievedObject.Should().BeNull();
        }

        [Test]
        public void CanInsertAndGetARecordById()
        {
            _mongoGateway.InsertRecord("test-collection-name", _testObjectForMongo);

            var retrievedObject =
                _mongoGateway.LoadRecordById<TestObjectForMongo>("test-collection-name", ObjectId.Parse(_testObjectForMongo.Id));

            retrievedObject.Should().BeEquivalentTo(_testObjectForMongo);
        }

        [Test]
        public void CanLoadAllRecordsFromACollection()
        {
            _mongoGateway.InsertRecord("test-collection-name", _testObjectForMongo);

            var retrievedObject = _mongoGateway.LoadRecords<TestObjectForMongo>("test-collection-name");

            retrievedObject?.Count.Should().Be(1);
            retrievedObject?[0].Should().BeEquivalentTo(_testObjectForMongo);

            _mongoGateway.InsertRecord("test-collection-name", new TestObjectForMongo());

            retrievedObject = _mongoGateway.LoadRecords<TestObjectForMongo>("test-collection-name");
            retrievedObject?.Count.Should().Be(2);
        }

        [Test]
        public void CanLoadARecordByACustomProperty()
        {
            _mongoGateway.InsertRecord("test-collection-name", _testObjectForMongo);

            var retrievedObject = _mongoGateway.LoadRecordByProperty<TestObjectForMongo, string>(
                        "test-collection-name", "Property1", _testObjectForMongo.Property1);

            retrievedObject.Should().BeEquivalentTo(_testObjectForMongo);
        }

        [Test]
        public void LoadRecordByCustomPropertyReturnsNullWhenNoMatchFound()
        {
            var retrievedObject = _mongoGateway.LoadRecordByProperty<TestObjectForMongo, string>(
                        "test-collection-name", "Property1", _testObjectForMongo.Property1);

            retrievedObject.Should().BeNull();
        }

        [Test]
        public void CanLoadMultipleRecordsByACustomProperty()
        {
            _mongoGateway.InsertRecord("test-collection-name", _testObjectForMongo);
            _mongoGateway.InsertRecord("test-collection-name", new TestObjectForMongo { Property1 = _testObjectForMongo.Property1 });

             var retrievedObject =
                    _mongoGateway.LoadMultipleRecordsByProperty<TestObjectForMongo, string>(
                        "test-collection-name", "Property1", _testObjectForMongo.Property1);

             retrievedObject?.Count.Should().Be(2);
        }

        [Test]
        public void LoadMultipleRecordsByCustomerPropertyWithNoMatchReturnsEmptyList()
        {
            var retrievedObject =
                    _mongoGateway.LoadMultipleRecordsByProperty<TestObjectForMongo, string>(
                            "test-collection-name", "Property1", _testObjectForMongo.Property1);

            retrievedObject?.Should().BeEmpty();
        }

        [Test]
        public void CanDeleteARecord()
        {
            _mongoGateway.InsertRecord("test-collection-name", _testObjectForMongo);

            var retrievedObject = _mongoGateway.LoadRecordById<TestObjectForMongo>("test-collection-name", ObjectId.Parse(_testObjectForMongo.Id));
            retrievedObject.Should().BeEquivalentTo(_testObjectForMongo);

            _mongoGateway.DeleteRecordById<TestObjectForMongo>("test-collection-name", ObjectId.Parse(_testObjectForMongo.Id));
            retrievedObject = _mongoGateway.LoadRecordById<TestObjectForMongo>("test-collection-name", ObjectId.Parse(_testObjectForMongo.Id));
            retrievedObject.Should().BeNull();
        }

        [Test]
        public void UpsertingARecordNotInTheDatabaseInserts()
        {
            _mongoGateway.UpsertRecord("test-collection-name", ObjectId.Parse(_testObjectForMongo.Id), _testObjectForMongo);
            var retrievedObject =
                    _mongoGateway.LoadRecordById<TestObjectForMongo>("test-collection-name", ObjectId.Parse(_testObjectForMongo.Id));
            retrievedObject.Should().BeEquivalentTo(_testObjectForMongo);
        }

        [Test]
        public void UpsertingARecordInTheDatabaseUpdates()
        {
            _mongoGateway.InsertRecord("test-collection-name", _testObjectForMongo);

            var retrievedObject =
                    _mongoGateway.LoadRecordById<TestObjectForMongo>("test-collection-name", ObjectId.Parse(_testObjectForMongo.Id));
            retrievedObject.Should().BeEquivalentTo(_testObjectForMongo);


            _testObjectForMongo.Property1 = "new-test-property";
            _testObjectForMongo.Property2?.Add(new TestObjectForMongo());
            _testObjectForMongo.Property3 = null;

            _mongoGateway.UpsertRecord("test-collection-name", ObjectId.Parse(_testObjectForMongo.Id), _testObjectForMongo);
            retrievedObject =
                _mongoGateway.LoadRecordById<TestObjectForMongo>("test-collection-name", ObjectId.Parse(_testObjectForMongo.Id));

            retrievedObject?.Property1.Should().Be("new-test-property");
            retrievedObject?.Property2?.Count.Should().Be(2);
            retrievedObject?.Property3.Should().BeNull();
        }

        [Test]
        public void CanLoadRecordsByFilter()
        {
            _mongoGateway.InsertRecord("test-collection-name", _testObjectForMongo);
            var id = _testObjectForMongo.Property2?[0].Id;

            var filter = Builders<TestObjectForMongo>
                .Filter
                .ElemMatch(x => x.Property2, y => y.Id == id);

            var retrievedObject = _mongoGateway.LoadRecordsByFilter("test-collection-name", filter);

            retrievedObject.Count.Should().Be(1);
            retrievedObject[0].Should().BeEquivalentTo(_testObjectForMongo);
        }
    }
}
