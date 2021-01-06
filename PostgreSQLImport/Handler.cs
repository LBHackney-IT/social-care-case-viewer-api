using Amazon.Lambda.Core;
using Npgsql;
using PostgreSQLImport.Database;
using System;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]
namespace PostgreSQLImport
{
    public class Handler
    {
        private IDatabaseActions _databaseActions;
        private static readonly string _awsRegion = "eu-west-2";
        private static readonly string _bucketName = Environment.GetEnvironmentVariable("BUCKET_NAME");
        private static readonly string _importFileName = Environment.GetEnvironmentVariable("ALLOCATIONS_IMPORT_FILE_NAME");

        public Handler(IDatabaseActions databaseActions)
        {
            _databaseActions = databaseActions;
        }

        public Handler() : this(new DatabaseActions()) { }

        public void ImportData(ILambdaContext context)
        {
            try
            {
                NpgsqlConnection connection = _databaseActions.SetupDatabase(context);

                using (var transaction = connection.BeginTransaction())
                {
                    //change date style for this session
                    //TODO: do this at db level once format has been standardised
                    int changeDateStyleResult = _databaseActions.ChangeDateStyleToDMY(transaction);
                    LambdaLogger.Log($"Date style change complete");

                    //truncate table
                    int truncateResult = _databaseActions.TruncateTable(context, "dbo.sccv_allocations", transaction);
                    LambdaLogger.Log("Table truncate complete");

                    //import data
                    int importResult = _databaseActions.CopyDataToDatabase(context, _awsRegion, _bucketName, _importFileName, "dbo.sccv_allocations", transaction);
                    LambdaLogger.Log("Data import complete");
                    transaction.Commit();
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
