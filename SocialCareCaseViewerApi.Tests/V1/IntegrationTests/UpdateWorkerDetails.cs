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
            var newTeamRequest = new WorkerTeamRequest { Id = 20, Name = "Tristique" };
            var getUri = new Uri("/api/v1/workers?email=bhadfield5@example.com", UriKind.Relative);

            // Patch request to update team

            var patchRequest = new UpdateWorkerRequest
            {
                WorkerId = 91,
                ModifiedBy = new Faker().Person.Email,
                FirstName = "Basilio",
                LastName = "Hadfield",
                ContextFlag = "C",
                Teams = new List<WorkerTeamRequest> { newTeamRequest },
                Role = "non",
                DateStart = new Faker().Date.Past()
            };

            var patchUri = new Uri("/api/v1/workers", UriKind.Relative);

            var serializedRequest = JsonSerializer.Serialize(patchRequest);
            var requestContent = new StringContent(serializedRequest, Encoding.UTF8, "application/json");

            var patchWorkerResponse = await Client.PatchAsync(patchUri, requestContent).ConfigureAwait(true);
            patchWorkerResponse.StatusCode.Should().Be(204);

            // Get request to check team has been updated
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
