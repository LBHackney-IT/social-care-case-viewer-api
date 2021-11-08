using Bogus;
using Moq;
using NUnit.Framework;
using SocialCareCaseViewerApi.Tests.V1.Helpers;
using SocialCareCaseViewerApi.V1.Gateways;
using SocialCareCaseViewerApi.V1.Gateways.Interfaces;

#nullable enable
namespace SocialCareCaseViewerApi.Tests.V1.Gateways.MashReferralGatewayTests
{
    [TestFixture]
    public class UpsertRecordTests
    {
        private Mock<IMongoGateway> _mongoGateway = null!;
        private IMashReferralGateway _mashReferralGateway = null!;
        private const string CollectionName = "mash-referrals";

        [SetUp]
        public void Setup()
        {
            _mongoGateway = new Mock<IMongoGateway>();
            _mashReferralGateway = new MashReferralGateway(_mongoGateway.Object);
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
