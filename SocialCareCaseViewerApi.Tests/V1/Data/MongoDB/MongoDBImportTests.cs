//using AutoFixture;
//using FluentAssertions;
//using MongoDB.Bson;
//using MongoDB.Driver;
//using NUnit.Framework;
//using System;
//using System.Linq;

//#nullable enable
//namespace SocialCareCaseViewerApi.Tests.V1.Data.MongoDB
//{
//    [TestFixture]
//    public class MongoDBImportTests
//    {
//        private readonly Fixture _fixture = new Fixture();
//        private const string MainFormCollection = "form_data";
//        private IMongoDatabase? _mongoDatabase;

//        private static readonly JsonCommand<BsonDocument> _dbCommandUnderTest = new JsonCommand<BsonDocument>(
//                @"{
//                    delete: ""form_data"",
//                    deletes:
//                        [
//                            {
//                                q: { MarkedForDeletion: ""true"" }, limit: 0
//                            }
//                        ]
//                }");

//        /// <summary>
//        ///  This script deletes all documents marked for deletion from form_data collection. Limit: 0 means all matching records will be deleted 
//        /// </summary>
//        /// <howto>
//        /// To run this script manually on the server:
//        /// - pass it to the db.runCommand() like this:
//        ///
//        /// db.runCommand({
//        /// delete: ""form_data"",
//        ///            deletes:
//        ///                [
//        ///                    {
//        ///                        q: { MarkedForDeletion: ""true"" }, limit: 0
//        ///                    }
//        ///                ]
//        ///        });
//        /// </howto>     


//        [SetUp]
//        public void SetUp()
//        {
//            string mongoConnectionString = Environment.GetEnvironmentVariable("SCCV_MONGO_CONN_STRING") ??
//                                           Environment.GetEnvironmentVariable("MONGO_CONN_STRING") ??
//                                           @"mongodb://localhost:1433/";

//            string databaseName = Environment.GetEnvironmentVariable("SCCV_MONGO_DB_NAME") ?? "social_care_db_name";

//            var mongoClient = new MongoClient(new MongoUrl(mongoConnectionString));
//            _mongoDatabase = mongoClient.GetDatabase(databaseName);
//        }

//        [TearDown]
//        public void TearDown()
//        {
//            _mongoDatabase?.DropCollection(MainFormCollection);
//        }

//        [Test]
//        public void DeletesRecordsMarkedForDeletion()
//        {
//            var casenoteOne = _fixture.Build<MongoDBTestObject>().With(x => x.MarkedForDeletion, "true").Create();

//            var collection = _mongoDatabase?.GetCollection<MongoDBTestObject>(MainFormCollection);

//            collection?.InsertOne(casenoteOne);

//            //verify filter
//            var filterByMarkedForDeletionFlag = Builders<MongoDBTestObject>.Filter.Eq("MarkedForDeletion", "true");
//            var originalMatchingRecords = collection.Find(filterByMarkedForDeletionFlag).ToList();
//            originalMatchingRecords.Count.Should().Be(1);

//            var result = _mongoDatabase?.RunCommand(_dbCommandUnderTest);

//            var newMatchingRecords = collection.Find(filterByMarkedForDeletionFlag).ToList();
//            newMatchingRecords.Count.Should().Be(0);
//        }

//        [Test]
//        public void DoesNotDeleteRecordsNotMarkedForDeletion()
//        {
//            var casenoteOne = _fixture.Build<MongoDBTestObject>().With(x => x.MarkedForDeletion, "true").Create();
//            var casenoteTwo = _fixture.Build<MongoDBTestObject>().With(x => x.MarkedForDeletion, "false").Create();
//            var casenoteThree = _fixture.Create<MongoDBTestObject>();

//            var collection = _mongoDatabase?.GetCollection<MongoDBTestObject>(MainFormCollection);

//            collection?.InsertOne(casenoteOne);
//            collection?.InsertOne(casenoteTwo);
//            collection?.InsertOne(casenoteThree);

//            var result = _mongoDatabase?.RunCommand(_dbCommandUnderTest);

//            var filterByMarkedForDeletionFlag = Builders<MongoDBTestObject>.Filter.Eq("MarkedForDeletion", "true");
//            var newMatchingRecords = collection.Find(filterByMarkedForDeletionFlag).ToList();
//            newMatchingRecords.Count.Should().Be(0);

//            var unchangedRecordsFilter = Builders<MongoDBTestObject>.Filter.Ne("MarkedForDeletion", "true");
//            var unchangedRecords = collection.Find(unchangedRecordsFilter).ToList();
//            unchangedRecords.Count.Should().Be(2);
//        }
//    }
//}
