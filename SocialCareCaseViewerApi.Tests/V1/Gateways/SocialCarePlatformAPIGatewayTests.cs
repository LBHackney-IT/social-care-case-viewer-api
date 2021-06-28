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
        private readonly Uri _mockBaseUri = new Uri("https://mockBase");
        private HttpClient _httpClient;

        [Test]
        public void GivenHttpClientReturnsValidResponseThenGatewayReturnsListCaseNotesResponse()
        {
            var caseNote1 = TestHelpers.CreateCaseNote();
            var caseNote2 = TestHelpers.CreateCaseNote();
            var caseNotes = new ListCaseNotesResponse { CaseNotes = new List<CaseNote> { caseNote1, caseNote2 } };
            var httpClient = CreateHttpClient(caseNotes);
            _socialCarePlatformAPIGateway = new SocialCarePlatformAPIGateway(httpClient);

            var response = _socialCarePlatformAPIGateway.GetCaseNotesByPersonId("1");

            response.Should().BeEquivalentTo(caseNotes);
        }

        [Test]
        public void GivenHttpClientReturnsValidResponseThenGatewayReturnsCaseNoteResponse()
        {
            var caseNote = TestHelpers.CreateCaseNote();
            var httpClient = CreateHttpClient(caseNote);
            _socialCarePlatformAPIGateway = new SocialCarePlatformAPIGateway(httpClient);

            var response = _socialCarePlatformAPIGateway.GetCaseNoteById("1");

            response.Should().BeEquivalentTo(caseNote);
        }

        [Test]
        public void GivenHttpClientReturnsValidResponseThenGatewayReturnsListVisitsResponse()
        {
            var visit1 = TestHelpers.CreateVisit();
            var visit2 = TestHelpers.CreateVisit();
            var visits = new List<Visit> { visit1, visit2 };
            var httpClient = CreateHttpClient(visits);

            _socialCarePlatformAPIGateway = new SocialCarePlatformAPIGateway(httpClient);

            var response = _socialCarePlatformAPIGateway.GetVisitsByPersonId("1").ToList();

            response.Count.Should().Be(visits.Count);
            response.Should().BeEquivalentTo(visits);
        }

        [Test]
        public void GivenHttpClientReturns404ThenGatewayReturnsNoVisits()
        {
            const int fakePersonId = 123;
            var httpClient = CreateHttpClient(HttpStatusCode.NotFound);

            _socialCarePlatformAPIGateway = new SocialCarePlatformAPIGateway(httpClient);

            var exception = Assert.Throws<SocialCarePlatformApiException>(delegate { _socialCarePlatformAPIGateway.GetVisitsByPersonId(fakePersonId.ToString()); });

            exception.Message.Should().Be("404");
        }

        [Test]
        public void GivenHttpClientReturnsValidResponseThenGatewayReturnsVisitResponse()
        {
            var visit = TestHelpers.CreateVisit();
            var httpClient = CreateHttpClient(visit);
            _socialCarePlatformAPIGateway = new SocialCarePlatformAPIGateway(httpClient);

            var response = _socialCarePlatformAPIGateway.GetVisitByVisitId(visit.VisitId);

            response.Should().BeEquivalentTo(visit);
        }

        [Test]
        public void GivenHttpClientReturns404ThenGatewayReturnsNoVisit()
        {
            const int fakeVisitId = 123;
            var httpClient = CreateHttpClient(HttpStatusCode.NotFound);

            _socialCarePlatformAPIGateway = new SocialCarePlatformAPIGateway(httpClient);

            var exception = Assert.Throws<SocialCarePlatformApiException>(delegate { _socialCarePlatformAPIGateway.GetVisitByVisitId(fakeVisitId); });

            exception.Message.Should().Be("404");
        }

        [Test]
        public void GivenHttpClientReturnsValidResponseButDeserialisationFailsThenGatewayThrowsSocialCarePlatformApiExceptionWithCorrectMessage()
        {
            const string invalidJson = "(^(^^*(^*__INVALID_JSON__(^*^(^*((*";
            var httpClient = CreateHttpClient(invalidJson);
            _socialCarePlatformAPIGateway = new SocialCarePlatformAPIGateway(httpClient);

            var exception = Assert.Throws<SocialCarePlatformApiException>(delegate { _socialCarePlatformAPIGateway.GetCaseNotesByPersonId("1"); });

            exception.Message.Should().Be("Unable to deserialize ListCaseNotesResponse object");
        }

        [Test]
        public void GivenHttpClientReturnsValidResponseButDeserialisationOfVisitsFailsThenGatewayThrowsSocialCarePlatformApiExceptionWithCorrectMessage()
        {
            const string invalidJson = "(^(^^*(^*__INVALID_JSON__(^*^(^*((*";
            var httpClient = CreateHttpClient(invalidJson);
            _socialCarePlatformAPIGateway = new SocialCarePlatformAPIGateway(httpClient);

            var exception = Assert.Throws<SocialCarePlatformApiException>(delegate { _socialCarePlatformAPIGateway.GetVisitsByPersonId("1"); });

            exception.Message.Should().Be("Unable to deserialize List`1 object");
        }

        [Test]
        public void GivenHttpClientReturnsUnauthorisedResponseThenGatewayThrowsSocialCarePlatformApiException()
        {
            var httpClient = CreateHttpClient(HttpStatusCode.Unauthorized);
            _socialCarePlatformAPIGateway = new SocialCarePlatformAPIGateway(httpClient);

            var exception = Assert.Throws<SocialCarePlatformApiException>(delegate { _socialCarePlatformAPIGateway.GetCaseNotesByPersonId("1"); });

            exception.Message.Should().Be(((int) HttpStatusCode.Unauthorized).ToString());
        }

        [Test]
        public void GivenHttpClientReturnsBadRequestResponseThenGatewayThrowsSocialCarePlatformApiException()
        {
            var httpClient = CreateHttpClient(HttpStatusCode.BadRequest);
            _socialCarePlatformAPIGateway = new SocialCarePlatformAPIGateway(httpClient);

            var exception = Assert.Throws<SocialCarePlatformApiException>(delegate { _socialCarePlatformAPIGateway.GetCaseNotesByPersonId("1"); });

            exception.Message.Should().Be(((int) HttpStatusCode.BadRequest).ToString());
        }


        [Test]
        [TestCase(HttpStatusCode.InternalServerError)]
        [TestCase(HttpStatusCode.BadRequest)]
        [TestCase(HttpStatusCode.NotFound)]
        public void GivenHttpClientReturnsNon200ResponseThenGatewayThrowsSocialCarePlatformApiExceptionWithStatusCode(HttpStatusCode code)
        {
            var httpClient = CreateHttpClient(code);
            _socialCarePlatformAPIGateway = new SocialCarePlatformAPIGateway(httpClient);

            var exception = Assert.Throws<SocialCarePlatformApiException>(delegate { _socialCarePlatformAPIGateway.GetCaseNotesByPersonId("1"); });

            exception.Message.Should().Be(((int) code).ToString());
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

            _httpClient = new HttpClient(mockHttpMessageHandler.Object)
            {
                BaseAddress = _mockBaseUri
            };

            return _httpClient;
        }

        private HttpClient CreateHttpClient<T>(T content)
        {
            var jsonContent = JsonSerializer.Serialize(content);
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

            return _httpClient;
        }
    }
}
