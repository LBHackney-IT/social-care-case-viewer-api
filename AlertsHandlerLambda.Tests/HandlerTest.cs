using AlertsHandlerLambda.Gateways;
using AlertsHandlerLambda.UseCases;
using Amazon.Lambda.SNSEvents;
using Amazon.Lambda.TestUtilities;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Amazon.Lambda.SNSEvents.SNSEvent;

namespace AlertsHandlerLambda.Tests
{
    [TestFixture]
    public class HandlerTest
    {
        private Mock<IGoogleRoomUseCase> _mockGoogleRoomUseCase;
        private Mock<IGoogleAPIGateway> _mockGoogleAPIGateway;

        private Handler _handler;

        private static readonly SNSEvent _sNSEvent = new SNSEvent
        {
            Records = new List<SNSRecord>
                {
                    new SNSRecord
                    {
                        Sns = new SNSMessage()
                        {
                            Message = "foobar"
                        }
                    }
                }
        };

        private readonly TestLambdaContext _context = new TestLambdaContext();

        [SetUp]
        public void SetUp()
        {
            _mockGoogleRoomUseCase = new Mock<IGoogleRoomUseCase>();
            _mockGoogleAPIGateway = new Mock<IGoogleAPIGateway>();

            Environment.SetEnvironmentVariable("GOOGLE_API_URL", "http://googleapiurl");
            Environment.SetEnvironmentVariable("GOOGLE_CHAT_ROOM_PATH", "/spaces");
        }

        [Test]
        public async Task HandlerCallsGoogleRoomUseCase()
        {
            _handler = new Handler(services =>
            {
                services.Replace(ServiceDescriptor.Scoped(x => _mockGoogleAPIGateway.Object));
                services.Replace(ServiceDescriptor.Scoped(x => _mockGoogleRoomUseCase.Object));
            });

            _mockGoogleAPIGateway.Setup(x => x.PostMessageToGoogleRoom(It.IsAny<string>())).ReturnsAsync("test");
            _mockGoogleRoomUseCase.Setup(x => x.SendMessage(It.IsAny<SNSEvent>())).ReturnsAsync("test");

            var r = await _handler.FunctionHandler(_sNSEvent, _context);

            _mockGoogleRoomUseCase.Verify(x => x.SendMessage(It.IsAny<SNSEvent>()), Times.Once);
        }

        [Test]
        public async Task HandleThrowsConfigurationExceptionWithCorrectMessageWhenIGoogleRoomUseCaseServiceIsNotConfigured()
        {
            _handler = new Handler(services =>
            {
                services.Remove(services.FirstOrDefault(descriptor => descriptor.ServiceType == typeof(IGoogleRoomUseCase)));
            });

            Func<Task> f = async () => await _handler.FunctionHandler(_sNSEvent, _context);

            await f.Should().ThrowAsync<ConfigurationException>().WithMessage("IGoogleRoomUseCase not configured");
        }
    }
}
