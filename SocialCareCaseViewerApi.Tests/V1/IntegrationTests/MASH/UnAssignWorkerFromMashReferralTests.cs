using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using SocialCareCaseViewerApi.Tests.V1.Helpers;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace SocialCareCaseViewerApi.Tests.V1.IntegrationTests.MASH
{
    [TestFixture]
    public class UnAssignWorkerFromMashReferralTests : IntegrationTestSetup<Startup>
    {
        private SocialCareCaseViewerApi.V1.Infrastructure.MashReferral _existingDbReferral;

        [SetUp]
        public void Setup()
        {
            // Create existing referral and unrelated worker
            _existingDbReferral = IntegrationTestHelpers.SaveMashReferralToDatabase(DatabaseContext, "CONTACT");
        }

        [Test]
        public async Task SuccessfulPatchUnAssignsWorkerFromReferral()
        {
            _existingDbReferral.WorkerId.Should().NotBeNull();

            var request = TestHelpers.CreateUpdateMashReferral();
            request.UpdateType = "UNASSIGN-WORKER";
            var postUri = new Uri($"/api/v1/mash-referral/{_existingDbReferral.Id}", UriKind.Relative);
            var serializedRequest = JsonSerializer.Serialize(request);
            var requestContent = new StringContent(serializedRequest, Encoding.UTF8, "application/json");

            var updatedMashReferralResponse = await Client.PatchAsync(postUri, requestContent).ConfigureAwait(true);

            _existingDbReferral.WorkerId.Should().Be(null);
            updatedMashReferralResponse.StatusCode.Should().Be(200);
        }
    }
}
