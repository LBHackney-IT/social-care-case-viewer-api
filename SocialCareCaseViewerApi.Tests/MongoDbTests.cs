using System;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using SocialCareCaseViewerApi.V1.Infrastructure;
using MongoDB.Bson;
using MongoDB.Driver;

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
                                 @"mongodb://localhost:27017/?gssapiServiceName=mongodb";

            //connect to local mongo DB
            MongoClient mongoClient = new MongoClient(new MongoUrl(MONGO_CONN_STRING));
            //create a new blank database if database does not exist, otherwise get existing database
            mongoDatabase = mongoClient.GetDatabase("social_care_db");
            //create collection to hold the documents if it does not exist, otherwise retrieve existing
            collection = mongoDatabase.GetCollection<BsonDocument>("form_data");
        }

        [OneTimeTearDown]
        public void RunAfterAnyTests()
        {
           //clear collection - remove any documents inserted during thet test
           collection.DeleteMany(Builders<BsonDocument>.Filter.Empty);
        }
    }
}