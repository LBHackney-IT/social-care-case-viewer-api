using Amazon.Lambda.Core;
using Npgsql;

namespace PostgreSQLImport.Database
{
    public interface  IDatabaseActions
    {
        int TruncateTable(ILambdaContext context, string tableName, NpgsqlTransaction transaction);
        int CopyDataToDatabase(ILambdaContext context, string awsRegion, string bucketName, string objectKey, string tableName, NpgsqlTransaction transaction);
        NpgsqlConnection SetupDatabase(ILambdaContext context);
    }
}
