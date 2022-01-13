using FluentAssertions;
using NUnit.Framework;
using System;
using System.Threading.Tasks;

namespace SocialCareCaseViewerApi.Tests.V1.IntegrationTests
{
    [TestFixture]
    public class DatabaseConnectionsHealthCheckTests : IntegrationTestSetup<Startup>
    {
        [Test]
        public async Task ReturnsOKWhenConnectionToHistoricalDataDatabaseIsSuccessful()
        {
            var connectionString = $"Host=127.0.0.1;Port=5434;Username=postgres;Password=mypassword;Database=testdb";

            Environment.SetEnvironmentVariable("HISTORICAL_DATA_CONNECTION_STRING", connectionString);

            var uri = new Uri("/api/v1/healthcheck/historical-data-database", UriKind.Relative);

            var response = await Client.GetAsync(uri).ConfigureAwait(true);

            response.StatusCode.Should().Be(200);
        }

        [Test]
        public async Task Returns500WithCorrectMessageWhenConnectionToHistoricalDataDatabaseIsNotSuccessful()
        {
            var connectionString = $"Host=0.0.0.0;Port=0;Username=username;Password=pw;Database=db";

            Environment.SetEnvironmentVariable("HISTORICAL_DATA_CONNECTION_STRING", connectionString);

            var uri = new Uri("/api/v1/healthcheck/historical-data-database", UriKind.Relative);

            var response = await Client.GetAsync(uri).ConfigureAwait(true);

            response.StatusCode.Should().Be(500);

            response.Content.ReadAsStringAsync().Result.Should().Contain("Unable to connect to historical data database");
        }
    }
}
