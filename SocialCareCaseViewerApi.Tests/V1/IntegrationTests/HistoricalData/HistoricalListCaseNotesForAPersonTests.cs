using FluentAssertions;
using Newtonsoft.Json;
using NUnit.Framework;
using SocialCareCaseViewerApi.V1.Boundary.Response;
using SocialCareCaseViewerApi.V1.Factories;
using System;
using System.Threading.Tasks;

namespace SocialCareCaseViewerApi.Tests.V1.IntegrationTests.HistoricalData
{
    [TestFixture]
    public class HistoricalListCaseNotesForAPersonTests : IntegrationTestSetup<Startup>
    {
        [Test]
        public async Task ReturnsAllCaseNotesForASpecificPerson()
        {
            var person = HistoricalE2ETestHelpers.AddPersonToDatabase(DatabaseContext);

            var caseNoteOne = HistoricalE2ETestHelpers.AddCaseNoteForASpecificPersonToDb(DatabaseContext, person.Id);
            var caseNoteTwo = HistoricalE2ETestHelpers.AddCaseNoteForASpecificPersonToDb(DatabaseContext, person.Id);
            var caseNoteThree = HistoricalE2ETestHelpers.AddCaseNoteForASpecificPersonToDb(DatabaseContext, person.Id);

            var uri = new Uri($"api/v1/casenotes/person/{person.Id}", UriKind.Relative);
            var response = Client.GetAsync(uri);

            var statusCode = response.Result.StatusCode;
            statusCode.Should().Be(200);

            var content = response.Result.Content;
            var stringContent = await content.ReadAsStringAsync().ConfigureAwait(true);
            var convertedResponse = JsonConvert.DeserializeObject<ListCaseNotesResponse>(stringContent);

            convertedResponse.CaseNotes.Count.Should().Be(3);
            convertedResponse.CaseNotes.Should().ContainEquivalentOf(caseNoteOne.ToDomain(includeNoteContent: false));
            convertedResponse.CaseNotes.Should().ContainEquivalentOf(caseNoteTwo.ToDomain(includeNoteContent: false));
            convertedResponse.CaseNotes.Should().ContainEquivalentOf(caseNoteThree.ToDomain(includeNoteContent: false));
        }
    }
}
