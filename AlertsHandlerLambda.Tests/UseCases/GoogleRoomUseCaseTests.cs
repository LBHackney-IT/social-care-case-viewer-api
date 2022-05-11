using AlertsHandlerLambda.Gateways;
using AlertsHandlerLambda.UseCases;
using Amazon.Lambda.SNSEvents;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Amazon.Lambda.SNSEvents.SNSEvent;

namespace AlertsHandlerLambda.Tests.UseCases
{
    [TestFixture]
    public class GoogleRoomUseCaseTests
    {
        private Mock<IGoogleAPIGateway> _mockGoogleAPIGateway;
        private GoogleRoomUseCase _googleRoomUseCase;
        private SNSEvent _sNSEvent;
        private readonly string _message = "message";

        [SetUp]
        public void SetUp()
        {
            _mockGoogleAPIGateway = new Mock<IGoogleAPIGateway>();
            _googleRoomUseCase = new GoogleRoomUseCase(_mockGoogleAPIGateway.Object);

            _sNSEvent = new SNSEvent
            {
                Records = new List<SNSRecord>
                {
                    new SNSRecord
                    {
                        Sns = new SNSMessage()
                        {
                            Message = _message
                        }
                    }
                }
            };
        }

        [Test]
        public async Task SendMessageCallsGoogleAPIGateway()
        {
            _mockGoogleAPIGateway.Setup(x => x.PostMessageToGoogleRoom(It.IsAny<string>())).Verifiable();

            await _googleRoomUseCase.SendMessage(_sNSEvent);

            _mockGoogleAPIGateway.Verify(x => x.PostMessageToGoogleRoom(It.IsAny<string>()), Times.Once);
        }

        [Test]
        public async Task SendMessageCallsGoogleAPIGatewayWithCorrectMessage()
        {
            _mockGoogleAPIGateway.Setup(x => x.PostMessageToGoogleRoom(It.IsAny<string>())).Verifiable();

            await _googleRoomUseCase.SendMessage(_sNSEvent);

            _mockGoogleAPIGateway.Verify(x => x.PostMessageToGoogleRoom(_message), Times.Once);
        }

        [Test]
        public async Task SendMessageThrowsArgumentNullExceptionWithCorrectMessageWhenMessageIsNull()
        {
            _sNSEvent.Records.First().Sns.Message = null;

            Func<Task> f = async () => await _googleRoomUseCase.SendMessage(_sNSEvent);

            await f.Should().ThrowAsync<ArgumentNullException>().WithMessage("Value cannot be null.");
        }
    }
}
