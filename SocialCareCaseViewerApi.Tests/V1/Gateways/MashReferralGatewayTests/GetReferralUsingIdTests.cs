using System;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using SocialCareCaseViewerApi.Tests.V1.Helpers;
using SocialCareCaseViewerApi.V1.Factories;
using SocialCareCaseViewerApi.V1.Gateways;
using SocialCareCaseViewerApi.V1.Gateways.Interfaces;
using SocialCareCaseViewerApi.V1.Helpers;


#nullable enable
namespace SocialCareCaseViewerApi.Tests.V1.Gateways.MashReferralGatewayTests
{
    [TestFixture]
    public class GetReferralUsingIdTests : DatabaseTests
    {
        private Mock<ISystemTime> _systemTime = null!;
        private IMashReferralGateway _mashReferralGateway = null!;

        [SetUp]
        public void Setup()
        {
            _systemTime = new Mock<ISystemTime>();
            _mashReferralGateway = new MashReferralGateway(_systemTime.Object, DatabaseContext);
        }

        [Test]
        public void GetReferralUsingIdReturnsDomainMashReferral()
        {
            const long referralId = 1L;
            var referral = MashReferralHelper.SaveMashReferralToDatabase(DatabaseContext, mashReferralId: referralId);

            for (var i = 0; i < 8; i++)
            {
                MashResidentHelper.SaveMashResidentToDatabase(DatabaseContext, referralId);
            }

            var response = _mashReferralGateway.GetReferralUsingId(referral.Id);

            response.Should().BeEquivalentTo(referral.ToDomain(),
            options =>
                {
                    options.Using<DateTime>(ctx => ctx.Subject.Should().BeCloseTo(ctx.Expectation, 1000)).WhenTypeIs<DateTime>();
                    return options;
                });
        }

        [Test]
        public void GetReferralUsingIdReturnsNullIfNoMashReferralFound()
        {
            const long nonExistentId = 123L;

            var response = _mashReferralGateway.GetReferralUsingId(nonExistentId);

            response.Should().BeNull();
        }
    }
}
