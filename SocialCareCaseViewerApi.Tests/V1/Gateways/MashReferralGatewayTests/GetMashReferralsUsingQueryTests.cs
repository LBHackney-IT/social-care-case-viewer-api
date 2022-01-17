using System;
using System.Linq;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using SocialCareCaseViewerApi.Tests.V1.Helpers;
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
            query.WorkerEmail = null;
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
            response[0].Id.Should().Equals(referral.Id);
        }

        [Test]
        public void GetMashReferralsWithEmailQueryReturnsReferralsMatchingEmail()
        {
            const int numberOfMashReferralsToAdd = 4;
            var exisitingDbReferall = MashReferralHelper.SaveMashReferralToDatabase(DatabaseContext);
            string workerEmail = exisitingDbReferall.Worker!.Email;

            for (var i = 0; i < numberOfMashReferralsToAdd; i++)
            {
                MashReferralHelper.SaveMashReferralToDatabase(DatabaseContext);
            }

            var query = TestHelpers.CreateQueryMashReferral(null, workerEmail);
            query.Id = null;

            var response = _mashReferralGateway.GetReferralsUsingQuery(query).ToList();

            response.Count.Should().Be(1);
            response[0].Worker!.Email.Should().Equals(workerEmail);
        }
    }
}
