using AlertsHandlerLambda.Gateways;
using FluentAssertions;
using Moq;
using Moq.Protected;
using NUnit.Framework;
using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace AlertsHandlerLambda.Tests.Gateways
{
    [TestFixture]
    public class GoogleAPIGatewayTests
    {
        private GoogleAPIGateway _googleAPIGateway;
        private readonly Uri _mockBaseUri = new Uri("http://mockBase");

        [SetUp]
        public void SetUp()
        {
            Environment.SetEnvironmentVariable("GOOGLE_CHAT_ROOM_PATH", "/mockUrl");
        }

        [Test]
        [TestCase(HttpStatusCode.InternalServerError)]
        [TestCase(HttpStatusCode.BadRequest)]
        [TestCase(HttpStatusCode.NotFound)]
        [TestCase(HttpStatusCode.Unauthorized)]
        public async Task GivenHttpClientReturnsNon200ResponseThenGatewayThrowsGoogleApiExceptionWithStatusCode(HttpStatusCode code)
        {
            var httpClient = CreateHttpClient(code);
            _googleAPIGateway = new GoogleAPIGateway(httpClient);

            Func<Task> f = async () => await _googleAPIGateway.PostMessageToGoogleRoom("message");

            await f.Should().ThrowAsync<GoogleApiException>().WithMessage(((int) code).ToString());
        }

        [Test]
        public async Task GivenHttpClientReturns200ResponseThenGatewayReturnsConfirmationMessage()
        {
            var httpClient = CreateHttpClient();
            _googleAPIGateway = new GoogleAPIGateway(httpClient);

            var result = await _googleAPIGateway.PostMessageToGoogleRoom("message");

            result.Should().Be("Message sent successfully");
        }

        private HttpClient CreateHttpClient(HttpStatusCode httpStatusCode = HttpStatusCode.OK)
        {
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>(MockBehavior.Strict);

            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = httpStatusCode
                }).Verifiable();

            return new HttpClient(mockHttpMessageHandler.Object)
            {
                BaseAddress = _mockBaseUri
            };
        }
    }
}
