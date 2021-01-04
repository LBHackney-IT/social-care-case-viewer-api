using Amazon.Lambda.Core;
using Npgsql;
using System;

namespace PostgreSQLImport.Database
{
    public class DatabaseActions : IDatabaseActions
    {
        private NpgsqlConnection _npgsqlConnection;
        public NpgsqlConnection SetupDatabase(ILambdaContext context)
        {
            LambdaLogger.Log("set up DB");
            var connString = $"{Environment.GetEnvironmentVariable("CONNECTION_STRING")};CommandTimeout=30;";

            try
            {
                var connection = new NpgsqlConnection(connString);
                LambdaLogger.Log("Opening DB connection");
                connection.Open();
                _npgsqlConnection = connection; 
                return connection;
            }
            catch (Exception ex)
            {
                LambdaLogger.Log($"Exception has occurred while setting up DB connection - {ex.Message} {ex.InnerException} {ex.StackTrace}");
                throw ex;
            }
        }

        public int TruncateTable(ILambdaContext context, string tableName, NpgsqlTransaction transaction)
        {
            var truncateCommand = _npgsqlConnection.CreateCommand();
            truncateCommand.Transaction = transaction;

            LambdaLogger.Log($"Table name to truncate {tableName}");
            var truncateTableQuery = $"TRUNCATE TABLE {tableName};";
            truncateCommand.CommandText = truncateTableQuery;
            var rowsAffected = truncateCommand.ExecuteNonQuery();

            return rowsAffected;
        }

        public int CopyDataToDatabase(ILambdaContext context, string awsRegion, string bucketName, string objectKey, string tableName, NpgsqlTransaction transaction)
        {
            var loadDataCommand = _npgsqlConnection.CreateCommand();
            loadDataCommand.Transaction = transaction;

            var loadDataFromCSV = $@"SELECT aws_s3.table_import_from_s3('{tableName}','','(FORMAT csv, HEADER)',@bucket, @objectkey, @awsregion);";
            loadDataCommand.CommandText = loadDataFromCSV;
            loadDataCommand.Parameters.AddWithValue("bucket", bucketName);
            loadDataCommand.Parameters.AddWithValue("objectkey", objectKey);
            loadDataCommand.Parameters.AddWithValue("awsregion", awsRegion);

            var rowsAffected = loadDataCommand.ExecuteNonQuery();

            if (rowsAffected == 0)
            {
                //no insert has occured
                LambdaLogger.Log($"Load has failed - no rows were affected. Ensure the file contained data");
                throw new NpgsqlException($"Load has failed - no rows were loaded from file {bucketName}/{objectKey} in region {awsRegion}");
            }

            return rowsAffected;
        }
    }
}
