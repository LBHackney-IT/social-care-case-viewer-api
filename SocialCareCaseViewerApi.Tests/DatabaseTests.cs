using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using MongoDB.Bson;
using MongoDB.Driver;
using NUnit.Framework;
using SocialCareCaseViewerApi.V1.Infrastructure;

namespace SocialCareCaseViewerApi.Tests
{
    [TestFixture]
    public class DatabaseTests
    {
        private IDbContextTransaction _transaction;
        protected DatabaseContext DatabaseContext { get; private set; }
        protected ISccvDbContext MongoDbTestContext { get; private set; }

        [SetUp]
        public void RunBeforeAnyTests()
        {
            var builder = new DbContextOptionsBuilder<DatabaseContext>();
            builder.UseNpgsql(ConnectionString.TestDatabase());
            DatabaseContext = new DatabaseContext(builder.Options);

            DatabaseContext.Database.EnsureCreated();

            DatabaseContext.Database.ExecuteSqlRaw("CREATE EXTENSION IF NOT EXISTS pg_trgm;");
            DatabaseContext.Database.ExecuteSqlRaw("CREATE EXTENSION IF NOT EXISTS btree_gin;");

            _transaction = DatabaseContext.Database.BeginTransaction();

            MongoDbTestContext = new MongoDbTestContext();
        }

        [TearDown]
        public void RunAfterAnyTests()
        {
            _transaction.Rollback();
            _transaction.Dispose();

            //remove any documents inserted during the test
            MongoDbTestContext.getCollection().DeleteMany(Builders<BsonDocument>.Filter.Empty);
        }
    }
}
