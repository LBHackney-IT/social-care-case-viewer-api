using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using NUnit.Framework;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Boundary.Response;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace SocialCareCaseViewerApi.Tests.V1.IntegrationTests
{
    [TestFixture]
    public class UpdateWorkerDetails : IntegrationTestSetup<Startup>
    {
        private SocialCareCaseViewerApi.V1.Infrastructure.Worker _existingDbWorker;
        private SocialCareCaseViewerApi.V1.Infrastructure.Worker _allocationWorker;
        private SocialCareCaseViewerApi.V1.Infrastructure.Team _existingDbTeam;
        private SocialCareCaseViewerApi.V1.Infrastructure.Team _differentDbTeam;
        private SocialCareCaseViewerApi.V1.Infrastructure.Person _resident;

        [SetUp]
        public void Setup()
        {
            // Clear test database of any rows in the database
            DatabaseContext.Database.ExecuteSqlRaw("DELETE from dbo.sccv_team;");
            DatabaseContext.Database.ExecuteSqlRaw("DELETE from dbo.sccv_worker;");
            DatabaseContext.Database.ExecuteSqlRaw("DELETE from dbo.sccv_workerteam;");
            DatabaseContext.Database.ExecuteSqlRaw("DELETE from dbo.dm_persons;");

            // Create an existing workers,teams and worker teams and associated insert statements
            (var existingDbWorker, var existingDbTeam, var existingDbWorkerTeam) = IntegrationTestHelpers.SetupExistingWorker(DatabaseContext);
            (var allocationWorker, var allocatorTeam, var allocatorWorkerTeam) = IntegrationTestHelpers.SetupExistingWorker(DatabaseContext);

            var differentDbTeam = IntegrationTestHelpers.CreateAnotherTeam(DatabaseContext, existingDbWorker.ContextFlag);

            //Create existing resident with same context as worker
            var resident = IntegrationTestHelpers.CreateExistingPerson(DatabaseContext, ageContext: existingDbWorker.ContextFlag);

            _existingDbWorker = existingDbWorker;
            _existingDbTeam = existingDbTeam;
            _allocationWorker = allocationWorker;
            _differentDbTeam = differentDbTeam;
            _resident = resident;
        }


        [Test]
        public async Task UpdateWorkerWithNewTeamReturnsTheOnlyTheUpdatedTeam()
        {
            // Patch request to update team
            var patchUri = new Uri("/api/v1/workers", UriKind.Relative);

            var newTeamRequest = new WorkerTeamRequest { Id = _differentDbTeam.Id, Name = _differentDbTeam.Name };
            var patchRequest = IntegrationTestHelpers.CreatePatchRequest(_existingDbWorker, newTeamRequest);
            var serializedRequest = JsonSerializer.Serialize(patchRequest);

            var requestContent = new StringContent(serializedRequest, Encoding.UTF8, "application/json");
            var patchWorkerResponse = await Client.PatchAsync(patchUri, requestContent).ConfigureAwait(true);
            patchWorkerResponse.StatusCode.Should().Be(204);

            // Get request to check team has been updated
            var getUri = new Uri($"/api/v1/workers?email={_existingDbWorker.Email}", UriKind.Relative);
            var getUpdatedWorkersResponse = await Client.GetAsync(getUri).ConfigureAwait(true);
            getUpdatedWorkersResponse.StatusCode.Should().Be(200);

            var updatedContent = await getUpdatedWorkersResponse.Content.ReadAsStringAsync().ConfigureAwait(true);
            var updatedWorkerResponse = JsonConvert.DeserializeObject<List<WorkerResponse>>(updatedContent).ToList();
            updatedWorkerResponse.Count.Should().Be(1);

            // NOTE: This should fail to replicate current bug
            updatedWorkerResponse.Single().Teams.Count.Should().Be(1);
            updatedWorkerResponse.Single().Teams.Single().Id.Should().Be(newTeamRequest.Id);
            updatedWorkerResponse.Single().Teams.Single().Name.Should().Be(newTeamRequest.Name);
        }

        [Test]
        public async Task UpdateWorkerWithNewTeamUpdatesAnyAllocationsAssociated()
        {
            // Create Allocation request for test worker
            var CreateAllocationUri = new Uri("/api/v1/allocations", UriKind.Relative);

            var allocationRequest = IntegrationTestHelpers.CreateAllocationRequest(_resident.Id, _existingDbTeam.Id, _existingDbWorker.Id, _allocationWorker);
            var serializedRequest = JsonSerializer.Serialize(allocationRequest);

            var requestContent = new StringContent(serializedRequest, Encoding.UTF8, "application/json");

            var allocationResponse = await Client.PostAsync(CreateAllocationUri, requestContent).ConfigureAwait(true);
            allocationResponse.StatusCode.Should().Be(201);

            // Patch request to update team
            var patchUri = new Uri("/api/v1/workers", UriKind.Relative);

            var newTeamRequest = new WorkerTeamRequest { Id = _differentDbTeam.Id, Name = _differentDbTeam.Name };
            var patchRequest = IntegrationTestHelpers.CreatePatchRequest(_existingDbWorker, newTeamRequest);
            var patchTeamSerializedRequest = JsonSerializer.Serialize(patchRequest);

            var patchRequestContent = new StringContent(patchTeamSerializedRequest, Encoding.UTF8, "application/json");
            var patchWorkerResponse = await Client.PatchAsync(patchUri, patchRequestContent).ConfigureAwait(true);
            patchWorkerResponse.StatusCode.Should().Be(204);

            // Get request to check team has been updated on the allocation
            var getAllocationsUri = new Uri($"/api/v1/allocations?mosaic_id={_resident.Id}", UriKind.Relative);
            var getAllocationsResponse = await Client.GetAsync(getAllocationsUri).ConfigureAwait(true);
            getAllocationsResponse.StatusCode.Should().Be(200);

            var allocationsContent = await getAllocationsResponse.Content.ReadAsStringAsync().ConfigureAwait(true);
            var updatedAllocationResponse = JsonConvert.DeserializeObject<AllocationList>(allocationsContent);

            updatedAllocationResponse.Allocations.Count.Should().Be(1);
            updatedAllocationResponse.Allocations.Single().AllocatedWorkerTeam.Should().Be(newTeamRequest.Name);
        }
    }
}
