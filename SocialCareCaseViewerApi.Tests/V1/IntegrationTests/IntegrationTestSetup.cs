using System;
using System.IO;
using System.Net.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Bson;
using MongoDB.Driver;
using Npgsql;
using NUnit.Framework;
using SocialCareCaseViewerApi.V1.Infrastructure;

namespace SocialCareCaseViewerApi.Tests.V1.IntegrationTests
{
    public class IntegrationTestSetup<TStartup> where TStartup : class
    {
        private MockWebApplicationFactory<TStartup> _factory;
        private NpgsqlConnection _connection;
        protected HttpClient Client { get; private set; }
        protected DatabaseContext DatabaseContext { get; private set; }
        private MongoClient MongoDbClient { get; set; }

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _connection = new NpgsqlConnection(ConnectionString.TestDatabase());
            _connection.Open();

            var npgsqlCommand = _connection.CreateCommand();
            npgsqlCommand.CommandText = "SET deadlock_timeout TO 30";
            npgsqlCommand.ExecuteNonQuery();
        }

        [SetUp]
        public void BaseSetup()
        {
            // Set up MongoDB connection string depending on whether the tests are run locally or in docker
            SetUpEnvironmentVariables();

            _factory = new MockWebApplicationFactory<TStartup>(_connection);
            Client = _factory.CreateClient();

            DatabaseContext = _factory.Server.Host.Services.GetRequiredService<DatabaseContext>();
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
        }

        private void WipeDatabase()
        {
            DatabaseContext.Database.ExecuteSqlRaw("DROP SCHEMA dbo CASCADE");
            var filePath = TestContext.CurrentContext.WorkDirectory.Replace(Path.Combine("SocialCareCaseViewerApi.Tests", "bin", "Debug", "netcoreapp3.1"), "");
            var path = Path.Combine(filePath, "database", "schema.sql");
            var sql = File.ReadAllText(path);
            DatabaseContext.Database.ExecuteSqlRaw(sql);
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
            Environment.SetEnvironmentVariable("SOCIAL_CARE_PLATFORM_API_URL", "https://mockBase");
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");
            Environment.SetEnvironmentVariable("HISTORICAL_DATA_CONNECTION_STRING", ConnectionString.HistoricalDataTestDatabase());
        }
    }
}
