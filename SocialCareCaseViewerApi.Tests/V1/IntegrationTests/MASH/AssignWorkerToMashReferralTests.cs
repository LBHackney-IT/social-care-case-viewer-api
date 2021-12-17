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
    public class AssignWorkerToMashReferralTests : IntegrationTestSetup<Startup>
    {
        private SocialCareCaseViewerApi.V1.Infrastructure.Worker _existingDbWorker;
        private SocialCareCaseViewerApi.V1.Infrastructure.MashReferral _existingDbRefferal;

        [SetUp]
        public void Setup()
        {
            // Clear test database of any rows in the database
            DatabaseContext.Teams.RemoveRange(DatabaseContext.Teams);
            DatabaseContext.Workers.RemoveRange(DatabaseContext.Workers);
            DatabaseContext.MashReferrals.RemoveRange(DatabaseContext.MashReferrals);

            // Create existing refferal and unrelated worker
            var (existingDbWorker, _) = IntegrationTestHelpers.SetupExistingWorker(DatabaseContext);
            var existingDbRefferal = IntegrationTestHelpers.SaveMashReferralToDatabase(DatabaseContext);
            _existingDbWorker = existingDbWorker;
            _existingDbRefferal = existingDbRefferal;
        }

        [Test]
        public async Task SuccessfulPatchAssignsWorkerToReferral()
        {
            var request = TestHelpers.CreateUpdateMashReferral();
            request.WorkerId = _existingDbWorker.Id;
            request.WorkerEmail = null;
            request.UpdateType = "ASSIGN-WORKER";
            var postUri = new Uri($"/api/v1/mash-referral/{_existingDbRefferal.Id}", UriKind.Relative);
            var serializedRequest = JsonSerializer.Serialize(request);
            var requestContent = new StringContent(serializedRequest, Encoding.UTF8, "application/json");

            var updatedMashReferralResponse = await Client.PatchAsync(postUri, requestContent).ConfigureAwait(true);

            updatedMashReferralResponse.StatusCode.Should().Be(200);
            _existingDbRefferal.WorkerId.Should().Equals(_existingDbWorker.Id);
        }
    }
}
