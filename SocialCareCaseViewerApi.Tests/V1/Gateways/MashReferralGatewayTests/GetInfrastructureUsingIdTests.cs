using FluentAssertions;
using Moq;
using NUnit.Framework;
using SocialCareCaseViewerApi.Tests.V1.Helpers;
using SocialCareCaseViewerApi.V1.Gateways;
using SocialCareCaseViewerApi.V1.Gateways.Interfaces;
using SocialCareCaseViewerApi.V1.Helpers;
using SocialCareCaseViewerApi.V1.Infrastructure;


#nullable enable
namespace SocialCareCaseViewerApi.Tests.V1.Gateways.MashReferralGatewayTests
{
    [TestFixture]
    public class GetInfrastructureUsingIdTests : DatabaseTests
    {
        private Mock<IMongoGateway> _mongoGateway = null!;
        private Mock<IDatabaseGateway> _databaseGateway = null!;
        private Mock<ISystemTime> _systemTime = null!;
        private IMashReferralGateway _mashReferralGateway = null!;
        private const string CollectionName = "mash-referrals";

        [SetUp]
        public void Setup()
        {
            _mongoGateway = new Mock<IMongoGateway>();
            _databaseGateway = new Mock<IDatabaseGateway>();
            _systemTime = new Mock<ISystemTime>();
            _mashReferralGateway = new MashReferralGateway(_mongoGateway.Object, _databaseGateway.Object, _systemTime.Object, DatabaseContext);
        }

        [Test]
        public void GetInfrastructureUsingIdReturnsAnInfrastructureVersionOfAMashReferral()
        {
            var referral = TestHelpers.CreateMashReferral();
            _mongoGateway
                .Setup(x => x.LoadRecordById<MashReferral>(CollectionName, referral.Id))
                .Returns(referral);

            var response = _mashReferralGateway.GetInfrastructureUsingId(referral.Id.ToString());

            response.Should().BeEquivalentTo(referral);
        }
    }
}
