using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Moq.Protected;
using NUnit.Framework;
using SocialCareCaseViewerApi.Tests.V1.Helpers;
using SocialCareCaseViewerApi.V1.Boundary.Response;
using SocialCareCaseViewerApi.V1.Domain;
using SocialCareCaseViewerApi.V1.Exceptions;
using SocialCareCaseViewerApi.V1.Gateways;

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
                                ""createdOn"": ""2019-04-23T11:28:43"",
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
            Assert.AreEqual("2", response.CaseNotes.First().CaseNoteId);
            Assert.AreEqual("My Title", response.CaseNotes.First().CaseNoteTitle);
            Assert.AreEqual("Content", response.CaseNotes.First().CaseNoteContent);
            //Assert.AreEqual("23/04/2019 11:28:43", response.CaseNotes.First().CreatedOn.ToString()); //TODO enable after date parsing fixes
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
                    ""createdOn"": ""2019-04-23T11:28:43"",
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
            Assert.AreEqual("2", response.CaseNoteId);
            Assert.AreEqual("My Title", response.CaseNoteTitle);
            Assert.AreEqual("Content", response.CaseNoteContent);
            //Assert.AreEqual("23/04/2019 11:28:43", response.CreatedOn.ToString()); //TODO enable after date parsing fixes
            Assert.AreEqual("first.last@domain.com", response.CreatedByEmail);
            Assert.AreEqual("last.first@domain.com", response.CreatedByName);
            Assert.AreEqual("My type", response.NoteType);
        }

        [Test]
        public void GivenHttpClientReturnsValidResponseThenGatewayReturnsListVisitsResponse()
        {
            var visit1 = TestHelper.CreateVisitEntity();
            var visit2 = TestHelper.CreateVisitEntity();
            var visits = new ListVisitsResponse {Visits = new List<Visit> {visit1, visit2}};
            var visitsJson = JsonSerializer.Serialize(visits);
            var mockHttpMessageHandler = CreateMockHttpHandler(visitsJson);
            _httpClient = CreateHttpClient(mockHttpMessageHandler);

            _socialCarePlatformAPIGateway = new SocialCarePlatformAPIGateway(_httpClient);

            var response = _socialCarePlatformAPIGateway.GetVisitsByPersonId("1");

            response.Should().NotBeNull();
            response.Visits.Count.Should().Be(visits.Visits.Count);
            response.Should().BeEquivalentTo(visits);
        }

        [Test]
        public void GivenHttpClientReturnsValidResponseThenGatewayReturnsVisitResponse()
        {
            var visit = TestHelper.CreateVisitEntity();
            var visitJson = JsonSerializer.Serialize(visit);
            var mockHttpMessageHandler = CreateMockHttpHandler(visitJson);
            _httpClient = CreateHttpClient(mockHttpMessageHandler);

            _socialCarePlatformAPIGateway = new SocialCarePlatformAPIGateway(_httpClient);

            var response = _socialCarePlatformAPIGateway.GetVisitByVisitId(visit.VisitId);

            response.Should().NotBeNull();
            response.Should().BeEquivalentTo(visit);
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

            Assert.AreEqual("Unable to deserialize ListCaseNotesResponse object", exception.Message);
        }

        [Test]
        public void GivenHttpClientReturnsValidResponseButDeserialisationOfVisitsFailsThenGatewayThrowsSocialCarePlatformApiExceptionWithCorrectMessage()
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

            var exception = Assert.Throws<SocialCarePlatformApiException>(delegate { _socialCarePlatformAPIGateway.GetVisitsByPersonId("1"); });

            Assert.AreEqual("Unable to deserialize ListVisitsResponse object", exception.Message);
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

            _httpClient = new HttpClient(mockHttpMessageHandler.Object)
            {
                BaseAddress = _mockBaseUri
            };

            _socialCarePlatformAPIGateway = new SocialCarePlatformAPIGateway(_httpClient);

            var exception = Assert.Throws<SocialCarePlatformApiException>(delegate { _socialCarePlatformAPIGateway.GetCaseNotesByPersonId("1"); });

            Assert.AreEqual(((int) HttpStatusCode.Unauthorized).ToString(), exception.Message);
        }


        [Test]
        public void GivenHttpClientReturnsBadRequestResponseThenGatewayThrowsSocialCarePlatformApiException()
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
                StatusCode = HttpStatusCode.BadRequest
            }).Verifiable();

            _httpClient = new HttpClient(mockHttpMessageHandler.Object)
            {
                BaseAddress = _mockBaseUri
            };

            _socialCarePlatformAPIGateway = new SocialCarePlatformAPIGateway(_httpClient);

            var exception = Assert.Throws<SocialCarePlatformApiException>(delegate { _socialCarePlatformAPIGateway.GetCaseNotesByPersonId("1"); });

            Assert.AreEqual(((int) HttpStatusCode.BadRequest).ToString(), exception.Message);
        }


        [Test]
        [TestCase(HttpStatusCode.InternalServerError)]
        [TestCase(HttpStatusCode.BadRequest)]
        [TestCase(HttpStatusCode.NotFound)]
        public void GivenHttpClientReturnsNon200ResponseThenGatewayThrowsSocialCarePlatformApiExceptionWithStatusCode(HttpStatusCode code)
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
                StatusCode = code
            }).Verifiable();

            _httpClient = new HttpClient(mockHttpMessageHandler.Object)
            {
                BaseAddress = _mockBaseUri
            };

            _socialCarePlatformAPIGateway = new SocialCarePlatformAPIGateway(_httpClient);

            var exception = Assert.Throws<SocialCarePlatformApiException>(delegate { _socialCarePlatformAPIGateway.GetCaseNotesByPersonId("1"); });

            Assert.AreEqual(((int) code).ToString(), exception.Message);
        }

        private Mock<HttpMessageHandler> CreateMockHttpHandler(string jsonContent)
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
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(jsonContent)

                }).Verifiable();

            _httpClient = new HttpClient(mockHttpMessageHandler.Object)
            {
                BaseAddress = _mockBaseUri
            };

            return mockHttpMessageHandler;
        }

        private HttpClient CreateHttpClient(Mock<HttpMessageHandler> mockHttpMessageHandler)
        {

            return new HttpClient(mockHttpMessageHandler.Object)
            {
                BaseAddress = _mockBaseUri
            };
        }
    }
}
