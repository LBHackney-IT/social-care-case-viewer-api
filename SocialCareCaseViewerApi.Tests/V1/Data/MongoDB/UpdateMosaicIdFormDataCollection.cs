using AutoFixture;
using FluentAssertions;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using NUnit.Framework;
using SocialCareCaseViewerApi.V1.Infrastructure;
using System;
using System.Linq;

#nullable enable
namespace SocialCareCaseViewerApi.Tests.V1.Data.MongoDB
{
    [TestFixture]
    public class UpdateMosaicIdFormDataCollection
    {
        private readonly Fixture _fixture = new Fixture();
        private const string MainFormCollection = "form_data";
        private IMongoDatabase? _mongoDatabase;

        private static readonly JsonCommand<BsonDocument> _dbCommandUnderTest = new JsonCommand<BsonDocument>(
                @"{
                    update: ""form_data"",
                    updates:
                        [
                            {
                                q: { MosaicId: ""334445566"", FormNameOverall: { $not: /^API_/ } }, u: { $set: { MosaicId: ""77889900"" } }, multi: true
                            }
                        ]
                }");

        /// <summary>
        ///  This script is designed to be run against a single ID as a workaround until the appropriate API endpoint is in place
        ///  Only runs against form_data collection
        ///  Updates all matching records that are not any of the audit event types created by the service API
        /// </summary>
        /// <howto>
        /// To run this script manually on the server:
        /// - remove double quotes
        /// - add appropriate IDs (one in the query (q) is the current one and the one in the $set is the new master id)
        /// - pass it to the db.runCommand() like this:
        ///
        /// db.runCommand({
        ///  update: "form_data",
        ///  updates:
        ///      [
        ///          {
        ///              q: { MosaicId: "334445566", FormNameOverall: { $not: /^API_/ } }, u: { $set: { MosaicId: "77889900" } }, multi: true
        ///          }
        ///      ]
        /// });
        /// </howto>      


        [SetUp]
        public void SetUp()
        {
            string mongoConnectionString = Environment.GetEnvironmentVariable("SCCV_MONGO_CONN_STRING") ??
                                           Environment.GetEnvironmentVariable("MONGO_DB_TEST_CONN_STRING") ??
                                           @"mongodb://localhost:1433/";

            string databaseName = Environment.GetEnvironmentVariable("SCCV_MONGO_DB_NAME") ?? "social_care_db_name";

            var mongoClient = new MongoClient(new MongoUrl(mongoConnectionString));
            _mongoDatabase = mongoClient.GetDatabase(databaseName);
        }

        [TearDown]
        public void TearDown()
        {
            _mongoDatabase?.DropCollection(MainFormCollection);
        }

        [Test]
        public void UpdatesMosaicIdToNewMasterPersonIdForAllMatchingRecords()
        {
            string mosaicId = "334445566";
            string masterPersonId = "77889900";

            var casenoteOne = _fixture.Build<MongoDBTestObject>().With(x => x.MosaicId, mosaicId).Create();
            var casenoteTwo = _fixture.Build<MongoDBTestObject>().With(x => x.MosaicId, mosaicId).Create();
            var casenoteThree = _fixture.Build<MongoDBTestObject>().With(x => x.MosaicId, mosaicId).Create();

            var collection = _mongoDatabase?.GetCollection<MongoDBTestObject>(MainFormCollection);

            collection?.InsertOne(casenoteOne);
            collection?.InsertOne(casenoteTwo);
            collection?.InsertOne(casenoteThree);

            var result = _mongoDatabase?.RunCommand(_dbCommandUnderTest);

            var filterByNewId = Builders<MongoDBTestObject>.Filter.Eq("MosaicId", masterPersonId);
            var recordsWithNewId = collection.Find(filterByNewId).ToList();
            recordsWithNewId.Count.Should().Be(3);

            var filterByOldId = Builders<MongoDBTestObject>.Filter.Eq("MosaicId", mosaicId);
            var recordsWithOldId = collection.Find(filterByOldId).ToList();
            recordsWithOldId.Count.Should().Be(0);
        }

        [Test]
        public void DoesNotUpdateOtherThanMatchingRecords()
        {
            string mosaicId = "334445566";
            string mosaicIdTwo = "556677";
            string masterPersonId = "77889900";

            var casenoteOneForMosaicId = _fixture.Build<MongoDBTestObject>().With(x => x.MosaicId, mosaicId).Create();
            var casenoteTwoForMosaicId = _fixture.Build<MongoDBTestObject>().With(x => x.MosaicId, mosaicId).Create();
            var casenoteThreeForMosaicIdTwo = _fixture.Build<MongoDBTestObject>().With(x => x.MosaicId, mosaicIdTwo).Create();

            var collection = _mongoDatabase?.GetCollection<MongoDBTestObject>(MainFormCollection);

            collection?.InsertOne(casenoteOneForMosaicId);
            collection?.InsertOne(casenoteTwoForMosaicId);
            collection?.InsertOne(casenoteThreeForMosaicIdTwo);

            var result = _mongoDatabase?.RunCommand(_dbCommandUnderTest);

            var filterByNewId = Builders<MongoDBTestObject>.Filter.Eq("MosaicId", masterPersonId);
            var recordsWithNewId = collection.Find(filterByNewId).ToList();
            recordsWithNewId.Count.Should().Be(2);

            var filterByExcludedId = Builders<MongoDBTestObject>.Filter.Eq("MosaicId", mosaicIdTwo);
            var recordsWitExcludedId = collection.Find(filterByExcludedId).ToList();
            recordsWitExcludedId.Count.Should().Be(1);
        }

        [Test]
        public void DoesNotUpdateAuditTypeRecordsThatHaveAPIInTheFormNameOverallField()
        {
            string mosaicId = "334445566";

            var casenoteOne = _fixture.Build<MongoDBTestObject>().With(x => x.MosaicId, mosaicId).With(x => x.FormNameOverall, "API_Event").Create();
            var casenoteTwo = _fixture.Build<MongoDBTestObject>().With(x => x.MosaicId, mosaicId).Create();
            var casenoteThree = _fixture.Build<MongoDBTestObject>().With(x => x.MosaicId, mosaicId).Create();

            var collection = _mongoDatabase?.GetCollection<MongoDBTestObject>(MainFormCollection);

            collection?.InsertOne(casenoteOne);
            collection?.InsertOne(casenoteTwo);
            collection?.InsertOne(casenoteThree);

            var result = _mongoDatabase?.RunCommand(_dbCommandUnderTest);

            var filterByFormNameOverall = Builders<MongoDBTestObject>.Filter.Eq("FormNameOverall", "API_Event");
            var filteredRecords = collection.Find(filterByFormNameOverall).ToList();

            filteredRecords.Count.Should().Be(1);
            filteredRecords.First().MosaicId.Should().Be(mosaicId);
        }
    }

    [BsonIgnoreExtraElements]
    public class MongoDBTestObject : CaseNoteBase
    {
    }
}
