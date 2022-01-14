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
    public class UnLinkClientInReferralToResidentInSystem : IntegrationTestSetup<Startup>
    {
        private MashReferral _mashReferral;
        private MashResident _linkedResident;
        private Person _existingDbPerson;

        [SetUp]
        public void Setup()
        {
            _existingDbPerson = IntegrationTestHelpers.CreateExistingPerson(DatabaseContext);
            _mashReferral = IntegrationTestHelpers.SaveMashReferralToDatabase(DatabaseContext, "CONTACT");

            _linkedResident =
                IntegrationTestHelpers.CreateMashResident(DatabaseContext, _mashReferral, _existingDbPerson);
        }

        [Test]
        public async Task SuccessfulUnLinkingRemovesLinkFromMashResidentToSavedPersonInDb()
        {
            _linkedResident.SocialCareId.Should().NotBeNull();

            var request = new UpdateMashResidentRequest { UpdateType = "UNLINK-PERSON"};
            var patchUri = new Uri($"/api/v1/mash-resident/{_linkedResident.Id}", UriKind.Relative);
            var serializedRequest = JsonSerializer.Serialize(request);
            var requestContent = new StringContent(serializedRequest, Encoding.UTF8, "application/json");

            var unlinkMashResidentResponse = await Client.PatchAsync(patchUri, requestContent).ConfigureAwait(true);

            unlinkMashResidentResponse.StatusCode.Should().Be(200);

            _linkedResident.SocialCareId.Should().BeNull();
        }
    }
}
