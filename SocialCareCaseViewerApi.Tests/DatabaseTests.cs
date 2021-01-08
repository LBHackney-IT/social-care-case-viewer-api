using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using SocialCareCaseViewerApi.V1.Infrastructure;
using NUnit.Framework;

namespace SocialCareCaseViewerApi.Tests
{
    [NonParallelizable]
    [TestFixture]
    public class DatabaseTests
    {
        private DatabaseContext _context;
        private IDbContextTransaction _transaction;
        private DbContextOptionsBuilder _builder;

        protected DatabaseContext DatabaseContext => _context;

        [OneTimeSetUp]
        public void RunBeforeAnyTests()
        {
            _builder = new DbContextOptionsBuilder();
            _builder.UseNpgsql(ConnectionString.TestDatabase());
        }

        [SetUp]
        public void SetUp()
        {
            _context = new DatabaseContext(_builder.Options);
            _context.Database.EnsureCreated();
            _transaction = DatabaseContext.Database.BeginTransaction();
        }

        [TearDown]
        public void TearDown()
        {
            _transaction.Rollback();
            _transaction.Dispose();
        }
    }
}
