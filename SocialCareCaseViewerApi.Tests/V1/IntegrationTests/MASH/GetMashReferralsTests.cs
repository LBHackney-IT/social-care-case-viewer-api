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
    public class GetMashReferralsTests : IntegrationTestSetup<Startup>
    {
        private DateTime _twentyHoursAgo;
        private DateTime _tenHoursAgo;
        private DateTime _oneHourAgo;

        [SetUp]
        public void Setup()
        {
            var currentLocalTime = DateTime.Now;
            var today = new DateTime(
                currentLocalTime.Year,
                currentLocalTime.Month,
                currentLocalTime.Day,
                currentLocalTime.Hour,
                currentLocalTime.Minute,
                currentLocalTime.Second,
                DateTimeKind.Local);

            _twentyHoursAgo = today.AddHours(-20);
            _tenHoursAgo = today.AddHours(-10);
            _oneHourAgo = today.AddHours(-1);

            IntegrationTestHelpers.SaveMashReferralToDatabase(DatabaseContext, createdTime: _oneHourAgo);
            IntegrationTestHelpers.SaveMashReferralToDatabase(DatabaseContext, createdTime: _tenHoursAgo);
            IntegrationTestHelpers.SaveMashReferralToDatabase(DatabaseContext, createdTime: _twentyHoursAgo);
            IntegrationTestHelpers.SaveMashReferralToDatabase(DatabaseContext, "Police - red");
            IntegrationTestHelpers.SaveMashReferralToDatabase(DatabaseContext, "Police - amber");
        }

        [Test]
        public async Task SuccessfulGetReturnsPoliceRedAndAmberFirstAndThenAllOtherReferralsFromOldest()
        {
            var getUri = new Uri($"/api/v1/mash-referral", UriKind.Relative);
            var response = await Client.GetAsync(getUri).ConfigureAwait(true);
            var content = await response.Content.ReadAsStringAsync().ConfigureAwait(true);

            var allMashReferrals = JsonConvert.DeserializeObject<List<MashReferral>>(content).ToList();

            allMashReferrals.Count.Should().Be(5);

            var firstReferral = allMashReferrals.FirstOrDefault();
            var secondReferral = allMashReferrals.ElementAtOrDefault(1);
            var thirdReferral = allMashReferrals.ElementAtOrDefault(2);
            var fourthReferral = allMashReferrals.ElementAtOrDefault(3);
            var fifthReferral = allMashReferrals.ElementAtOrDefault(4);

            firstReferral?.Referrer.Should().BeEquivalentTo("Police - red");
            secondReferral?.Referrer.Should().BeEquivalentTo("Police - amber");

            thirdReferral?.ReferralCreatedAt.Should().Be(_twentyHoursAgo);
            fourthReferral?.ReferralCreatedAt.Should().Be(_tenHoursAgo);
            fifthReferral?.ReferralCreatedAt.Should().Be(_oneHourAgo);
        }
    }
}
