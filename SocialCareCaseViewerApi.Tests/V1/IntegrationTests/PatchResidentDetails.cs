using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using MongoDB.Bson.IO;
using NUnit.Framework;
using SocialCareCaseViewerApi.V1.Boundary.Requests;

namespace SocialCareCaseViewerApi.Tests.V1.IntegrationTests
{
    [TestFixture]
    public class PatchResidentDetails : IntegrationTestSetup<Startup>
    {
        private SocialCareCaseViewerApi.V1.Infrastructure.Person _resident;
        private SocialCareCaseViewerApi.V1.Infrastructure.Worker _existingDbWorker;
        private SocialCareCaseViewerApi.V1.Infrastructure.Team _existingDbTeam;

        [SetUp]
        public void Setup()
        {
            _resident = IntegrationTestHelpers.CreateExistingPerson(DatabaseContext);
            (_existingDbWorker, _existingDbTeam) = IntegrationTestHelpers.SetupExistingWorker(DatabaseContext);


        }


        [Test]
        public async Task UpdateWorkerWithNewTeamReturnsTheOnlyTheUpdatedTeam()
        {
            var patchUri = new Uri("/api/v1/residents", UriKind.Relative);
            var request = new PatchPersonRequest()
            {
                Id = _resident.Id,
                EmailAddress =_existingDbWorker.Email,
                Title = "Update"
            } ;

            var serializedRequest = JsonSerializer.Serialize(request);
            var requestContent = new StringContent(serializedRequest, Encoding.UTF8, "application/json");
            var patchResidentResponse = await Client.PatchAsync(patchUri, requestContent).ConfigureAwait(true);
            patchResidentResponse.StatusCode.Should().Be(204);
            _resident.Title.Should().Equals(request.Title);
        }
    }
}
