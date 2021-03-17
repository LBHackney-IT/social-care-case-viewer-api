using MongoDB.Bson;
using MongoDB.Driver;
using NUnit.Framework;
using System;

namespace SocialCareCaseViewerApi.Tests
{
    public class MongoDbTests
    {
        protected IMongoDatabase mongoDatabase;
        protected IMongoCollection<BsonDocument> collection;

        [OneTimeSetUp]
        public void RunBeforeAnyTests()
        {
            string MONGO_CONN_STRING = Environment.GetEnvironmentVariable("MONGO_CONN_STRING") ??
                                 @"mongodb://localhost:1433/";

            //connect to local mongo DB
            MongoClient mongoClient = new MongoClient(new MongoUrl(MONGO_CONN_STRING));
            //create a new blank database if database does not exist, otherwise get existing database
            mongoDatabase = mongoClient.GetDatabase("social_care_test_db");
            //create collection to hold the documents if it does not exist, otherwise retrieve existing
            collection = mongoDatabase.GetCollection<BsonDocument>("form_data_test");
        }

        [OneTimeTearDown]
        public void RunAfterAnyTests()
        {
            //clear collection - remove any documents inserted during the test
            collection.DeleteMany(Builders<BsonDocument>.Filter.Empty);
        }
    }
}
