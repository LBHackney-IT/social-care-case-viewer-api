using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using MongoDB.Bson.Serialization.Attributes;
using NUnit.Framework;
using SocialCareCaseViewerApi.V1.Gateways;

#nullable enable
namespace SocialCareCaseViewerApi.Tests.V1.Gateways
{
    public class TestObjectForMongo
    {
        [BsonId]
        public Guid Id { get; set; }
        public string Property1 { get; set; } = null!;
        public List<TestObjectForMongo>? Property2 { get; set; }
        public TestObjectForMongo? Property3 { get; set; }
    }

    [TestFixture]
    public class MongoGatewayTests
    {
        private readonly IMongoGateway _mongoGateway = new MongoGateway("mongodb://localhost:1433/", "social_care_db");

        private TestObjectForMongo? _testObjectForMongo;

        [SetUp]
        public void Setup()
        {
            _testObjectForMongo = new TestObjectForMongo
            {
                Id = new Guid(),
                Property1 = "test-property",
                Property2 = new List<TestObjectForMongo>
                {
                    new TestObjectForMongo
                    {
                        Id = new Guid(),
                        Property1 = "test-property-list",
                        Property2 = null,
                        Property3 = null
                    }
                },
                Property3 = new TestObjectForMongo
                {
                    Id = new Guid(),
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
            TestObjectForMongo? retrievedObject = null;

            if (_testObjectForMongo != null)
            {
                retrievedObject =
                    _mongoGateway.LoadRecordById<TestObjectForMongo>("test-collection-name", _testObjectForMongo.Id);
            }
            retrievedObject.Should().BeNull();
        }

        [Test]
        public void CanInsertAndGetARecordById()
        {
            _mongoGateway.InsertRecord("test-collection-name", _testObjectForMongo);

            TestObjectForMongo? retrievedObject = null;

            if (_testObjectForMongo != null)
            {
                retrievedObject =
                    _mongoGateway.LoadRecordById<TestObjectForMongo>("test-collection-name", _testObjectForMongo.Id);
            }
            retrievedObject.Should().BeEquivalentTo(_testObjectForMongo);
        }

        [Test]
        public void CanLoadAllRecordsFromACollection()
        {
            _mongoGateway.InsertRecord("test-collection-name", _testObjectForMongo);

            List<TestObjectForMongo>? retrievedObject = null;

            if (_testObjectForMongo != null)
            {
                retrievedObject = _mongoGateway.LoadRecords<TestObjectForMongo>("test-collection-name");
            }
            retrievedObject?.Count.Should().Be(1);
            retrievedObject?[0].Should().BeEquivalentTo(_testObjectForMongo);

            _mongoGateway.InsertRecord("test-collection-name", new TestObjectForMongo());
            if (_testObjectForMongo != null)
            {
                retrievedObject = _mongoGateway.LoadRecords<TestObjectForMongo>("test-collection-name");
            }
            retrievedObject?.Count.Should().Be(2);
        }

        [Test]
        public void CanLoadARecordByACustomProperty()
        {
            _mongoGateway.InsertRecord("test-collection-name", _testObjectForMongo);

            TestObjectForMongo? retrievedObject = null;

            if (_testObjectForMongo != null)
            {
                retrievedObject =
                    _mongoGateway.LoadRecordByProperty<TestObjectForMongo, string>(
                        "test-collection-name", "Property1", _testObjectForMongo.Property1);
            }
            retrievedObject.Should().BeEquivalentTo(_testObjectForMongo);
        }

        [Test]
        public void LoadRecordByCustomPropertyReturnsNullWhenNoMatchFound()
        {
            TestObjectForMongo? retrievedObject = null;

            if (_testObjectForMongo != null)
            {
                retrievedObject =
                    _mongoGateway.LoadRecordByProperty<TestObjectForMongo, string>(
                        "test-collection-name", "Property1", _testObjectForMongo.Property1);
            }
            retrievedObject.Should().BeNull();
        }

        [Test]
        public void CanLoadMultipleRecordsByACustomProperty()
        {
            _mongoGateway.InsertRecord("test-collection-name", _testObjectForMongo);
            if (_testObjectForMongo != null)
            {
                _mongoGateway.InsertRecord("test-collection-name", new TestObjectForMongo { Property1 = _testObjectForMongo.Property1 });
            }

            List<TestObjectForMongo>? retrievedObject = null;

            if (_testObjectForMongo != null)
            {
                retrievedObject =
                    _mongoGateway.LoadMultipleRecordsByProperty<TestObjectForMongo, string>(
                        "test-collection-name", "Property1", _testObjectForMongo.Property1)
                        .ToList();
            }
            retrievedObject?.Count.Should().Be(2);
        }

        [Test]
        public void LoadMultipleRecordsByCustomerPropertyWithNoMatchReturnsEmptyList()
        {
            List<TestObjectForMongo>? retrievedObject = null;

            if (_testObjectForMongo != null)
            {
                retrievedObject =
                    _mongoGateway.LoadMultipleRecordsByProperty<TestObjectForMongo, string>(
                            "test-collection-name", "Property1", _testObjectForMongo.Property1)
                        .ToList();
            }
            retrievedObject?.Should().BeEmpty();
        }

        [Test]
        public void CanDeleteARecord()
        {
            _mongoGateway.InsertRecord("test-collection-name", _testObjectForMongo);

            TestObjectForMongo? retrievedObject = null;

            if (_testObjectForMongo != null)
            {
                retrievedObject =
                    _mongoGateway.LoadRecordById<TestObjectForMongo>("test-collection-name", _testObjectForMongo.Id);
            }
            retrievedObject.Should().BeEquivalentTo(_testObjectForMongo);

            if (_testObjectForMongo != null)
            {
                _mongoGateway.DeleteRecordById<TestObjectForMongo>("test-collection-name", _testObjectForMongo.Id);
                retrievedObject =
                    _mongoGateway.LoadRecordById<TestObjectForMongo>("test-collection-name", _testObjectForMongo.Id);
            }
            retrievedObject.Should().BeNull();
        }

        [Test]
        public void UpsertingARecordNotInTheDatabaseInserts()
        {
            TestObjectForMongo? retrievedObject = null;

            if (_testObjectForMongo != null)
            {
                _mongoGateway.UpsertRecord("test-collection-name", _testObjectForMongo.Id, _testObjectForMongo);
                retrievedObject =
                    _mongoGateway.LoadRecordById<TestObjectForMongo>("test-collection-name", _testObjectForMongo.Id);
            }
            retrievedObject.Should().BeEquivalentTo(_testObjectForMongo);
        }

        [Test]
        public void UpsertingARecordInTheDatabaseUpdates()
        {
            TestObjectForMongo? retrievedObject = null;
            _mongoGateway.InsertRecord("test-collection-name", _testObjectForMongo);

            if (_testObjectForMongo != null)
            {
                retrievedObject =
                    _mongoGateway.LoadRecordById<TestObjectForMongo>("test-collection-name", _testObjectForMongo.Id);
            }
            retrievedObject.Should().BeEquivalentTo(_testObjectForMongo);

            if (_testObjectForMongo != null)
            {
                _testObjectForMongo.Property1 = "new-test-property";
                _testObjectForMongo.Property2?.Add(new TestObjectForMongo());
                _testObjectForMongo.Property3 = null;

                _mongoGateway.UpsertRecord("test-collection-name", _testObjectForMongo.Id, _testObjectForMongo);
                retrievedObject =
                    _mongoGateway.LoadRecordById<TestObjectForMongo>("test-collection-name", _testObjectForMongo.Id);
            }

            retrievedObject?.Property1.Should().Be("new-test-property");
            retrievedObject?.Property2?.Count.Should().Be(2);
            retrievedObject?.Property3.Should().BeNull();
        }
    }
}
