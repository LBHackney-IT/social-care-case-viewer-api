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
        private SocialCareCaseViewerApi.V1.Infrastructure.Team _differentDbTeam;

        [SetUp]
        public void Setup()
        {
            // Clear test database of any rows in the database
            DatabaseContext.Database.ExecuteSqlRaw("DELETE from dbo.sccv_team;");
            DatabaseContext.Database.ExecuteSqlRaw("DELETE from dbo.sccv_worker;");
            DatabaseContext.Database.ExecuteSqlRaw("DELETE from dbo.sccv_workerteam;");

            // Create an existing worker,team and worker team and associated insert statements
            (var existingDbWorker, var insertTeamQuery, var insertWorkerTeamQuery, var insertWorkerQuery) = IntegrationTestHelpers.SetupExistingWorker();
            (var differentDbTeam, var insertDifferentTeamQuery) = IntegrationTestHelpers.CreateAnotherTeam(existingDbWorker.ContextFlag);

            // Seed fake data into the test database before running tests
            DatabaseContext.Database.ExecuteSqlRaw(insertTeamQuery);
            DatabaseContext.Database.ExecuteSqlRaw(insertDifferentTeamQuery);
            DatabaseContext.Database.ExecuteSqlRaw(insertWorkerTeamQuery);
            DatabaseContext.Database.ExecuteSqlRaw(insertWorkerQuery);

            _existingDbWorker = existingDbWorker;
            _differentDbTeam = differentDbTeam;
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
    }
}
