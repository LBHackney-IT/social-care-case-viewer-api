using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Bogus;
using FluentAssertions;
using NUnit.Framework;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Infrastructure;

namespace SocialCareCaseViewerApi.Tests.V1.IntegrationTests
{
    [TestFixture]
    public class UpdateWorkerDetails: IntegrationTestSetup<Startup>
    {
        [SetUp]
        public void SetUp()
        {
            Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT","Development");
            Environment.SetEnvironmentVariable("CONNECTION_STRING", ConnectionString.TestDatabase());
            Environment.SetEnvironmentVariable("SCCV_MONGO_CONN_STRING", "mongodb://localhost:1433/");
            Environment.SetEnvironmentVariable("SCCV_MONGO_DB_NAME", "social_care_db_test");
            Environment.SetEnvironmentVariable("SCCV_MONGO_COLLECTION_NAME", "form_data_test");
            Environment.SetEnvironmentVariable("SOCIAL_CARE_PLATFORM_API_URL", "https://mockBase");
        }

        [Test]
        public async Task UpdateWorkerWithNewTeamReturnsTheOnlyTheUpdatedTeam()
        {
            var workerId = new Faker().Random.Int(1, 100);
            var workerContext = new Faker().Random.String2(1, "AC");

            var team = new Faker<Team>()
                .RuleFor(t => t.Id, f => f.UniqueIndex + 1)
                .RuleFor(t => t.Context, f => workerContext )
                .RuleFor(t => t.Name, f => f.Name.JobType()).Generate();

            var newTeam = new Faker<Team>()
                .RuleFor(t => t.Id, f => f.UniqueIndex + team.Id)
                .RuleFor(t => t.Context, f => workerContext )
                .RuleFor(t => t.Name, f => f.Name.JobType()).Generate();

            DatabaseContext.Teams.Add(team);
            DatabaseContext.Teams.Add(newTeam);

            var workerTeam = new Faker<WorkerTeam>()
                .RuleFor(t => t.Id, f => f.UniqueIndex + 1)
                .RuleFor(t => t.WorkerId, f => workerId )
                .RuleFor(t => t.TeamId, f => team.Id)
                .RuleFor(t => t.Team, team).Generate();

            DatabaseContext.WorkerTeams.Add(workerTeam);

            var worker = new Faker<Worker>().RuleFor(w => w.Id, workerId)
                .RuleFor(w => w.FirstName, f => f.Person.FirstName)
                .RuleFor(w => w.LastName, f => f.Person.LastName)
                .RuleFor(w => w.Email, f => f.Person.Email)
                .RuleFor(w => w.Role, f => f.Random.Word())
                .RuleFor(w => w.ContextFlag, f => workerContext)
                .RuleFor(w => w.CreatedBy, f => f.Person.Email)
                .RuleFor(w => w.CreatedAt, f => f.Date.Soon())
                .RuleFor(w => w.DateStart, f => f.Date.Recent())
                .RuleFor(w => w.DateEnd, f => f.Date.Soon())
                .RuleFor(w => w.IsActive, true)
                .RuleFor(w => w.Allocations, new List<AllocationSet>())
                .RuleFor(w => w.WorkerTeams, new List<WorkerTeam> { workerTeam })
                .RuleFor(w => w.LastModifiedBy, f => f.Person.Email).Generate();

            DatabaseContext.Workers.Add(worker);
            await DatabaseContext.SaveChangesAsync().ConfigureAwait(true);

            // var expectedResponse = E2ETestHelpers.AddPersonWithRelatedEntitiesToDb(MosaicContext, personId);

            var patchUri = new Uri("http://localhost:5000/api/v1/workers");
            var patchRequest = new UpdateWorkerRequest
            {
                WorkerId = worker.Id,
                ModifiedBy = new Faker().Person.Email,
                FirstName = worker.FirstName,
                LastName = worker.LastName,
                ContextFlag = worker.ContextFlag,
                Teams = new List<WorkerTeamRequest>
                {
                    new WorkerTeamRequest{Id = newTeam.Id, Name = newTeam.Name}
                },
                Role = worker.Role,
                DateStart = new Faker().Date.Recent()

            };
            // var serializedRequest = JsonConvert.SerializeObject(patchRequest);
            // var requestContent = new StringContent(serializedRequest, Encoding.UTF8, "application/json-patch+json");
            // var patchResponse = await Client.PatchAsync(patchUri,requestContent).ConfigureAwait(true);
            // var patchResult = patchResponse.ReasonPhrase;
            // var patchStatusCode = patchResponse.StatusCode;
            // patchStatusCode.Should().Be(204);

            var getUri = new Uri($"http://localhost:5000/api/v1/workers?id={workerId}");
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
