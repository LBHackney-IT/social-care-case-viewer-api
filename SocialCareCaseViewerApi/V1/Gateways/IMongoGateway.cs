using System;
using System.Collections.Generic;

namespace SocialCareCaseViewerApi.V1.Gateways
{
    public interface IMongoGateway
    {
        public void InsertRecord<T>(string collectionName, T objToAdd);

        public void UpsertRecord<T>(string collectionName, Guid id, T record);

        public void DeleteRecord<T>(string collectionName, Guid id);

        public List<T> LoadRecords<T>(string collectionName);

        public T LoadRecordById<T>(string collectionName, Guid id);

        public T1 LoadRecordByProperty<T1, T2>(string collectionName, string propertyName, T2 propertyValue);
    }
}
