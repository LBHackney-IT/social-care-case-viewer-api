using FluentAssertions;
using Newtonsoft.Json;
using NUnit.Framework;
using SocialCareCaseViewerApi.Tests.V1.Helpers;
using SocialCareCaseViewerApi.V1.Boundary.Response;
using SocialCareCaseViewerApi.V1.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace SocialCareCaseViewerApi.Tests.V1.IntegrationTests.CaseStatus.LAC
{
    [TestFixture]
    public class EndStatus : IntegrationTestSetup<Startup>
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
        public async Task EndLACCaseStatusThatHasScheduledAnswers()
        {
            //create new LAC case status, start date 12/01/2000
            var postUri = new Uri($"api/v1/residents/{_person.Id}/case-statuses", UriKind.Relative);

            var answers = CaseStatusHelper.CreateCaseStatusRequestAnswers(min: 2, max: 2);

            var request = CaseStatusHelper.CreateCaseStatusRequest(
                personId: _person.Id,
                type: "LAC",
                answers: answers,
                startDate: new DateTime(2000, 01, 12),
                createdBy: _worker.Email
            );
            request.Notes = null;

            var requestContent = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

            var createCaseStatusResponse = await Client.PostAsync(postUri, requestContent).ConfigureAwait(true);
            createCaseStatusResponse.StatusCode.Should().Be(201);

            //Get request to check that the case status has been added
            var getUri = new Uri($"api/v1/residents/{_person.Id}/case-statuses", UriKind.Relative);
            var getCaseStatusesResponse = await Client.GetAsync(getUri).ConfigureAwait(true);

            getCaseStatusesResponse.StatusCode.Should().Be(200);

            var addedContent = await getCaseStatusesResponse.Content.ReadAsStringAsync().ConfigureAwait(true);
            var addedCaseStatusResponse = JsonConvert.DeserializeObject<List<CaseStatusResponse>>(addedContent).ToList();

            addedCaseStatusResponse.Count.Should().Be(1);
            addedCaseStatusResponse.Single().Answers.Count.Should().Be(2);
            addedCaseStatusResponse.Single().EndDate.Should().BeNull();
            addedCaseStatusResponse.Single().Notes.Should().BeNull();
            addedCaseStatusResponse.Single().StartDate.Should().Be(request.StartDate.ToString("O"));
            addedCaseStatusResponse.Single().Type.Should().Be(request.Type);

            //patch request to update the start date to 11/01/2000
            var caseStatusId = addedCaseStatusResponse.First().Id;

            var patchUri = new Uri($"api/v1/case-statuses/{caseStatusId}", UriKind.Relative);

            var patchRequest = TestHelpers.CreateUpdateCaseStatusRequest(startDate: new DateTime(2000, 01, 11), email: _worker.Email, caseStatusId: caseStatusId, min: 2, max: 2);
            patchRequest.Notes = null;
            patchRequest.EndDate = null;

            var patchRequestContent = new StringContent(JsonSerializer.Serialize(patchRequest), Encoding.UTF8, "application/json");

            var patchStatusResponse = await Client.PatchAsync(patchUri, patchRequestContent).ConfigureAwait(true);

            patchStatusResponse.StatusCode.Should().Be(200);

            //Get request to check that the case status was update
            var getCaseStatusesResponseAfterUpdate = await Client.GetAsync(getUri).ConfigureAwait(true);

            getCaseStatusesResponseAfterUpdate.StatusCode.Should().Be(200);

            var updateContent = await getCaseStatusesResponseAfterUpdate.Content.ReadAsStringAsync().ConfigureAwait(true);
            var updatedCaseStatusResponse = JsonConvert.DeserializeObject<List<CaseStatusResponse>>(updateContent).ToList();

            updatedCaseStatusResponse.Count.Should().Be(1);
            updatedCaseStatusResponse.Single().Answers.Count.Should().Be(2);
            updatedCaseStatusResponse.Single().EndDate.Should().BeNull();
            updatedCaseStatusResponse.Single().Notes.Should().BeNull();
            updatedCaseStatusResponse.Single().StartDate.Should().Be(patchRequest.StartDate?.ToString("O"));
            updatedCaseStatusResponse.Single().Type.Should().Be(request.Type);

            //add new scheduled answer
            var postScheduledAnswersUri = new Uri($"api/v1/case-statuses/{caseStatusId}/answers", UriKind.Relative);

            var addScheduledAnswersRequest = CaseStatusHelper.CreateCaseStatusAnswerRequest(
                caseStatusId: caseStatusId,
                startDate: new DateTime(2040, 02, 01),
                createdBy: _worker.Email
            );
            request.Notes = null;

            var scheduledAnswersRequestContent = new StringContent(JsonSerializer.Serialize(addScheduledAnswersRequest), Encoding.UTF8, "application/json");

            var createScheduledAnswersResponse = await Client.PostAsync(postScheduledAnswersUri, scheduledAnswersRequestContent).ConfigureAwait(true);
            createScheduledAnswersResponse.StatusCode.Should().Be(201);

            //patch case status to end it
            var endRequest = TestHelpers.CreateUpdateCaseStatusRequest(endDate: new DateTime(2000, 01, 11), email: _worker.Email, caseStatusId: caseStatusId, min: 1, max: 1);
            patchRequest.Notes = null;
            patchRequest.StartDate = null;

            var serialisedEndRequest = JsonSerializer.Serialize(endRequest);
            var endRequestContent = new StringContent(serialisedEndRequest, Encoding.UTF8, "application/json");

            var endStatusResponse = await Client.PatchAsync(patchUri, endRequestContent).ConfigureAwait(true);

            endStatusResponse.StatusCode.Should().Be(200);

            //get request to check that the case has been closed (end point only returns active ones at the moment)
            var getCaseStatusesResponseAfterEnd = await Client.GetAsync(getUri).ConfigureAwait(true);

            getCaseStatusesResponseAfterEnd.StatusCode.Should().Be(200);

            var contentAfterContent = await getCaseStatusesResponseAfterEnd.Content.ReadAsStringAsync().ConfigureAwait(true);
            var endCaseStatusResponse = JsonConvert.DeserializeObject<List<CaseStatusResponse>>(contentAfterContent).ToList();

            endCaseStatusResponse.Count.Should().Be(0);
        }
    }
}
