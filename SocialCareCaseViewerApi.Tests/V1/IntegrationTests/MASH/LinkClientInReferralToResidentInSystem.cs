using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Infrastructure;

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

            _unlinkedResident = IntegrationTestHelpers.CreateUnLinkedMashResident(DatabaseContext, _mashReferral, _existingDbPerson);
        }

        [Test]
        public async Task SuccessfulLinkingMatchesMashResidentToSavedPersonInDb()
        {
            _unlinkedResident.SocialCareId.Should().BeNull();

            var request = new UpdateMashResidentRequest { SocialCareId = _existingDbPerson.Id };
            var patchUri = new Uri($"/api/v1/mash-resident/{_unlinkedResident.Id}", UriKind.Relative);
            var serializedRequest = JsonSerializer.Serialize(request);
            var requestContent = new StringContent(serializedRequest, Encoding.UTF8, "application/json");

            var linkMashResidentResponse = await Client.PatchAsync(patchUri, requestContent).ConfigureAwait(true);

            linkMashResidentResponse.StatusCode.Should().Be(200);

            _unlinkedResident.SocialCareId.Should().Be(_existingDbPerson.Id);
        }
    }
}
