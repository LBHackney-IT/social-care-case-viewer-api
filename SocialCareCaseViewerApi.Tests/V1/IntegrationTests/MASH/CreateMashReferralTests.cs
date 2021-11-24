using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using SocialCareCaseViewerApi.Tests.V1.Helpers;

namespace SocialCareCaseViewerApi.Tests.V1.IntegrationTests.MASH
{
    [TestFixture]
    public class CreateMashReferralTests : IntegrationTestSetup<Startup>
    {
        [SetUp]
        public void Setup()
        {

        }

        [Test]
        public async Task AddMashReferral()
        {
            var postUri = new Uri($"/api/v1/mash-referral", UriKind.Relative);

            var request = TestHelpers.GenerateCreateReferralRequest();
            var serializedRequest = JsonSerializer.Serialize(request);
            var requestContent = new StringContent(serializedRequest, Encoding.UTF8, "application/json");

            var createMashReferralResponse = await Client.PostAsync(postUri, requestContent).ConfigureAwait(true);
            createMashReferralResponse.StatusCode.Should().Be(201);
        }
    }
}
