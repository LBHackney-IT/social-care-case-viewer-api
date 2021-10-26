using Moq;
using NUnit.Framework;
using SocialCareCaseViewerApi.V1.Gateways;
using SocialCareCaseViewerApi.V1.Gateways.Interfaces;

#nullable enable
namespace SocialCareCaseViewerApi.Tests.V1.Gateways.MashReferralGatewayTests
{
    [TestFixture]
    public class ResetTests
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
        public void CallingResetWillDropTheMashReferralCollection()
        {
            _mongoGateway.Setup(x => x.DropCollection(CollectionName));

            _mashReferralGateway.Reset();

            _mongoGateway.Verify(x => x.DropCollection(CollectionName));
        }
    }
}
