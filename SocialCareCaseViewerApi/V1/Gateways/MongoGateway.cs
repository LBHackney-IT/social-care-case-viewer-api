using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Driver;
using SocialCareCaseViewerApi.V1.Helpers;

#nullable enable
namespace SocialCareCaseViewerApi.V1.Gateways
{
    public class MongoGateway : IMongoGateway
    {
        private readonly IMongoDatabase _mongoDatabase;

        public MongoGateway()
        {
            string mongoConnectionString = Environment.GetEnvironmentVariable("SCCV_MONGO_CONN_STRING") ??
                                           Environment.GetEnvironmentVariable("MONGO_DB_TEST_CONN_STRING") ??
                                           @"mongodb://localhost:1433/";

            string databaseName = Environment.GetEnvironmentVariable("SCCV_MONGO_DB_NAME") ?? "social_care_db_name";

            var mongoClient = new MongoClient(new MongoUrl(mongoConnectionString));
            _mongoDatabase = mongoClient.GetDatabase(databaseName);
        }

        public void DropCollection(string collectionName)
        {
            _mongoDatabase.DropCollection(collectionName);
        }

        public void InsertRecord<T>(string collectionName, T objToAdd)
        {
            var collection = _mongoDatabase.GetCollection<T>(collectionName);
            collection.InsertOne(objToAdd);
        }

        public void UpsertRecord<T>(string collectionName, BsonObjectId id, T record)
        {
            var collection = _mongoDatabase.GetCollection<T>(collectionName);
            var filter = new BsonDocument("_id", id);
            collection.ReplaceOne(filter, record, new ReplaceOptions { IsUpsert = true });
        }

        public void DeleteRecordById<T>(string collectionName, BsonObjectId id)
        {
            var collection = _mongoDatabase.GetCollection<T>(collectionName);
            var filter = Builders<T>.Filter.Eq("_id", id);
            collection.DeleteOne(filter);
        }

        public List<T> LoadRecords<T>(string collectionName)
        {
            var collection = _mongoDatabase.GetCollection<T>(collectionName);
            return collection.Find(new BsonDocument()).ToList();
        }

        public T LoadRecordById<T>(string collectionName, BsonObjectId id)
        {
            var collection = _mongoDatabase.GetCollection<T>(collectionName);
            var filter = Builders<T>.Filter.Eq("_id", id);
            return collection.Find(filter).FirstOrDefault();
        }

        public List<T1> LoadMultipleRecordsByProperty<T1, T2>(string collectionName, string propertyName, T2 propertyValue)
        {
            var collection = _mongoDatabase.GetCollection<T1>(collectionName);
            var filter = Builders<T1>.Filter.Eq(propertyName, propertyValue);
            return collection.Find(filter).ToList();
        }

        public T1 LoadRecordByProperty<T1, T2>(string collectionName, string propertyName, T2 propertyValue)
        {
            var collection = _mongoDatabase.GetCollection<T1>(collectionName);
            var filter = Builders<T1>.Filter.Eq(propertyName, propertyValue);
            return collection.Find(filter).FirstOrDefault();
        }

        public List<T1> LoadRecordsByFilter<T1>(string collectionName, FilterDefinition<T1> filter, Pagination? pagination = null)
        {
            var collection = _mongoDatabase.GetCollection<T1>(collectionName);

            if (pagination == null) return collection.Find(filter).ToList();

            var skip = pagination.Size * (pagination.Page - 1);
            return collection.Find(filter).Skip(skip).Limit(pagination.Size).ToList();

        }
    }


    public static class MongoConnectionStrings
    {
        public static readonly Dictionary<Collection, string> Map = new Dictionary<Collection, string>
        {
            {Collection.ResidentCaseSubmissions, "resident-case-submissions"}
        };
    }

    public enum Collection
    {
        ResidentCaseSubmissions
    }
}
