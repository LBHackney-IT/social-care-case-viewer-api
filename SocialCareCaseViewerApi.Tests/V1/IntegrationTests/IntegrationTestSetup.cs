using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using Npgsql;
using NUnit.Framework;
using SocialCareCaseViewerApi.V1.Infrastructure;
using System;
using System.IO;
using System.Net.Http;

namespace SocialCareCaseViewerApi.Tests.V1.IntegrationTests
{
    public class IntegrationTestSetup<TStartup> where TStartup : class
    {
        private MockWebApplicationFactory<TStartup> _factory;
        private NpgsqlConnection _connection;
        private NpgsqlConnection _historicalDataDBconnection;

        protected HttpClient Client { get; private set; }
        protected DatabaseContext DatabaseContext { get; private set; }
        protected HistoricalDataContext HistoricalSocialCareContext { get; private set; }
        private MongoClient MongoDbClient { get; set; }

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _connection = new NpgsqlConnection(ConnectionString.TestDatabase());
            _connection.Open();

            var npgsqlCommand = _connection.CreateCommand();
            npgsqlCommand.CommandText = "SET deadlock_timeout TO 30";
            npgsqlCommand.ExecuteNonQuery();

            _historicalDataDBconnection = new NpgsqlConnection(ConnectionString.HistoricalDataTestDatabase());
            _historicalDataDBconnection.Open();

            var historicalDatanpsqlCommand = _historicalDataDBconnection.CreateCommand();
            historicalDatanpsqlCommand.CommandText = "SET deadlock_timeout TO 30";
            historicalDatanpsqlCommand.ExecuteNonQuery();
        }

        [SetUp]
        public void BaseSetup()
        {
            // Set up MongoDB connection string depending on whether the tests are run locally or in docker
            SetUpEnvironmentVariables();

            _factory = new MockWebApplicationFactory<TStartup>(_connection, _historicalDataDBconnection);
            Client = _factory.CreateClient();

            DatabaseContext = _factory.Server.Host.Services.GetRequiredService<DatabaseContext>();
            HistoricalSocialCareContext = _factory.Server.Host.Services.GetRequiredService<HistoricalDataContext>();

            WipeDatabase();
            WipeMongoDatabase();
        }

        [TearDown]
        public void BaseTearDown()
        {
            Client.Dispose();
            _factory.Dispose();
            WipeDatabase();
            WipeMongoDatabase();
        }

        [OneTimeTearDown]
        public void AfterAllTests()
        {
            _connection.Dispose();
            _historicalDataDBconnection.Dispose();
        }

        private void WipeDatabase()
        {
            var filePath = TestContext.CurrentContext.WorkDirectory.Replace(Path.Combine("SocialCareCaseViewerApi.Tests", "bin", "Debug", "netcoreapp3.1"), "");

            DatabaseContext.Database.ExecuteSqlRaw("DROP SCHEMA dbo CASCADE");
            var path = Path.Combine(filePath, "database", "schema.sql");
            var sql = File.ReadAllText(path);
            DatabaseContext.Database.ExecuteSqlRaw(sql);

            HistoricalSocialCareContext.Database.ExecuteSqlRaw("DROP SCHEMA dbo CASCADE");
            var pathToSchemaFile = Path.Combine(filePath, "database", "historical-data-schema.sql");
            var sqlToExecute = File.ReadAllText(pathToSchemaFile);
            HistoricalSocialCareContext.Database.ExecuteSqlRaw(sqlToExecute);
        }

        private void WipeMongoDatabase()
        {
            var mongoConnectionString = Environment.GetEnvironmentVariable("SCCV_MONGO_CONN_STRING");
            var databaseName = Environment.GetEnvironmentVariable("SCCV_MONGO_DB_NAME");

            MongoDbClient = new MongoClient(new MongoUrl(mongoConnectionString));
            MongoDbClient.DropDatabase(databaseName);
        }

        private static void SetUpEnvironmentVariables()
        {
            if (Environment.GetEnvironmentVariable("CONTAINER_ENV") != "DockerTest")
            {
                Environment.SetEnvironmentVariable("SCCV_MONGO_CONN_STRING", "mongodb://localhost:1433/");
            }

            Environment.SetEnvironmentVariable("SCCV_MONGO_DB_NAME", "social_care_db_test");
            Environment.SetEnvironmentVariable("SCCV_MONGO_COLLECTION_NAME", "form_data_test");
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");
            Environment.SetEnvironmentVariable("HISTORICAL_DATA_CONNECTION_STRING", ConnectionString.HistoricalDataTestDatabase());
        }
    }
}
