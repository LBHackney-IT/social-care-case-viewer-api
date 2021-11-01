using FluentAssertions;
using Newtonsoft.Json;
using NUnit.Framework;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Boundary.Response;
using SocialCareCaseViewerApi.V1.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace SocialCareCaseViewerApi.Tests.V1.IntegrationTests.CaseStatus.CIN
{
    [TestFixture]
    public class CreateCaseStatus : IntegrationTestSetup<Startup>
    {
        private Person _person;
        private Worker _worker;

        [SetUp]
        public void SetUp()
        {
            _person = IntegrationTestHelpers.CreateExistingPerson(DatabaseContext, ageContext: "C");
            (_worker, _) = IntegrationTestHelpers.SetupExistingWorker(DatabaseContext);
        }

        [Test]
        public async Task AddNewCINCaseStatus()
        {
            //create new case status
            var postUri = new Uri($"api/v1/residents/{_person.Id}/case-statuses", UriKind.Relative);

            var request = new CreateCaseStatusRequest()
            {
                CreatedBy = _worker.Email,
                StartDate = DateTime.Today.AddDays(-1),
                Type = "CIN",
                PersonId = _person.Id
            };

            var serializedRequest = JsonSerializer.Serialize(request);
            var requestContent = new StringContent(serializedRequest, Encoding.UTF8, "application/json");

            var createCaseStatusResponse = await Client.PostAsync(postUri, requestContent).ConfigureAwait(true);
            createCaseStatusResponse.StatusCode.Should().Be(201);

            //Get request to check that the case status has been added
            var getUri = new Uri($"api/v1/residents/{_person.Id}/case-statuses", UriKind.Relative);
            var getCaseStatusesResponse = await Client.GetAsync(getUri).ConfigureAwait(true);

            getCaseStatusesResponse.StatusCode.Should().Be(200);

            var addedContent = await getCaseStatusesResponse.Content.ReadAsStringAsync().ConfigureAwait(true);
            var addedCaseStatusResponse = JsonConvert.DeserializeObject<List<CaseStatusResponse>>(addedContent).ToList();

            addedCaseStatusResponse.Count.Should().Be(1);
            addedCaseStatusResponse.Single().Answers.Should().BeEmpty();
            addedCaseStatusResponse.Single().EndDate.Should().BeNull();
            addedCaseStatusResponse.Single().Notes.Should().BeNull();
            addedCaseStatusResponse.Single().StartDate.Should().Be(request.StartDate.ToString("O"));
            addedCaseStatusResponse.Single().Type.Should().Be(request.Type);
        }
    }
}
