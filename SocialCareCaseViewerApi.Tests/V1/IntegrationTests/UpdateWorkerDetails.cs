using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Bogus;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using NUnit.Framework;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Boundary.Response;
using SocialCareCaseViewerApi.V1.Domain;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace SocialCareCaseViewerApi.Tests.V1.IntegrationTests
{
    [TestFixture]
    public class UpdateWorkerDetails : IntegrationTestSetup<Startup>
    {

        [Test]
        public async Task UpdateWorkerWithNewTeamReturnsTheOnlyTheUpdatedTeam()
        {
            // Get request to get an existing worker
            var existingWorker = IntegrationTestHelpers.SetupExistingWorker(DatabaseContext);

            var newTeam = IntegrationTestHelpers.CreateAnotherTeam(DatabaseContext, existingWorker.ContextFlag);

            var getWorkersUri = new Uri($"/api/v1/workers?email={existingWorker.Email}", UriKind.Relative);

            var getWorkersResponse = await Client.GetAsync(getWorkersUri).ConfigureAwait(true);
            getWorkersResponse.StatusCode.Should().Be(200);

            var initialContent = await getWorkersResponse.Content.ReadAsStringAsync().ConfigureAwait(true);
            var initialWorkerResponse = JsonConvert.DeserializeObject<List<WorkerResponse>>(initialContent).ToList();

            initialWorkerResponse.Count.Should().Be(1);
            initialWorkerResponse.Single().Teams.Single().Id.Should().NotBe(newTeam.Id);
            initialWorkerResponse.Single().Teams.Single().Name.Should().NotBe(newTeam.Name);

            // Patch request to update team

            var newTeamRequest = new WorkerTeamRequest { Id = newTeam.Id, Name = newTeam.Name };

            var patchRequest = IntegrationTestHelpers.CreatePatchRequest(existingWorker, newTeamRequest);

            var patchUri = new Uri("/api/v1/workers", UriKind.Relative);

            var serializedRequest = JsonSerializer.Serialize(patchRequest);
            var requestContent = new StringContent(serializedRequest, Encoding.UTF8, "application/json");

            var patchWorkerResponse = await Client.PatchAsync(patchUri, requestContent).ConfigureAwait(true);
            patchWorkerResponse.StatusCode.Should().Be(204);

            // Get request to check team has been updated

            var getUpdatedWorkersResponse = await Client.GetAsync(getWorkersUri).ConfigureAwait(true);
            getUpdatedWorkersResponse.StatusCode.Should().Be(200);

            var updatedContent = await getUpdatedWorkersResponse.Content.ReadAsStringAsync().ConfigureAwait(true);
            var updatedWorkerResponse = JsonConvert.DeserializeObject<List<WorkerResponse>>(updatedContent).ToList();

            updatedWorkerResponse.Count.Should().Be(1);

            // NOTE: This should fail to replicate current bug
            updatedWorkerResponse.Single().Teams.Count.Should().Be(1);

            updatedWorkerResponse.Single().Teams.Single().Id.Should().Be(newTeam.Id);
            updatedWorkerResponse.Single().Teams.Single().Name.Should().Be(newTeam.Name);
        }
    }
}
