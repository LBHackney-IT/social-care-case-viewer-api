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
    public class AssignWorkerToMashReferralOnContactDecisionTests : IntegrationTestSetup<Startup>
    {
        private SocialCareCaseViewerApi.V1.Infrastructure.Worker _existingDbWorker;
        private SocialCareCaseViewerApi.V1.Infrastructure.MashReferral _existingDbReferral;

        [SetUp]
        public void Setup()
        {
            // Create existing referral and unrelated worker
            (_existingDbWorker, _) = IntegrationTestHelpers.SetupExistingWorker(DatabaseContext);
            _existingDbReferral = IntegrationTestHelpers.SaveMashReferralToDatabase(DatabaseContext, "CONTACT");
        }

        [Test]
        public async Task SuccessfulPatchAssignsWorkerToReferralOnContactDecision()
        {
            var request = TestHelpers.CreateUpdateMashReferral();
            request.WorkerId = _existingDbWorker.Id;
            request.WorkerEmail = null;
            request.UpdateType = "CONTACT-DECISION";
            request.RequiresUrgentContact = false;
            var postUri = new Uri($"/api/v1/mash-referral/{_existingDbReferral.Id}", UriKind.Relative);
            var serializedRequest = JsonSerializer.Serialize(request);
            var requestContent = new StringContent(serializedRequest, Encoding.UTF8, "application/json");

            var updatedMashReferralResponse = await Client.PatchAsync(postUri, requestContent).ConfigureAwait(true);

            updatedMashReferralResponse.StatusCode.Should().Be(200);
            _existingDbReferral.WorkerId.Should().Equals(_existingDbWorker.Id);
        }
    }
}
