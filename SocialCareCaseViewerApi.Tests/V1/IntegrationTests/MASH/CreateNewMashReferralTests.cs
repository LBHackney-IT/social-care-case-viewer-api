using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Newtonsoft.Json;
using NUnit.Framework;
using SocialCareCaseViewerApi.Tests.V1.Helpers;
using SocialCareCaseViewerApi.V1.Infrastructure;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace SocialCareCaseViewerApi.Tests.V1.IntegrationTests.MASH
{
    [TestFixture]
    public class CreateNewMashReferralTests : IntegrationTestSetup<Startup>
    {
        [SetUp]
        public void Setup()
        {

        }

        [Test]
        public async Task SuccessfulPostReturns201()
        {
            var postUri = new Uri($"/api/v1/mash-referral", UriKind.Relative);

            var request = TestHelpers.CreateNewMashReferralRequest();
            var serializedRequest = JsonSerializer.Serialize(request);
            var requestContent = new StringContent(serializedRequest, Encoding.UTF8, "application/json");

            var createMashReferralResponse = await Client.PostAsync(postUri, requestContent).ConfigureAwait(true);
            createMashReferralResponse.StatusCode.Should().Be(201);
        }

        [Test]
        [Ignore("Does not work as have not implemented a way to retrieve the newly inserted referral")]
        public async Task SuccessfulPostCreatesANewReferralAtTheContactStage()
        {
            var postUri = new Uri($"/api/v1/mash-referral", UriKind.Relative);

            var request = TestHelpers.CreateNewMashReferralRequest();
            var serializedRequest = JsonSerializer.Serialize(request);
            var requestContent = new StringContent(serializedRequest, Encoding.UTF8, "application/json");

            var createMashReferralResponse = await Client.PostAsync(postUri, requestContent).ConfigureAwait(true);

            //Get request to check that the new referral has been added
            var getUri = new Uri($"/api/v1/mash-referral", UriKind.Relative);
            var getReferralResponse = await Client.GetAsync(getUri).ConfigureAwait(true);

            var addedContent = await getReferralResponse.Content.ReadAsStringAsync().ConfigureAwait(true);
            var addedReferralResponse = JsonConvert.DeserializeObject<List<MashReferral>>(addedContent).ToList();

            addedReferralResponse.Count.Should().Be(1);
            addedReferralResponse.FirstOrDefault()?.Referrer.Should().BeEquivalentTo(request.Referrer);
            addedReferralResponse.FirstOrDefault()?.RequestedSupport.Should().BeEquivalentTo(request.RequestedSupport);
            addedReferralResponse.FirstOrDefault()?.Clients.Should().BeEquivalentTo(request.Clients);
            addedReferralResponse.FirstOrDefault()?.ReferralDocumentURI.Should().BeEquivalentTo(request.ReferralUri);
            addedReferralResponse.FirstOrDefault()?.Stage.Should().BeEquivalentTo("CONTACT");
        }
    }
}
