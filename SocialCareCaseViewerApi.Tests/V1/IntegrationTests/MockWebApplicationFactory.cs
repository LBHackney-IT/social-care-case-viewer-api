using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SocialCareCaseViewerApi.V1.Infrastructure;

namespace SocialCareCaseViewerApi.Tests.V1.IntegrationTests
{
    public class MockWebApplicationFactory<TStartup>
        : WebApplicationFactory<TStartup> where TStartup : class
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                services.AddDbContext<DatabaseContext>(
                    opt => opt.UseNpgsql(ConnectionString.TestDatabase()));

                services.AddDbContext<HistoricalDataContext>(
                   opt => opt.UseNpgsql(ConnectionString.HistoricalDataTestDatabase()));
            });
        }
    }
}
