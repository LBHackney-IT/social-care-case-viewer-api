using Amazon.Lambda.Core;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Util;
using CsvHelper;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]
namespace MongoDBImport
{
    public class Handler
    {
        public void ImportFormData(S3EventNotification s3Event, ILambdaContext context)
        {
            //Add proper triggers and env vars once POC phase is done
            List<BsonDocument> csvToBsonRecords = new List<BsonDocument>();

            //check the events
            foreach (var record in s3Event.Records)
            {
                //only handle hard coded test file for now, these need to be added to serverless trigger setup
                if (record.AwsRegion.ToString().ToUpper() == "EU-WEST-2"
                        && record.S3.Bucket.Name.ToUpper() == "TEST-SOCIAL-CARE-DATA"
                        && record.S3.Object.Key.ToUpper() == "SAMPLECSV.CSV")
                {
                    LambdaLogger.Log($"Correct file: {record.AwsRegion}/{record.S3.Bucket.Name}/{record.S3.Object.Key}, processing");

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
                                    var records = csv.GetRecords<dynamic>(); //will read in chunks as longs as .ToList() etc. is not used 

                                    //tidy up the data set to ensure we don't have '.'s in the field names since they are invalid in Mongo. This can be removed after data cleansing
                                    foreach (var cvsRecord in records)
                                    {
                                        var tmpItem = (IDictionary<string, object>) cvsRecord;

                                        Dictionary<string, object> newItem = new Dictionary<string, object>();

                                        foreach (var keyValuePair in tmpItem)
                                        {
                                            newItem.Add(!keyValuePair.Key.Contains('.') ? keyValuePair.Key : keyValuePair.Key.Replace(".", ""), keyValuePair.Value);
                                        }

                                        BsonDocument bd = newItem.ToBsonDocument();

                                        csvToBsonRecords.Add(bd);
                                    }
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
            //TODO: use proper usecases for this
            if (csvToBsonRecords.Count > 0)
            {
                //import the records
                try
                {
                    //TODO: use use case 
                    var client = new MongoClient(Environment.GetEnvironmentVariable("SCCV_MONGO_CONN_STRING"));

                    var database = client.GetDatabase(Environment.GetEnvironmentVariable("SCCV_MONGO_DB_NAME"));

                    //TODO: use correct collection
                    var userCollection = database.GetCollection<BsonDocument>("s3_csv_test");

                    var requests = csvToBsonRecords.Select(record => new ReplaceOneModel<BsonDocument>(new BsonDocument("unique_id", record.GetValue("unique_id")), record) { IsUpsert = true });

                    var bulkUpsertResult = userCollection.BulkWrite(requests, new BulkWriteOptions { IsOrdered = false });

                    var upserts = bulkUpsertResult.Upserts.ToList();

                    Console.WriteLine($"OK?: {bulkUpsertResult.IsAcknowledged}");
                    Console.WriteLine($"Updated: {bulkUpsertResult.ModifiedCount}");
                    Console.WriteLine($"Added: {bulkUpsertResult.Upserts.Count}");
                }
                catch (Exception ex)
                {
                    LambdaLogger.Log($"Import failed: {ex.Message}");
                }
            }
        }
    }
}
