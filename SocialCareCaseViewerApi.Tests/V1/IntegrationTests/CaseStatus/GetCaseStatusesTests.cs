using FluentAssertions;
using Newtonsoft.Json;
using NUnit.Framework;
using SocialCareCaseViewerApi.V1.Boundary.Response;
using SocialCareCaseViewerApi.V1.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CaseStatusInfrasructure = SocialCareCaseViewerApi.V1.Infrastructure.CaseStatus;

namespace SocialCareCaseViewerApi.Tests.V1.IntegrationTests.CaseStatus
{
    [TestFixture]
    public class GetCaseStatusesTests : IntegrationTestSetup<Startup>
    {
        private Person _person;
        private List<CaseStatusInfrasructure> _caseStatuses;

        [SetUp]
        public void SetUp()
        {
            (_caseStatuses, _person) = IntegrationTestHelpers.SavePersonWithMultipleCaseStatusesToDatabase(DatabaseContext, addClosedCaseStatuses: true);
        }

        [Test]
        public async Task ReturnsOnlyActiveCasesWhenIncludeClosedCasesFlagIsFalse()
        {
            var getUri = new Uri($"api/v1/residents/{_person.Id}/case-statuses", UriKind.Relative);
            var getCaseStatusesResponse = await Client.GetAsync(getUri).ConfigureAwait(true);

            getCaseStatusesResponse.StatusCode.Should().Be(200);

            var addedContent = await getCaseStatusesResponse.Content.ReadAsStringAsync().ConfigureAwait(true);
            var addedCaseStatusResponse = JsonConvert.DeserializeObject<List<CaseStatusResponse>>(addedContent).ToList();

            addedCaseStatusResponse.Count.Should().Be(_caseStatuses.Where(x => x.EndDate == null).Count());
            addedCaseStatusResponse.All(x => x.EndDate == null).Should().BeTrue();
        }

        [Test]
        public async Task ReturnsAllCasesWhenIncludeClosedCasesFlagIsTrue()
        {
            var getUri = new Uri($"api/v1/residents/{_person.Id}/case-statuses?include_closed_cases=true", UriKind.Relative);
            var getCaseStatusesResponse = await Client.GetAsync(getUri).ConfigureAwait(true);

            getCaseStatusesResponse.StatusCode.Should().Be(200);

            var addedContent = await getCaseStatusesResponse.Content.ReadAsStringAsync().ConfigureAwait(true);
            var addedCaseStatusResponse = JsonConvert.DeserializeObject<List<CaseStatusResponse>>(addedContent).ToList();

            addedCaseStatusResponse.Count.Should().Be(_caseStatuses.Count);
        }
    }
}
