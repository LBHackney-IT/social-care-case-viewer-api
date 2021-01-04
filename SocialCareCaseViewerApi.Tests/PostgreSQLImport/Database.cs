//using Amazon.Lambda.Core;
//using FluentAssertions;
//using Moq;
//using Npgsql;
//using NUnit.Framework;
//using PostgreSQLImport.Database;

//namespace SocialCareCaseViewerApi.Tests.PostgreSQLImport
//{
//    //public class DatabaseTests
//    //{

//    //    [Test]
//    //    public void CanTruncateTable()
//    //    {
//    //        Mock<ILambdaContext> contextMock = new Mock<ILambdaContext>();

//    //        var databaseActions = new DatabaseActions();
//    //        var dbConnection = databaseActions.SetupDatabase(contextMock.Object);

//    //        //drop existing table
//    //        var dropTableCommand = dbConnection.CreateCommand();
//    //        var dropTableQuery = @"DROP TABLE dbo.postgresImportTest;";
//    //        dropTableCommand.CommandText = dropTableQuery;
//    //        dropTableCommand.ExecuteNonQuery();

//    //        var npgsqlCommand = dbConnection.CreateCommand();
//    //        var truncateTableQuery = @"CREATE TABLE dbo.postgresImportTest (id int);";
//    //        npgsqlCommand.CommandText = truncateTableQuery;
//    //        npgsqlCommand.ExecuteNonQuery();

//    //        npgsqlCommand.CommandText = @"INSERT INTO dbo.postgresImportTest (id) values (1)";
//    //        npgsqlCommand.ExecuteNonQuery();

//    //        var result = databaseActions.TruncateTable(contextMock.Object, "dbo.postgresImportTest");

//    //        result.Should().Be(-1);
//    //    }

//    //    [Test]
//    //    public void CanSetupDatabaseConnection()
//    //    {
//    //        var contextMock = new Mock<ILambdaContext>();
//    //        var databaseActions = new DatabaseActions();

//    //        var result = databaseActions.SetupDatabase(contextMock.Object);

//    //        result.Should().NotBeNull();
//    //        result.Should().BeOfType<NpgsqlConnection>();
//    //    }
//    //}
//}
