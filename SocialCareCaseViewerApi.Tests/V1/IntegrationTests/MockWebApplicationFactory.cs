using System.Data.Common;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SocialCareCaseViewerApi.V1.Infrastructure;

namespace SocialCareCaseViewerApi.Tests.V1.IntegrationTests
{
    public class MockWebApplicationFactory<TStartup>
        : WebApplicationFactory<TStartup> where TStartup : class
    {
        private readonly DbConnection _connection;
        private readonly DbConnection _historicalDataDbConnection;

        public MockWebApplicationFactory(DbConnection connection, DbConnection historicalDataDbConnection)
        {
            _connection = connection;
            _historicalDataDbConnection = historicalDataDbConnection;
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureAppConfiguration(b => b.AddEnvironmentVariables());
            builder.ConfigureServices(services =>
            {
                var dbBuilder = new DbContextOptionsBuilder();
                dbBuilder.UseNpgsql(_connection);
                var context = new DatabaseContext(dbBuilder.Options);
                services.AddSingleton(context);

                var historicalDataDbBuilder = new DbContextOptionsBuilder();
                historicalDataDbBuilder.UseNpgsql(_historicalDataDbConnection);
                var historicalDataContext = new HistoricalSocialCareContext(historicalDataDbBuilder.Options);
                services.AddSingleton(historicalDataContext);

                var serviceProvider = services.BuildServiceProvider();
                var dbContext = serviceProvider.GetRequiredService<DatabaseContext>();
                var historicalDbContext = serviceProvider.GetRequiredService<HistoricalSocialCareContext>();

                dbContext.Database.EnsureCreated();
                historicalDataContext.Database.EnsureCreated();
            });
        }
    }
}
