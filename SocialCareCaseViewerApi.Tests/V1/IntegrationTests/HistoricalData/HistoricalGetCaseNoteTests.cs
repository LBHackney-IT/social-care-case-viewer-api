using FluentAssertions;
using Newtonsoft.Json;
using NUnit.Framework;
using SocialCareCaseViewerApi.V1.Boundary.Response;
using SocialCareCaseViewerApi.V1.Factories;
using System;
using System.Threading.Tasks;

namespace SocialCareCaseViewerApi.Tests.V1.IntegrationTests.HistoricalData
{
    [TestFixture]
    public class HistoricalGetCaseNoteTests : IntegrationTestSetup<Startup>
    {
        [Test]
        public async Task WhenThereIsAMatchingCaseNoteIdReturns200AndCaseNoteInformation()
        {
            var historicalCaseNote = HistoricalE2ETestHelpers.AddCaseNoteWithNoteTypeAndWorkerToDatabase(HistoricalSocialCareContext);
            var uri = new Uri($"api/v1/casenotes/{historicalCaseNote.Id}", UriKind.Relative);

            var response = await Client.GetAsync(uri).ConfigureAwait(true);

            response.StatusCode.Should().Be(200);

            var content = response.Content;
            var stringContent = await content.ReadAsStringAsync().ConfigureAwait(true);
            var convertedResponse = JsonConvert.DeserializeObject<CaseNoteResponse>(stringContent);

            var caseNoteDomain = historicalCaseNote.ToDomain();

            var expectedResponse = ResponseFactory.ToResponse(caseNoteDomain);

            convertedResponse.Should().BeEquivalentTo(expectedResponse);
        }

        [Test]
        public async Task WhenThereIsNotAMatchingCaseNoteIdReturns404()
        {
            const string nonExistentCaseNoteId = "1234";
            var uri = new Uri($"api/v1/casenotes/{nonExistentCaseNoteId}", UriKind.Relative);

            var response = await Client.GetAsync(uri).ConfigureAwait(true);

            response.StatusCode.Should().Be(404);
        }
    }
}
