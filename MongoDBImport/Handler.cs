using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Amazon.Lambda.Core;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Util;
using CsvHelper;
using MongoDB.Bson;
using MongoDB.Driver;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]
namespace MongoDBImport
{
    public class Handler
    {
        private static string _collectionName = Environment.GetEnvironmentVariable("SCCV_MONGO_IMPORT_COLLECTION_NAME");
        private static string _importFileName = Environment.GetEnvironmentVariable("SCCV_MONGO_IMPORT_FILE_NAME");
        private static string _uniqueId = "unique_id";

        public void ImportFormData(S3EventNotification s3Event, ILambdaContext context)
        {
            //Add proper triggers and env vars once POC phase is done
            List<BsonDocument> csvToBsonRecords = new List<BsonDocument>();

            //check the events
            foreach (var record in s3Event.Records)
            {
                //only handle single, predefined import file for now
                if (record.S3.Object.Key.ToUpper() == _importFileName.ToUpper())
                {
                    LambdaLogger.Log($"File to be processed: {record.AwsRegion}/{record.S3.Bucket.Name}/{record.S3.Object.Key}, processing");

                    //load the file from the bucket
                    using (var s3Client = new AmazonS3Client(Amazon.RegionEndpoint.EUWest2))
                    {
                        GetObjectRequest request = new GetObjectRequest
                        {
                            BucketName = record.S3.Bucket.Name,
                            Key = record.S3.Object.Key
                        };
                        LambdaLogger.Log($"getting object:{ request.BucketName}{ request.Key}");

                        GetObjectResponse response = null;

                        try
                        {
                            response = s3Client.GetObjectAsync(request).Result;
                            LambdaLogger.Log("response:" + response.HttpStatusCode.ToString());
                        }
                        catch (Exception ex)
                        {
                            LambdaLogger.Log("s3 client connection error:" + ex.Message);
                        }

                        if (response?.ResponseStream != null)
                        {
                            try
                            {
                                using (var reader = new StreamReader(response.ResponseStream))
                                using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                                {
                                    csvToBsonRecords = csv.GetRecords<FormRecord>().Select(x => x.ToBsonDocument()).ToList();
                                }
                                LambdaLogger.Log($"{csvToBsonRecords.Count} records to be processed");
                            }
                            catch (Exception ex)
                            {
                                LambdaLogger.Log("csv stream handling error:" + ex.Message);
                            }
                        }
                        else
                        {
                            LambdaLogger.Log("stream from S3 is null");
                        }
                    };
                }
                else
                {
                    LambdaLogger.Log("nothing to process");
                }
            }

            //check if we have anything to process
            if (csvToBsonRecords.Count > 0)
            {
                //import the records
                try
                {
                    var mongoClient = new MongoClient(Environment.GetEnvironmentVariable("SCCV_MONGO_CONN_STRING"));

                    var database = mongoClient.GetDatabase(Environment.GetEnvironmentVariable("SCCV_MONGO_DB_NAME"));

                    var collection = database.GetCollection<BsonDocument>(_collectionName);

                    var records = csvToBsonRecords.Select(record => new ReplaceOneModel<BsonDocument>(new BsonDocument(_uniqueId, record.GetValue(_uniqueId)), record) { IsUpsert = true });

                    var bulkUpsertResult = collection.BulkWrite(records, new BulkWriteOptions { IsOrdered = false });

                    var upserts = bulkUpsertResult.Upserts.ToList();

                    Console.WriteLine($"Bulk upsert operation successful? {bulkUpsertResult.IsAcknowledged}");
                    Console.WriteLine($"Updated: {bulkUpsertResult.ModifiedCount} records");
                    Console.WriteLine($"Added: {bulkUpsertResult.Upserts.Count} records");
                }
                catch (Exception ex)
                {
                    LambdaLogger.Log($"Import failed: {ex.Message}");
                }
            }
        }
    }
}
