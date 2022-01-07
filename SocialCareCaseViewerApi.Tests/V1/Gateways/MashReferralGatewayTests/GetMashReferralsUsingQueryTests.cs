using System;
using System.Linq;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using SocialCareCaseViewerApi.Tests.V1.Helpers;
using SocialCareCaseViewerApi.V1.Boundary.Requests;
using SocialCareCaseViewerApi.V1.Domain;
using SocialCareCaseViewerApi.V1.Factories;
using SocialCareCaseViewerApi.V1.Gateways;
using SocialCareCaseViewerApi.V1.Gateways.Interfaces;
using SocialCareCaseViewerApi.V1.Helpers;

#nullable enable
namespace SocialCareCaseViewerApi.Tests.V1.Gateways.MashReferralGatewayTests

{
    public class GetMashReferralsUsingQueryTests : DatabaseTests
    {
        private Mock<ISystemTime> _systemTime = null!;
        private IMashReferralGateway _mashReferralGateway = null!;

        [SetUp]
        public void Setup()
        {
            _systemTime = new Mock<ISystemTime>();
            _mashReferralGateway = new MashReferralGateway(_systemTime.Object, DatabaseContext);

            DatabaseContext.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
        }

        [Test]
        public void GetMashReferralsWithNoQueryReturnsAllMashReferrals()
        {
            var query = TestHelpers.CreateQueryMashReferral();
            query.Id = null;
            const int numberOfMashReferralsToAdd = 5;

            for (var i = 0; i < numberOfMashReferralsToAdd; i++)
            {
                MashReferralHelper.SaveMashReferralToDatabase(DatabaseContext);
            }

            var response = _mashReferralGateway.GetReferralsUsingQuery(query);

            response.ToList().Count.Should().Be(numberOfMashReferralsToAdd);
        }

        [Test]
        public void GetMashReferralsWithIdQueryReturnsReferralsMatchingId()
        {
            const int numberOfMashReferralsToAdd = 5;
            var id = -1L;
            MashReferral referral = null!;

            for (var i = 0; i < numberOfMashReferralsToAdd; i++)
            {
                var tempRef = MashReferralHelper.SaveMashReferralToDatabase(DatabaseContext);
                if (i == 0)
                {
                    referral = tempRef.ToDomain();
                    id = referral.Id;
                }
            }

            var query = TestHelpers.CreateQueryMashReferral(id);

            var response = _mashReferralGateway.GetReferralsUsingQuery(query).ToList();

            response.Count.Should().Be(1);
            response[0].Id.Should().Be(referral.Id);
        }

        [Test]
        public void ReturnsReferralsOrderedByPoliceRedAndThenPoliceAmberBeforeAnyOtherReferrals()
        {
            const string? policeRed = "Police - red";
            const string? policeAmber = "Police - amber";

            var query = new QueryMashReferrals { Id = null };

            const int numberOfMashReferralsToAdd = 5;

            for (var i = 0; i < numberOfMashReferralsToAdd; i++)
            {
                MashReferralHelper.SaveMashReferralToDatabase(DatabaseContext);
            }

            MashReferralHelper.SaveMashReferralToDatabase(DatabaseContext, referrer: policeRed);
            MashReferralHelper.SaveMashReferralToDatabase(DatabaseContext, referrer: policeAmber);
            var response = _mashReferralGateway.GetReferralsUsingQuery(query);

            var mashReferrals = response.ToList();

            mashReferrals.ToList().Count.Should().Be(7);

            var firstReferral = mashReferrals.FirstOrDefault();
            var secondReferral = mashReferrals.ElementAtOrDefault(1);

            firstReferral?.Referrer.Should().Be(policeRed);
            secondReferral?.Referrer.Should().Be(policeAmber);
        }

        [Test]
        public void ReturnsRemainingReferralsOrderedByOldestFirstImmediatelyAfterPoliceRedOrAmberReferrals()
        {
            var currentLocalTime = DateTime.Now;
            var todayWithoutMilliseconds = new DateTime(
                currentLocalTime.Year,
                currentLocalTime.Month,
                currentLocalTime.Day,
                currentLocalTime.Hour,
                currentLocalTime.Minute,
                currentLocalTime.Second,
                DateTimeKind.Local);

            var oldest = todayWithoutMilliseconds.AddHours(-12);
            var recent = todayWithoutMilliseconds.AddHours(-3);

            const int oldReferralsToAdd = 2;
            const int newerReferralsToAdd = 2;

            const string? policeRed = "Police - red";
            const string? policeAmber = "Police - amber";

            var query = new QueryMashReferrals { Id = null };

            for (var i = 0; i < newerReferralsToAdd; i++)
            {
                MashReferralHelper.SaveMashReferralToDatabase(DatabaseContext, referralCreatedAt: recent);
            }

            for (var i = 0; i < oldReferralsToAdd; i++)
            {
                MashReferralHelper.SaveMashReferralToDatabase(DatabaseContext, referralCreatedAt: oldest);
            }

            MashReferralHelper.SaveMashReferralToDatabase(DatabaseContext, referrer: policeRed);
            MashReferralHelper.SaveMashReferralToDatabase(DatabaseContext, referrer: policeAmber);

            var response = _mashReferralGateway.GetReferralsUsingQuery(query);

            var mashReferrals = response.ToList();

            mashReferrals.ToList().Count.Should().Be(6);

            var firstReferral = mashReferrals.FirstOrDefault();
            var secondReferral = mashReferrals.ElementAtOrDefault(1);
            var thirdReferral = mashReferrals.ElementAtOrDefault(2);
            var fourthReferral = mashReferrals.ElementAtOrDefault(3);
            var fifthReferral = mashReferrals.ElementAtOrDefault(4);
            var sixthReferral = mashReferrals.ElementAtOrDefault(5);

            firstReferral?.Referrer.Should().Be(policeRed);
            secondReferral?.Referrer.Should().Be(policeAmber);

            thirdReferral?.ReferralCreatedAt.Should().Be(oldest);
            fourthReferral?.ReferralCreatedAt.Should().Be(oldest);

            fifthReferral?.ReferralCreatedAt.Should().Be(recent);
            sixthReferral?.ReferralCreatedAt.Should().Be(recent);
        }
    }
}
