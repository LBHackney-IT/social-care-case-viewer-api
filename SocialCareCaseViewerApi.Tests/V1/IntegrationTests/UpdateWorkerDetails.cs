using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Bogus;
using FluentAssertions;
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
        [Test]
        public async Task UpdateWorkerWithNewTeamReturnsTheOnlyTheUpdatedTeam()
        {
            var (existingWorker, newTeam) = IntegrationTestHelpers.SetupExistingWorker(DatabaseContext);

            var patchUri = new Uri("/api/v1/workers", UriKind.Relative);
            var patchRequest = new UpdateWorkerRequest
            {
                WorkerId = existingWorker.Id,
                ModifiedBy = new Faker().Person.Email,
                FirstName = existingWorker.FirstName,
                LastName = existingWorker.LastName,
                ContextFlag = existingWorker.ContextFlag,
                Teams = new List<WorkerTeamRequest>
                {
                    new WorkerTeamRequest{Id = newTeam.Id, Name = newTeam.Name}
                },
                Role = existingWorker.Role,
                DateStart = new Faker().Date.Recent()
            };

            var serializedRequest = JsonSerializer.Serialize(patchRequest);
            var requestContent = new StringContent(serializedRequest, Encoding.UTF8, "application/json");
            var patchResponse = await Client.PatchAsync(patchUri, requestContent).ConfigureAwait(true);
            var patchStatusCode = patchResponse.StatusCode;
            patchStatusCode.Should().Be(204);

            var getUri = new Uri($"/api/v1/workers?id={existingWorker.Id}",UriKind.Relative);
            var getResponse = Client.GetAsync(getUri);

            var getStatusCode = getResponse.Result.StatusCode;
            getStatusCode.Should().Be(200);

            var content = getResponse.Result.Content;
            var stringContent = await content.ReadAsStringAsync().ConfigureAwait(true);
            var convertedResponse = JsonConvert.DeserializeObject<List<WorkerResponse>>(stringContent);

            convertedResponse.FirstOrDefault().Teams.Count.Should().Be(1);
        }
    }
}
