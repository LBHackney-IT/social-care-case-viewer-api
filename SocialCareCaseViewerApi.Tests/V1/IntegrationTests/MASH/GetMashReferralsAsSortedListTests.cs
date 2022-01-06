using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Newtonsoft.Json;
using NUnit.Framework;
using SocialCareCaseViewerApi.V1.Infrastructure;

namespace SocialCareCaseViewerApi.Tests.V1.IntegrationTests.MASH
{
    [TestFixture]
    public class GetMashReferralsAsSortedListTests : IntegrationTestSetup<Startup>
    {
        [SetUp]
        public void Setup()
        {
            IntegrationTestHelpers.SaveMashReferralToDatabase(DatabaseContext);
            IntegrationTestHelpers.SaveMashReferralToDatabase(DatabaseContext);
            IntegrationTestHelpers.SaveMashReferralToDatabase(DatabaseContext, "Police - red");
            IntegrationTestHelpers.SaveMashReferralToDatabase(DatabaseContext, "Police - amber");
        }

        [Test]
        public async Task SuccessfulGetReturnsListOfReferralsAtContactStageSortedByRagStatus()
        {
            var getUri = new Uri($"/api/v1/mash-referral", UriKind.Relative);
            var response = await Client.GetAsync(getUri).ConfigureAwait(true);

            var content = await response.Content.ReadAsStringAsync().ConfigureAwait(true);
            var referralList = JsonConvert.DeserializeObject<List<MashReferral>>(content).ToList();

            referralList.Count.Should().Be(4);

            var firstReferralReturned = referralList.FirstOrDefault();
            firstReferralReturned?.Referrer.Should().BeEquivalentTo("Police - red");

            var secondReferralReturned = referralList.ElementAtOrDefault(1);
            secondReferralReturned?.Referrer.Should().BeEquivalentTo("Police - amber");
        }
    }
}
