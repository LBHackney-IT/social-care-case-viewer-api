using Amazon.Lambda.Core;
using FluentAssertions;
using Moq;
using Npgsql;
using NUnit.Framework;
using PostgreSQLImport.Database;

namespace PostgreSQLImport.Tests
{
    //[TestFixture]
    //public class DatabaseTests
    //{
    //    [Test]
    //    public void CanTruncateTable()
    //    {
    //        Mock<ILambdaContext> contextMock = new Mock<ILambdaContext>();

    //        var databaseActions = new DatabaseActions();
    //        var dbConnection = databaseActions.SetupDatabase(contextMock.Object);

    //        //create and insert data to test against
    //        var npgsqlCommand = dbConnection.CreateCommand();
    //        var truncateTableQuery = @"CREATE TABLE postgresImportTest (id, int);";
    //        npgsqlCommand.CommandText = truncateTableQuery;
    //        npgsqlCommand.ExecuteNonQuery();

    //        npgsqlCommand.CommandText = @"INSERT INTO postgresImportTest values (id, 1)";
    //        npgsqlCommand.ExecuteNonQuery();

    //        var result = databaseActions.TruncateTable(contextMock.Object, "postgresImportTest");

    //        result.Should().Be(1);
    //    }

    //    [Test]
    //    public void CanSetupDatabaseConnection()
    //    {
    //        var contextMock = new Mock<ILambdaContext>();
    //        var databaseActions = new DatabaseActions();

    //        var result = databaseActions.SetupDatabase(contextMock.Object);

    //        result.Should().NotBeNull();
    //        result.Should().BeOfType<NpgsqlConnection>();
    //    }
    //}
}
