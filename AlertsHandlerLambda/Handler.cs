using AlertsHandlerLambda.UseCases;
using Amazon.Lambda.Core;
using Amazon.Lambda.SNSEvents;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]
namespace AlertsHandlerLambda
{
    public class Handler
    {
        private readonly ServiceProvider _serviceProvider;

        public Handler()
        {
            var serviceCollection = new ServiceCollection();
            ServiceConfigurator.ConfigureServices(serviceCollection);

            _serviceProvider = serviceCollection.BuildServiceProvider();
        }

        public Handler(Action<ServiceCollection> configureTestServices)
        {
            var serviceCollection = new ServiceCollection();
            ServiceConfigurator.ConfigureServices(serviceCollection);
            configureTestServices?.Invoke(serviceCollection);

            _serviceProvider = serviceCollection.BuildServiceProvider();
        }

        public async Task<string> FunctionHandler(SNSEvent snsEvent, ILambdaContext context)
        {
            var googleRoomUseCase = _serviceProvider.GetService<IGoogleRoomUseCase>();

            if (googleRoomUseCase == null)
            {
                throw new ConfigurationException(null, null, "IGoogleRoomUseCase not configured");
            }

            return await googleRoomUseCase.SendMessage(snsEvent);
        }
    }
}
