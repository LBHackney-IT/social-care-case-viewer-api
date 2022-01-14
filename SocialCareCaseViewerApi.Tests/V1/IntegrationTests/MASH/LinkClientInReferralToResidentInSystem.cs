using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Newtonsoft.Json;
using NUnit.Framework;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Boundary.Response;
using SocialCareCaseViewerApi.V1.Infrastructure;
using JsonSerializer = System.Text.Json.JsonSerializer;
using MashReferral = SocialCareCaseViewerApi.V1.Infrastructure.MashReferral;

namespace SocialCareCaseViewerApi.Tests.V1.IntegrationTests.MASH
{
    [TestFixture]
    public class LinkClientInReferralToResidentInSystem : IntegrationTestSetup<Startup>
    {
        private MashReferral _mashReferral;
        private MashResident _unlinkedResident;
        private Person _existingDbPerson;

        [SetUp]
        public void Setup()
        {
            _existingDbPerson = IntegrationTestHelpers.CreateExistingPerson(DatabaseContext);
            _mashReferral = IntegrationTestHelpers.SaveMashReferralToDatabase(DatabaseContext, "CONTACT");

            _unlinkedResident = IntegrationTestHelpers.CreateMashResident(DatabaseContext, _mashReferral, _existingDbPerson);
        }

        [Test]
        public async Task SuccessfulLinkingMatchesMashResidentToSavedPersonInDb()
        {
            _unlinkedResident.SocialCareId = null;
            DatabaseContext.SaveChanges();

            var request = new UpdateMashResidentRequest { SocialCareId = _existingDbPerson.Id };
            var patchUri = new Uri($"/api/v1/mash-resident/{_unlinkedResident.Id}", UriKind.Relative);
            var serializedRequest = JsonSerializer.Serialize(request);
            var requestContent = new StringContent(serializedRequest, Encoding.UTF8, "application/json");

            var linkMashResidentResponse = await Client.PatchAsync(patchUri, requestContent).ConfigureAwait(true);

            linkMashResidentResponse.StatusCode.Should().Be(200);

            _unlinkedResident.SocialCareId.Should().Be(_existingDbPerson.Id);

            var content = await linkMashResidentResponse.Content.ReadAsStringAsync().ConfigureAwait(true);
            var patchMashResidentResponse = JsonConvert.DeserializeObject<MashResidentResponse>(content);

            patchMashResidentResponse.SocialCareId.Should().Be(_existingDbPerson.Id);
        }

        // [Test]
        // public async Task WhenTheRequestIsInvalidReturnsBadRequestStatusCode()
        // {
        //     const string request = "invalid request";
        //     var patchUri = new Uri($"/api/v1/mash-resident/{_unlinkedResident.Id}", UriKind.Relative);
        //     var serializedRequest = JsonSerializer.Serialize(request);
        //     var requestContent = new StringContent(serializedRequest, Encoding.UTF8, "application/json");
        //
        //     var response = await Client.PatchAsync(patchUri, requestContent).ConfigureAwait(true);
        //
        //     response.StatusCode.Should().Be(400);
        // }
        //
        // [Test]
        // public async Task WhenTheRequestContainsAnIncorrectPersonIdReturnsRelevantErrorMessage()
        // {
        //     var incorrectPersonId = _existingDbPerson.Id + 20;
        //     var request = new UpdateMashResidentRequest { SocialCareId = incorrectPersonId };
        //     var patchUri = new Uri($"/api/v1/mash-resident/{_unlinkedResident.Id}", UriKind.Relative);
        //     var serializedRequest = JsonSerializer.Serialize(request);
        //     var requestContent = new StringContent(serializedRequest, Encoding.UTF8, "application/json");
        //
        //     var response = await Client.PatchAsync(patchUri, requestContent).ConfigureAwait(true);
        //
        //     response.StatusCode.Should().Be(400);
        //     var content = await response.Content.ReadAsStringAsync().ConfigureAwait(true);
        //     var patchResponse = JsonConvert.DeserializeObject<string>(content);
        //
        //     patchResponse.Should().Be($"Person with id {incorrectPersonId} not found");
        // }
        //
        //
        // [Test]
        // public async Task WhenTheRequestContainsAnIncorrectMashResidentIdReturnsRelevantErrorMessage()
        // {
        //     var incorrectMashResidentId = _unlinkedResident.Id + 20;
        //     var request = new UpdateMashResidentRequest { SocialCareId = _existingDbPerson.Id };
        //     var patchUri = new Uri($"/api/v1/mash-resident/{incorrectMashResidentId}", UriKind.Relative);
        //     var serializedRequest = JsonSerializer.Serialize(request);
        //     var requestContent = new StringContent(serializedRequest, Encoding.UTF8, "application/json");
        //
        //     var response = await Client.PatchAsync(patchUri, requestContent).ConfigureAwait(true);
        //
        //     response.StatusCode.Should().Be(400);
        //     var content = await response.Content.ReadAsStringAsync().ConfigureAwait(true);
        //     var patchResponse = JsonConvert.DeserializeObject<string>(content);
        //
        //     patchResponse.Should().Be($"MASH resident with id {incorrectMashResidentId} not found");
        // }
    }
}
