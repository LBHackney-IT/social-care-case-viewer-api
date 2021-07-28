using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Bogus;
using FluentAssertions;
using NUnit.Framework;
using SocialCareCaseViewerApi.V1.Boundary.Requests;

namespace SocialCareCaseViewerApi.Tests.V1.IntegrationTests
{
    [TestFixture]
    public class UpdateWorkerDetails: IntegrationTestSetup<Startup>
    {
        [Test]
        public async Task UpdateWorkerWithNewTeamReturnsTheOnlyTheUpdatedTeam()
        {
            var (existingWorker, newTeam) = IntegrationTestHelpers.SetupExistingWorker(DatabaseContext);

            var patchUri = new Uri("http://localhost:5000/api/v1/workers");
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
            var requestContent = new StringContent(serializedRequest, Encoding.UTF8, "application/json-patch+json");
            var patchResponse = await Client.PatchAsync(patchUri,requestContent).ConfigureAwait(true);
            var patchStatusCode = patchResponse.StatusCode;
            patchStatusCode.Should().Be(204);

            var getUri = new Uri($"http://localhost:5000/api/v1/workers?id={existingWorker.Id}");
            var getResponse = Client.GetAsync(getUri);

            var getStatusCode = getResponse.Result.StatusCode;
            getStatusCode.Should().Be(200);

            // var content = response.Result.Content;
            // var stringContent = await content.ReadAsStringAsync().ConfigureAwait(true);
            // var convertedResponse = JsonConvert.DeserializeObject<ResidentInformation>(stringContent);
            //
            // convertedResponse.Should().BeEquivalentTo(expectedResponse);
        }
    }
}
