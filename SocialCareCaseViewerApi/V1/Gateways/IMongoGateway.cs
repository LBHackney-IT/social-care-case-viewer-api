using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Driver;
using SocialCareCaseViewerApi.V1.Helpers;

#nullable enable
namespace SocialCareCaseViewerApi.V1.Gateways
{
    public interface IMongoGateway
    {
        public void DropCollection(string collectionName);
        public void InsertRecord<T>(string collectionName, T objToAdd);
        public void UpsertRecord<T>(string collectionName, BsonObjectId id, T record);
        public void DeleteRecordById<T>(string collectionName, BsonObjectId id);
        public List<T> LoadRecords<T>(string collectionName);
        public T LoadRecordById<T>(string collectionName, BsonObjectId id);
        public List<T1> LoadMultipleRecordsByProperty<T1, T2>(string collectionName, string propertyName,
            T2 propertyValue);
        public T1 LoadRecordByProperty<T1, T2>(string collectionName, string propertyName, T2 propertyValue);
        public (List<T1>, long) LoadRecordsByFilter<T1>(string collectionName, FilterDefinition<T1> filter, Pagination? pagination);
    }
}
