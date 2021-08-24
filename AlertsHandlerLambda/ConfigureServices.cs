using AlertsHandlerLambda.Gateways;
using AlertsHandlerLambda.UseCases;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace AlertsHandlerLambda
{
    public static class ServiceConfigurator
    {
        public static void ConfigureServices(this IServiceCollection services)
        {
            services.AddScoped<IGoogleAPIGateway, GoogleAPIGateway>();
            services.AddScoped<IGoogleRoomUseCase, GoogleRoomUseCase>();

            services.AddHttpClient<IGoogleAPIGateway, GoogleAPIGateway>(client =>
            {
                client.BaseAddress = new Uri(Environment.GetEnvironmentVariable("GOOGLE_API_URL")) ?? throw new ConfigurationException("GOOGLE_API_URL not set");
            });
        }
    }
}
