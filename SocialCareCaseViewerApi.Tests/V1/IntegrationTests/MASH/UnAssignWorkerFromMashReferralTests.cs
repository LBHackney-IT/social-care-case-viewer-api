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
        private SocialCareCaseViewerApi.V1.Infrastructure.Worker _existingDbWorker;
        private SocialCareCaseViewerApi.V1.Infrastructure.MashReferral _existingDbReferral;

        [SetUp]
        public void Setup()
        {
            // Clear test database of any rows in the database
            DatabaseContext.Teams.RemoveRange(DatabaseContext.Teams);
            DatabaseContext.Workers.RemoveRange(DatabaseContext.Workers);
            DatabaseContext.MashReferrals.RemoveRange(DatabaseContext.MashReferrals);

            // Create existing referral and unrelated worker
            var (existingDbWorker, _) = IntegrationTestHelpers.SetupExistingWorker(DatabaseContext);
            var existingDbReferral = IntegrationTestHelpers.SaveMashReferralToDatabase(DatabaseContext);
            _existingDbWorker = existingDbWorker;
            _existingDbReferral = existingDbReferral;
        }

        [Test]
        public async Task SuccessfulPatchUnAssignsWorkerToReferral()
        {
            var request = TestHelpers.CreateUpdateMashReferral();
            request.WorkerId = _existingDbWorker.Id;
            request.WorkerEmail = null;
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
