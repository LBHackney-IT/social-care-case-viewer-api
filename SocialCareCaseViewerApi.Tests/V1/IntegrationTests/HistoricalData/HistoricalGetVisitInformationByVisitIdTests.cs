using FluentAssertions;
using Newtonsoft.Json;
using NUnit.Framework;
using SocialCareCaseViewerApi.V1.Domain;
using SocialCareCaseViewerApi.V1.Factories;
using System;
using System.Threading.Tasks;

namespace SocialCareCaseViewerApi.Tests.V1.IntegrationTests.HistoricalData
{

    [TestFixture]
    public class HistoricalGetVisitInformationByVisitIdTests : IntegrationTestSetup<Startup>
    {
        [Test]
        public async Task WhenThereIsAVisitWithMatchingIdReturns200AndVisitInformation()
        {
            var worker = HistoricalE2ETestHelpers.AddWorkerToDatabase(HistoricalSocialCareContext);
            var visit = HistoricalE2ETestHelpers.AddVisitToDatabase(HistoricalSocialCareContext, worker).ToDomain();

            visit.CreatedByEmail = worker.EmailAddress;
            visit.CreatedByName = $"{worker.FirstNames} {worker.LastNames}";

            var uri = new Uri($"api/v1/visits/{visit.VisitId}", UriKind.Relative);

            var response = Client.GetAsync(uri);

            response.Result.StatusCode.Should().Be(200);

            var content = response.Result.Content;
            var stringContent = await content.ReadAsStringAsync().ConfigureAwait(true);
            var convertedResponse = JsonConvert.DeserializeObject<Visit>(stringContent);

            convertedResponse.Should().BeEquivalentTo(visit,
                options =>
                    {
                        options.Using<DateTime>(ctx => ctx.Subject.Should().BeCloseTo(ctx.Expectation, 1000)).WhenTypeIs<DateTime>();
                        return options;
                    }
                );
        }

        [Test]
        public async Task WhenThereIsNotAMatchingVisitIdReturns404()
        {
            const long nonExistentVisitId = 12345L;
            var uri = new Uri($"api/v1/visits/{nonExistentVisitId}", UriKind.Relative);

            var response = await Client.GetAsync(uri).ConfigureAwait(true);

            response.StatusCode.Should().Be(404);
        }
    }
}
