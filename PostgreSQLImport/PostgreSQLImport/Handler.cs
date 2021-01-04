using Amazon.Lambda.Core;
using Amazon.S3.Util;
using Npgsql;
using PostgreSQLImport.Database;
using System;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]
namespace PostgreSQLImport
{
    public class Handler
    {
        private IDatabaseActions _databaseActions;

        public Handler(IDatabaseActions databaseActions)
        {
            _databaseActions = databaseActions;
        }

        public Handler() : this(new DatabaseActions()) { }


        //TODO: run this on trigger, but schedule the file drop out of hours
        public void ImportData(S3EventNotification s3Event, ILambdaContext context)
        {
            try
            {
                //setup db connection
                NpgsqlConnection connection = _databaseActions.SetupDatabase(context);

                //check the events
                foreach (var record in s3Event.Records)
                {
                    switch (record.S3.Object.Key.ToUpper())
                    {
                        case "CFS_ALLOCATIONS_REMOVE_ME_.CSV": //TODO: remove after initial POC
                            {
                                using (var transaction = connection.BeginTransaction()) //TODO: check transaction handling
                                {
                                    //truncate table
                                    int truncateResult = _databaseActions.TruncateTable(context, "SCCV_ALLOCATIONS");
                                    LambdaLogger.Log($"{truncateResult} rows affected");

                                    //import data
                                    int importResult = _databaseActions.CopyDataToDatabase(context, record.AwsRegion, record.S3.Bucket.Name, record.S3.Object.Key, "SCCV_ALLOCATIONS");
                                    LambdaLogger.Log($"{importResult} rows affected");
                                    transaction.Commit();
                                }
                                return;
                            }

                        default:
                            LambdaLogger.Log($"{record.S3.Object.Key} not recognised as a file to be imported");
                            return;
                    }
                }
                LambdaLogger.Log("Data import complete");
                connection.Close();
            }
            catch (Exception ex)
            {
                LambdaLogger.Log($"Error occurred while importing data: {ex.Message}");
                throw ex;
            }
        }
    }
}
