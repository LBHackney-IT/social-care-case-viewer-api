using System;
using System.Data;
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
        private ISccvDbContext _mongoDbContext;
        private MockWebApplicationFactory<TStartup> _factory;
        private NpgsqlConnection _connection;
        private NpgsqlTransaction _transaction;
        protected HttpClient Client { get; private set; }
        protected DatabaseContext DatabaseContext { get; private set; }

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
            if (Environment.GetEnvironmentVariable("CONTAINER_ENV") != "DockerTest")
            {
                Environment.SetEnvironmentVariable("SCCV_MONGO_CONN_STRING", "mongodb://localhost:1433/");
            }

            Environment.SetEnvironmentVariable("SCCV_MONGO_DB_NAME", "social_care_db_test");
            Environment.SetEnvironmentVariable("SCCV_MONGO_COLLECTION_NAME", "form_data_test");
            Environment.SetEnvironmentVariable("SOCIAL_CARE_PLATFORM_API_URL", "https://mockBase");
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Development");

            _factory = new MockWebApplicationFactory<TStartup>(_connection);
            Client = _factory.CreateClient();

            DatabaseContext = _factory.Server.Host.Services.GetRequiredService<DatabaseContext>();
            _mongoDbContext = new MongoDbTestContext();

            _transaction = _connection.BeginTransaction(IsolationLevel.ReadCommitted);
            DatabaseContext.Database.UseTransaction(_transaction);
        }

        [TearDown]
        public void BaseTearDown()
        {
            Client.Dispose();
            _factory.Dispose();
            _transaction.Rollback();
            _transaction.Dispose();
            _mongoDbContext.getCollection().DeleteMany(Builders<BsonDocument>.Filter.Empty);
        }

        [OneTimeTearDown]
        public void AfterAllTests()
        {
            _connection.Dispose();
        }
    }
}
