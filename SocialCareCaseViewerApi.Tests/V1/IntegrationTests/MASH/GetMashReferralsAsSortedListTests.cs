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
        private DateTime _oldest;
        private DateTime _intermediate;
        private DateTime _recent;

        [SetUp]
        public void Setup()
        {
            var currentTime = DateTime.Now;
            _oldest = currentTime.AddHours(-20);
            _intermediate = currentTime.AddHours(-10);
            _recent = currentTime.AddHours(-1);

            IntegrationTestHelpers.SaveMashReferralToDatabase(DatabaseContext, createdTime: _recent);
            IntegrationTestHelpers.SaveMashReferralToDatabase(DatabaseContext, createdTime: _intermediate);
            IntegrationTestHelpers.SaveMashReferralToDatabase(DatabaseContext, createdTime: _oldest);
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

            referralList.Count.Should().Be(5);

            var firstReferralReturned = referralList.FirstOrDefault();
            firstReferralReturned?.Referrer.Should().BeEquivalentTo("Police - red");

            var secondReferralReturned = referralList.ElementAtOrDefault(1);
            secondReferralReturned?.Referrer.Should().BeEquivalentTo("Police - amber");
        }

        [Test]
        public async Task SuccessfulGetReturnsListOfNonPolicePriorityReferralsAtContactStageSortedByOldest()
        {
            var getUri = new Uri($"/api/v1/mash-referral", UriKind.Relative);
            var response = await Client.GetAsync(getUri).ConfigureAwait(true);

            var content = await response.Content.ReadAsStringAsync().ConfigureAwait(true);
            var referralList = JsonConvert.DeserializeObject<List<MashReferral>>(content).ToList();

            referralList.Count.Should().Be(5);

            var firstReferralReturned = referralList.FirstOrDefault();
            firstReferralReturned?.Referrer.Should().BeEquivalentTo("Police - red");

            var secondReferralReturned = referralList.ElementAtOrDefault(1);
            secondReferralReturned?.Referrer.Should().BeEquivalentTo("Police - amber");

            var thirdReferralReturned = referralList.ElementAtOrDefault(2);
            thirdReferralReturned?.ReferralCreatedAt.Should().Be(_oldest);

            var fourthReferralReturned = referralList.ElementAtOrDefault(3);
            fourthReferralReturned?.ReferralCreatedAt.Should().Be(_intermediate);

            var fifthReferralReturned = referralList.ElementAtOrDefault(4);
            fifthReferralReturned?.ReferralCreatedAt.Should().Be(_recent);
        }
    }
}
