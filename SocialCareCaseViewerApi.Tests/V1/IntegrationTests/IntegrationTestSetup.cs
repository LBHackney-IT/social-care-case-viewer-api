using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
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

        protected HttpClient Client { get; private set; }
        protected DatabaseContext DatabaseContext { get; private set; }
        protected HistoricalDataContext HistoricalSocialCareContext { get; private set; }
        private MongoClient MongoDbClient { get; set; }

        [SetUp]
        public void BaseSetup()
        {
            // Set up MongoDB connection string depending on whether the tests are run locally or in docker
            SetUpEnvironmentVariables();

            _factory = new MockWebApplicationFactory<TStartup>();
            Client = _factory.CreateClient();

            var scope = _factory.Services.GetRequiredService<IServiceScopeFactory>().CreateScope();

            DatabaseContext = scope.ServiceProvider.GetRequiredService<DatabaseContext>();
            DatabaseContext.Database.EnsureCreated();

            HistoricalSocialCareContext = scope.ServiceProvider.GetRequiredService<HistoricalDataContext>();
            HistoricalSocialCareContext.Database.EnsureCreated();

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
