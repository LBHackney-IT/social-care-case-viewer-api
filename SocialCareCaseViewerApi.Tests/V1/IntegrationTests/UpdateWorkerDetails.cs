using System;
using System.Collections.Generic;
using System.Linq;
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

            // Create existing workers with teams
            (_existingDbWorker, _existingDbTeam) = IntegrationTestHelpers.SetupExistingWorker(DatabaseContext);
            (_allocationWorker, _) = IntegrationTestHelpers.SetupExistingWorker(DatabaseContext);

            _differentDbTeam = IntegrationTestHelpers.CreateTeam(DatabaseContext, _existingDbWorker.ContextFlag);

            // Create an existing resident that shares the same age context as existingDbWorker
            _resident = IntegrationTestHelpers.CreateExistingPerson(DatabaseContext, ageContext: _existingDbWorker.ContextFlag);

        }

        [Test]
        public async Task UpdateWorkerWithNewTeamReturnsTheOnlyTheUpdatedTeam()
        {
            // Patch request to update team of existingDbWorker
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

            //worker should have two teams now
            updatedWorkerResponse.First().Teams.Count.Should().Be(2);
            updatedWorkerResponse.First().Teams.Any(x => x.Id == newTeamRequest.Id && x.Name == newTeamRequest.Name).Should().BeTrue();
            updatedWorkerResponse.First().Teams.Any(x => x.Id == _existingDbTeam.Id && x.Name == _existingDbTeam.Name).Should().BeTrue();

            // Check the db state as well
            var persistedWorkerTeams = DatabaseContext.WorkerTeams.Where(x => x.WorkerId.Equals(_existingDbWorker.Id)).ToList();
            persistedWorkerTeams.Count.Should().Be(2);
            persistedWorkerTeams.Any(x => x.Team.Id == newTeamRequest.Id && x.Team.Name == newTeamRequest.Name).Should().BeTrue();
            persistedWorkerTeams.Any(x => x.Team.Id == _existingDbTeam.Id && x.Team.Name == _existingDbTeam.Name).Should().BeTrue();
        }

        [Test]
        public async Task UpdateWorkerWithNewTeamUpdatesAnyAllocationsAssociated()
        {
            var firstDbAllocation = new AllocationSet()
            {
                PersonId = _resident.Id,
                TeamId = _existingDbTeam.Id,
                WorkerId = _existingDbWorker.Id,
                RagRating = "red",
                CreatedBy = _allocationWorker.Email,
                AllocationStartDate = DateTime.Now,
            };
            DatabaseContext.Allocations.Add(firstDbAllocation);
            DatabaseContext.SaveChanges();

            // Create another allocation request for existingDbWorker, due to new restrictions in use case unable to create second allocation via API, saving directly to the DB.
            var secondDbAllocation = new AllocationSet()
            {
                PersonId = _resident.Id,
                TeamId = _existingDbTeam.Id,
                WorkerId = _existingDbWorker.Id,
                RagRating = "red",
                CreatedBy = _allocationWorker.Email,
                AllocationStartDate = DateTime.Now,
            };
            DatabaseContext.Allocations.Add(secondDbAllocation);
            DatabaseContext.SaveChanges();

            // Patch request to update team of existingDbWorker
            var patchUri = new Uri("/api/v1/workers", UriKind.Relative);

            var newTeamRequest = new WorkerTeamRequest { Id = _differentDbTeam.Id, Name = _differentDbTeam.Name };
            var patchRequest = IntegrationTestHelpers.CreatePatchRequest(_existingDbWorker, newTeamRequest);
            var patchTeamSerializedRequest = JsonSerializer.Serialize(patchRequest);

            var patchRequestContent = new StringContent(patchTeamSerializedRequest, Encoding.UTF8, "application/json");
            var patchWorkerResponse = await Client.PatchAsync(patchUri, patchRequestContent).ConfigureAwait(true);
            patchWorkerResponse.StatusCode.Should().Be(204);

            // Get request to check team has been updated on existingDbWorker's allocations
            var getAllocationsUri = new Uri($"/api/v1/allocations?mosaic_id={_resident.Id}", UriKind.Relative);
            var getAllocationsResponse = await Client.GetAsync(getAllocationsUri).ConfigureAwait(true);
            getAllocationsResponse.StatusCode.Should().Be(200);

            var allocationsContent = await getAllocationsResponse.Content.ReadAsStringAsync().ConfigureAwait(true);
            var updatedAllocationResponse = JsonConvert.DeserializeObject<AllocationList>(allocationsContent);

            updatedAllocationResponse.Allocations.Count.Should().Be(2);

            var firstAllocation = updatedAllocationResponse.Allocations.ElementAtOrDefault(0);

            firstAllocation?.AllocatedWorkerTeam.Should().BeNull();
            firstAllocation?.PersonId.Should().Be(_resident.Id);
            firstAllocation?.AllocatedWorker.Should().Be($"{_existingDbWorker.FirstName} {_existingDbWorker.LastName}");

            var secondAllocation = updatedAllocationResponse.Allocations.ElementAtOrDefault(1);

            secondAllocation?.AllocatedWorkerTeam.Should().BeNull();
            secondAllocation?.PersonId.Should().Be(_resident.Id);
            secondAllocation?.AllocatedWorker.Should().Be($"{_existingDbWorker.FirstName} {_existingDbWorker.LastName}");
        }
    }
}
