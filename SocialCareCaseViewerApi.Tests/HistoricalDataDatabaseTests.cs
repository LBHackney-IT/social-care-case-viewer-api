using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using NUnit.Framework;
using SocialCareCaseViewerApi.V1.Infrastructure;

namespace SocialCareCaseViewerApi.Tests
{
    [TestFixture]
    public class HistoricalDataDatabaseTests
    {
        private IDbContextTransaction _transaction;
        protected HistoricalDataContext HistoricalSocialCareContext { get; private set; }

        [SetUp]
        public void RunBeforeAnyTests()
        {
            var builder = new DbContextOptionsBuilder<HistoricalDataContext>();
            builder.UseNpgsql(ConnectionString.HistoricalDataTestDatabase());
            HistoricalSocialCareContext = new HistoricalDataContext(builder.Options);

            //context is configured to be read only on production
            HistoricalSocialCareContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;

            HistoricalSocialCareContext.Database.EnsureCreated();
            _transaction = HistoricalSocialCareContext.Database.BeginTransaction();
        }

        [TearDown]
        public void RunAfterAnyTests()
        {
            _transaction.Rollback();
            _transaction.Dispose();
        }
    }
}
