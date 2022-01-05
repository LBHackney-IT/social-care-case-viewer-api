using System;

namespace SocialCareCaseViewerApi.Tests
{
    public static class ConnectionString
    {
        public static string TestDatabase()
        {
            return $"Host={Environment.GetEnvironmentVariable("DB_HOST") ?? "127.0.0.1"};" +
                   $"Port={Environment.GetEnvironmentVariable("DB_PORT") ?? "5432"};" +
                   $"Username={Environment.GetEnvironmentVariable("DB_USERNAME") ?? "postgres"};" +
                   $"Password={Environment.GetEnvironmentVariable("DB_PASSWORD") ?? "mypassword"};" +
                   $"Database={Environment.GetEnvironmentVariable("DB_DATABASE") ?? "testdb"}";
        }

        public static string HistoricalDataTestDatabase()
        {
            return $"Host={Environment.GetEnvironmentVariable("DB_HOST_HISTORICAL_DATA") ?? "127.0.0.1"};" +
                   $"Port={Environment.GetEnvironmentVariable("DB_PORT_HISTORICAL_DATA") ?? "5434"};" +
                   $"Username={Environment.GetEnvironmentVariable("DB_USERNAME_HISTORICAL_DATA") ?? "postgres"};" +
                   $"Password={Environment.GetEnvironmentVariable("DB_PASSWORD_HISTORICAL_DATA") ?? "mypassword"};" +
                   $"Database={Environment.GetEnvironmentVariable("DB_DATABASE_HISTORICAL_DATA") ?? "historical-data-testdb"}";
        }
    }
}
