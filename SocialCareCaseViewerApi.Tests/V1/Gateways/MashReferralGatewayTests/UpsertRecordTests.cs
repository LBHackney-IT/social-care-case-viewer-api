using Bogus;
using Moq;
using NUnit.Framework;
using SocialCareCaseViewerApi.Tests.V1.Helpers;
using SocialCareCaseViewerApi.V1.Gateways;
using SocialCareCaseViewerApi.V1.Gateways.Interfaces;
using SocialCareCaseViewerApi.V1.Helpers;

#nullable enable
namespace SocialCareCaseViewerApi.Tests.V1.Gateways.MashReferralGatewayTests
{
    [TestFixture]
    public class UpsertRecordTests : DatabaseTests
    {
        private Mock<IMongoGateway> _mongoGateway = null!;
        private Mock<ISystemTime> _systemTime = null!;
        private IMashReferralGateway _mashReferralGateway = null!;
        private const string CollectionName = "mash-referrals";

        [SetUp]
        public void Setup()
        {
            _mongoGateway = new Mock<IMongoGateway>();
            _systemTime = new Mock<ISystemTime>();
            _mashReferralGateway = new MashReferralGateway(_mongoGateway.Object, _systemTime.Object, DatabaseContext);
        }

        [Test]
        public void UpsertingRecordCallsMongoGatewayWithCorrectCollectionNameAndReferralInformation()
        {
            var referral = TestHelpers.CreateMashReferral();

            _mashReferralGateway.UpsertRecord(referral);

            _mongoGateway.Verify(x => x.UpsertRecord(CollectionName, referral.Id, referral), Times.Once);
        }
    }
}
