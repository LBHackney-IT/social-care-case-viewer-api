using Moq;
using Moq.Protected;
using NUnit.Framework;
using SocialCareCaseViewerApi.V1.Exceptions;
using SocialCareCaseViewerApi.V1.Gateways;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace SocialCareCaseViewerApi.Tests.V1.Gateways
{
    [TestFixture]
    public class SocialCarePlatformAPIGatewayTests
    {
        private SocialCarePlatformAPIGateway _socialCarePlatformAPIGateway;
        private readonly Uri _mockBaseUri = new Uri("http://mockBase");
        private HttpClient _httpClient;

        [Test]
        public void GivenHttpClientReturnsValidResponseThenGatewayReturnsListCaseNotesResponse()
        {
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>(MockBehavior.Strict);

            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
            .ReturnsAsync(new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(@"
                {
                    ""caseNotes"": [
                            {
                                ""mosaicId"": ""1"",
                                ""caseNoteId"": ""2"",
                                ""caseNoteTitle"": ""My Title"",
                                ""caseNoteContent"": ""Content"",
                                ""createdOn"": ""10/01/2001 11:11:11"",
                                ""createdByEmail"": ""first.last@domain.com"",
                                ""createdByName"": ""last.first@domain.com"",
                                ""noteType"": ""My type""
                            }
	                    ]
                }")

            }).Verifiable();

            _httpClient = new HttpClient(mockHttpMessageHandler.Object)
            {
                BaseAddress = _mockBaseUri
            };

            _socialCarePlatformAPIGateway = new SocialCarePlatformAPIGateway(_httpClient);

            var response = _socialCarePlatformAPIGateway.GetCaseNotesByPersonId("1");

            Assert.IsNotNull(response);
            Assert.AreEqual(1, response.CaseNotes.Count);
            Assert.AreEqual("1", response.CaseNotes.First().MosaicId);
            Assert.AreEqual(2, response.CaseNotes.First().CaseNoteId);
            Assert.AreEqual("My Title", response.CaseNotes.First().CaseNoteTitle);
            Assert.AreEqual("Content", response.CaseNotes.First().CaseNoteContent);
            Assert.AreEqual("10/01/2001 11:11:11", response.CaseNotes.First().CreatedOn);
            Assert.AreEqual("first.last@domain.com", response.CaseNotes.First().CreatedByEmail);
            Assert.AreEqual("last.first@domain.com", response.CaseNotes.First().CreatedByName);
            Assert.AreEqual("My type", response.CaseNotes.First().NoteType);
        }

        [Test]
        public void GivenHttpClientReturnsValidResponseThenGatewayReturnsCaseNoteResponse()
        {
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>(MockBehavior.Strict);

            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
            .ReturnsAsync(new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(@"
                {
                    ""mosaicId"": ""1"",
                    ""caseNoteId"": ""2"",
                    ""caseNoteTitle"": ""My Title"",
                    ""caseNoteContent"": ""Content"",
                    ""createdOn"": ""10/01/2001 11:11:11"",
                    ""createdByEmail"": ""first.last@domain.com"",
                    ""createdByName"": ""last.first@domain.com"",
                    ""noteType"": ""My type""
                }")
            }).Verifiable();

            _httpClient = new HttpClient(mockHttpMessageHandler.Object)
            {
                BaseAddress = _mockBaseUri
            };

            _socialCarePlatformAPIGateway = new SocialCarePlatformAPIGateway(_httpClient);

            var response = _socialCarePlatformAPIGateway.GetCaseNoteById("1");

            Assert.IsNotNull(response);
            Assert.AreEqual("1", response.MosaicId);
            Assert.AreEqual(2, response.CaseNoteId);
            Assert.AreEqual("My Title", response.CaseNoteTitle);
            Assert.AreEqual("Content", response.CaseNoteContent);
            Assert.AreEqual("10/01/2001 11:11:11", response.CreatedOn);
            Assert.AreEqual("first.last@domain.com", response.CreatedByEmail);
            Assert.AreEqual("last.first@domain.com", response.CreatedByName);
            Assert.AreEqual("My type", response.NoteType);
        }

        [Test]
        public void GivenHttpClientReturnsValidResponseButDeserialisationFailsThenGatewayThrowsSocialCarePlatformApiExceptionWithCorrectMessage()
        {
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>(MockBehavior.Strict);

            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
            .ReturnsAsync(new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(@"(^(^^*(^*__INVALID_JSON__(^*^(^*((*")
            }).Verifiable();

            _httpClient = new HttpClient(mockHttpMessageHandler.Object)
            {
                BaseAddress = _mockBaseUri
            };

            _socialCarePlatformAPIGateway = new SocialCarePlatformAPIGateway(_httpClient);

            var exception = Assert.Throws<SocialCarePlatformApiException>(delegate { _socialCarePlatformAPIGateway.GetCaseNotesByPersonId("1"); });

            Assert.AreEqual("Unable to deserialize object", exception.Message);
        }

        [Test]
        public void GivenHttpClientReturnsUnauthorisedResponseThenGatewayThrowsSocialCarePlatformApiException()
        {
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>(MockBehavior.Strict);

            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
            .ReturnsAsync(new HttpResponseMessage()
            {
                StatusCode = HttpStatusCode.Unauthorized
            }).Verifiable();

            _httpClient= new HttpClient(mockHttpMessageHandler.Object)
            {
                BaseAddress = _mockBaseUri
            };

            _socialCarePlatformAPIGateway = new SocialCarePlatformAPIGateway(_httpClient);

            var exception = Assert.Throws<SocialCarePlatformApiException>(delegate { _socialCarePlatformAPIGateway.GetCaseNotesByPersonId("1"); });

            Assert.AreEqual("Unauthorized", exception.Message);
        }
    }
}
