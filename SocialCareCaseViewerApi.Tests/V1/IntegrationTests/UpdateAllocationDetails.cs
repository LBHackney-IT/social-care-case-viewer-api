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
    public class UpdateAllocationDetails : IntegrationTestSetup<Startup>
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
        public async Task UpdateAllocationWithNewRagRatingUpdatesTheAllocation()
        {
            // Create allocation via API
            var patchUri = new Uri("/api/v1/allocations", UriKind.Relative);
            var newAllocationRequest = new CreateAllocationRequest()
            {
                MosaicId = _resident.Id,
                AllocatedTeamId = _existingDbTeam.Id,
                AllocationStartDate = DateTime.Now,
                Summary = "summary",
                RagRating = "high",
                CreatedBy = _allocationWorker.Email,
                CarePackage = "package"

            };
            var serializedRequest = JsonSerializer.Serialize(newAllocationRequest);
            var requestContent = new StringContent(serializedRequest, Encoding.UTF8, "application/json");
            var createAllocationResponse = await Client.PostAsync(patchUri, requestContent).ConfigureAwait(true);

            // Check the API response and DB records
            var createdAllocation = DatabaseContext.Allocations.First();
            createdAllocation.PersonId.Should().Equals(newAllocationRequest.MosaicId);
            createAllocationResponse.StatusCode.Should().Be(201);

            // Create update rag rating request and update created allocation via API
            var newAllocationPatchRequest = IntegrationTestHelpers.PatchAllocationRequest(
                createdAllocation.Id,
                createdByWorker: _existingDbWorker,
                ragRating: "low",
                deallocationReason: null,
                deallocationDate: null);
            var serializedPatchRequest = JsonSerializer.Serialize(newAllocationPatchRequest);
            var patchRequestContent = new StringContent(serializedPatchRequest, Encoding.UTF8, "application/json");
            var patchAllocationResponse = await Client.PatchAsync(patchUri, patchRequestContent).ConfigureAwait(true);

            // Check the API response and DB records
            var updatedAllocation = DatabaseContext.Allocations.First();
            patchAllocationResponse.StatusCode.Should().Be(200);
            updatedAllocation.RagRating.Should().Equals(newAllocationPatchRequest.RagRating);
            updatedAllocation.Summary.Should().Equals(createdAllocation.Summary);
            updatedAllocation.CarePackage.Should().Equals(createdAllocation.CarePackage);
            updatedAllocation.PersonId.Should().Equals(createdAllocation.PersonId);
            updatedAllocation.AllocationStartDate.Should().Equals(createdAllocation.AllocationStartDate);
            updatedAllocation.CreatedBy.Should().Equals(createdAllocation.CreatedBy);
        }

        [Test]
        public async Task UpdateAllocationWithDeallocationDetailsUpdatesTheAllocation()
        {
            var today = DateTime.Today;

            // Create allocation via API
            var yesterday = today.AddDays(-1);
            var patchUri = new Uri("/api/v1/allocations", UriKind.Relative);
            var newAllocationRequest = new CreateAllocationRequest()
            {
                MosaicId = _resident.Id,
                AllocatedTeamId = _existingDbTeam.Id,
                AllocationStartDate = yesterday,
                Summary = "summary",
                RagRating = "high",
                CreatedBy = _allocationWorker.Email,
                CarePackage = "package"
            };
            var serializedRequest = JsonSerializer.Serialize(newAllocationRequest);
            var requestContent = new StringContent(serializedRequest, Encoding.UTF8, "application/json");
            var createAllocationResponse = await Client.PostAsync(patchUri, requestContent).ConfigureAwait(true);

            // Check the API response and DB records
            var createdAllocation = DatabaseContext.Allocations.First();
            createdAllocation.PersonId.Should().Equals(newAllocationRequest.MosaicId);
            createAllocationResponse.StatusCode.Should().Be(201);

            // Create deallocation request and update created allocation via API
            var newAllocationPatchRequest = IntegrationTestHelpers.PatchAllocationRequest(
                allocationId: createdAllocation.Id,
                createdByWorker: _existingDbWorker,
                ragRating: null,
                deallocationReason: "reason",
                deallocationDate: today);
            var serializedPatchRequest = JsonSerializer.Serialize(newAllocationPatchRequest);
            var patchRequestContent = new StringContent(serializedPatchRequest, Encoding.UTF8, "application/json");
            var patchAllocationResponse = await Client.PatchAsync(patchUri, patchRequestContent).ConfigureAwait(true);

            // Check the API response and DB records
            var updatedAllocation = DatabaseContext.Allocations.First();
            patchAllocationResponse.StatusCode.Should().Be(200);
            updatedAllocation.RagRating.Should().Equals(newAllocationPatchRequest.RagRating);
            updatedAllocation.Summary.Should().Equals(createdAllocation.Summary);
            updatedAllocation.CarePackage.Should().Equals(createdAllocation.CarePackage);
            updatedAllocation.PersonId.Should().Equals(createdAllocation.PersonId);
            updatedAllocation.AllocationStartDate.Should().Equals(createdAllocation.AllocationStartDate);
            updatedAllocation.CreatedBy.Should().Equals(createdAllocation.CreatedBy);
            updatedAllocation.CaseStatus.Should().Equals("CLOSED");
            updatedAllocation.AllocationEndDate.Should().Equals(today);
        }
    }
}
