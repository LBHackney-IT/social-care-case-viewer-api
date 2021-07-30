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
        private SocialCareCaseViewerApi.V1.Infrastructure.Worker _existingDbWorker;
        private SocialCareCaseViewerApi.V1.Infrastructure.Team _differentDbTeam;

        [SetUp]
        public void Setup()
        {
            (var existingDbWorker, var insertTeamQuery, var insertWorkerTeamQuery, var insertWorkerQuery) = IntegrationTestHelpers.SetupExistingWorker();
            (var differentDbTeam, var insertDifferentTeamQuery) = IntegrationTestHelpers.CreateAnotherTeam(existingDbWorker.ContextFlag);

            DatabaseContext.Database.ExecuteSqlRaw("DELETE from dbo.sccv_team;");
            DatabaseContext.Database.ExecuteSqlRaw("DELETE from dbo.sccv_worker;");
            DatabaseContext.Database.ExecuteSqlRaw("DELETE from dbo.sccv_workerteam;");

            DatabaseContext.Database.ExecuteSqlRaw(insertTeamQuery);
            DatabaseContext.Database.ExecuteSqlRaw(insertDifferentTeamQuery);

            DatabaseContext.Database.ExecuteSqlRaw(insertWorkerTeamQuery);
            DatabaseContext.Database.ExecuteSqlRaw(insertWorkerQuery);

            _existingDbWorker = existingDbWorker;
            _differentDbTeam = differentDbTeam;

            // DatabaseContext.Database
            // .ExecuteSqlRaw("insert into dbo.sccv_worker (id, email, first_name, last_name, role, context_flag) values (91, 'bhadfield5@example.com', 'Basilio', 'Hadfield', 'non', 'C');");

            // DatabaseContext.Database.ExecuteSqlRaw("UPDATE DBO.SCCV_WORKER SET is_active = true WHERE is_active isnull;");

            // DatabaseContext.Database
            // .ExecuteSqlRaw("insert into dbo.sccv_team (id, name, context) values (35, 'Aenean', 'C');");

            // DatabaseContext.Database
            // .ExecuteSqlRaw("insert into dbo.sccv_team (id, name, context) values (20, 'Tristique', 'C');");

            // DatabaseContext.Database
            // .ExecuteSqlRaw("insert into dbo.sccv_workerteam (id, worker_id, team_id) values (29, 91, 35);");
        }


        [Test]
        public async Task UpdateWorkerWithNewTeamReturnsTheOnlyTheUpdatedTeam()
        {
            var newTeamRequest = new WorkerTeamRequest { Id = _differentDbTeam.Id, Name = _differentDbTeam.Name };

            // Patch request to update team

            var patchRequest = IntegrationTestHelpers.CreatePatchRequest(_existingDbWorker, newTeamRequest);

            var patchUri = new Uri("/api/v1/workers", UriKind.Relative);

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
