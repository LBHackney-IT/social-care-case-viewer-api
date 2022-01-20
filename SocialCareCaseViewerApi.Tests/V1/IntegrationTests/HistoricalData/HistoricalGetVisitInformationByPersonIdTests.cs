using FluentAssertions;
using Newtonsoft.Json;
using NUnit.Framework;
using SocialCareCaseViewerApi.V1.Domain;
using SocialCareCaseViewerApi.V1.Factories;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace SocialCareCaseViewerApi.Tests.V1.IntegrationTests.HistoricalData
{
    [TestFixture]
    public class HistoricalGetVisitInformationByPersonIdTests : IntegrationTestSetup<Startup>
    {
        [Test]
        public async Task WhenThereIsAVisitWithMatchingPersonIdReturns200AndVisitInformation()
        {
            var worker = HistoricalE2ETestHelpers.AddWorkerToDatabase(HistoricalSocialCareContext);
            var visitInformation = HistoricalE2ETestHelpers.AddVisitToDatabase(HistoricalSocialCareContext, worker).ToDomain();

            visitInformation.CreatedByEmail = worker.EmailAddress;
            visitInformation.CreatedByName = $"{worker.FirstNames} {worker.LastNames}";
            var uri = new Uri($"api/v1/visits/person/{visitInformation.PersonId}", UriKind.Relative);

            var response = Client.GetAsync(uri);

            response.Result.StatusCode.Should().Be(200);

            var content = response.Result.Content;
            var stringContent = await content.ReadAsStringAsync().ConfigureAwait(true);
            var convertedResponse = JsonConvert.DeserializeObject<List<Visit>>(stringContent);

            convertedResponse.Should().BeEquivalentTo(new List<Visit> { visitInformation });
        }

        [Test]
        public async Task WhenThereAreNotAnyMatchingVisitsForPersonIdReturnsOK()
        {
            const long nonExistentPersonId = 1234L;
            var uri = new Uri($"api/v1/visits/person/{nonExistentPersonId}", UriKind.Relative);

            var response = await Client.GetAsync(uri).ConfigureAwait(true);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }
    }
}
