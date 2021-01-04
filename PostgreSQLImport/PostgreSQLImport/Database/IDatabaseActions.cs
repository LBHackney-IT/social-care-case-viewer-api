using Amazon.Lambda.Core;
using Npgsql;

namespace PostgreSQLImport.Database
{
    public interface  IDatabaseActions
    {
        int TruncateTable(ILambdaContext context, string tableName);
        int CopyDataToDatabase(ILambdaContext context, string awsRegion, string bucketName, string objectKey, string tableName);
        NpgsqlConnection SetupDatabase(ILambdaContext context);
    }
}
