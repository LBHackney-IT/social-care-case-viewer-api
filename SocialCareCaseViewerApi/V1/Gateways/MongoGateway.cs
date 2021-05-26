using System;
using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Driver;

#nullable enable
namespace SocialCareCaseViewerApi.V1.Gateways
{
    public class MongoGateway : IMongoGateway
    {
        private readonly IMongoDatabase _mongoDatabase;

        public MongoGateway()
        {
            var mongoClient = new MongoClient(new MongoUrl(Environment.GetEnvironmentVariable("SCCV_MONGO_CONN_STRING")));
            _mongoDatabase = mongoClient.GetDatabase(Environment.GetEnvironmentVariable("SCCV_MONGO_DB_NAME"));
        }

        public void InsertRecord<T>(string collectionName, T objToAdd)
        {
            var collection = _mongoDatabase.GetCollection<T>(collectionName);
            collection.InsertOne(objToAdd);
        }

        public void UpsertRecord<T>(string collectionName, Guid id, T record)
        {
            var collection = _mongoDatabase.GetCollection<T>(collectionName);
            var filter = new BsonDocument("_id", new BsonBinaryData(id, GuidRepresentation.Standard));
            collection.ReplaceOne(filter, record, new ReplaceOptions { IsUpsert = true });
        }

        public void DeleteRecord<T>(string collectionName, Guid id)
        {
            var collection = _mongoDatabase.GetCollection<T>(collectionName);
            var filter = Builders<T>.Filter.Eq("Id", id);
            collection.DeleteOne(filter);
        }

        public List<T> LoadRecords<T>(string collectionName)
        {
            var collection = _mongoDatabase.GetCollection<T>(collectionName);
            return collection.Find(new BsonDocument()).ToList();
        }

        public T LoadRecordById<T>(string collectionName, Guid id)
        {
            var collection = _mongoDatabase.GetCollection<T>(collectionName);
            var filter = Builders<T>.Filter.Eq("Id", id);
            return collection.Find(filter).FirstOrDefault();
        }

        public T1 LoadRecordByProperty<T1, T2>(string collectionName, string propertyName, T2 propertyValue)
        {
            var collection = _mongoDatabase.GetCollection<T1>(collectionName);
            var filter = Builders<T1>.Filter.Eq(propertyName, propertyValue);
            return collection.Find(filter).FirstOrDefault();
        }
    }
}
