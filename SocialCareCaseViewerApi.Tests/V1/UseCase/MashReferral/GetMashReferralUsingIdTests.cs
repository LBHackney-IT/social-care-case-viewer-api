using FluentAssertions;
using Moq;
using NUnit.Framework;
using SocialCareCaseViewerApi.Tests.V1.Helpers;
using SocialCareCaseViewerApi.V1.Factories;
using SocialCareCaseViewerApi.V1.Gateways.Interfaces;
using SocialCareCaseViewerApi.V1.UseCase;
using SocialCareCaseViewerApi.V1.UseCase.Interfaces;

#nullable enable
namespace SocialCareCaseViewerApi.Tests.V1.UseCase.MashReferral
{
    [TestFixture]
    public class GetMashReferralUsingIdTests
    {
        private Mock<IMashReferralGateway> _mashReferralGateway = null!;
        private Mock<IDatabaseGateway> _databaseGateway = null!;
        private IMashReferralUseCase _mashReferralUseCase = null!;
        private readonly SocialCareCaseViewerApi.V1.Infrastructure.Worker _worker = TestHelpers.CreateWorker();

        [SetUp]
        public void Setup()
        {
            _mashReferralGateway = new Mock<IMashReferralGateway>();
            _databaseGateway = new Mock<IDatabaseGateway>();
            _mashReferralUseCase = new MashReferralUseCase(_mashReferralGateway.Object, _databaseGateway.Object);

            _databaseGateway.Setup(x => x.GetWorkerByEmail(It.IsAny<string>())).Returns(_worker);

        }

        [Test]
        public void GettingAMashReferralReturnsAMashReferralResponseObject()
        {
            var referral = TestHelpers.CreateMashReferral2().ToDomain();
            _mashReferralGateway
                .Setup(x => x.GetReferralUsingId_2(referral.Id))
                .Returns(referral);

            var response = _mashReferralUseCase.GetMashReferralUsingId(referral.Id);

            response?.Id.Should().Be(referral.Id);
            response?.Referrer.Should().BeEquivalentTo(referral.Referrer);
            response?.RequestedSupport.Should().BeEquivalentTo(referral.RequestedSupport);
            response?.ReferralCreatedAt.Should().BeEquivalentTo(referral.ReferralCreatedAt.ToString("O"));
            response?.ReferralDocumentURI.Should().BeEquivalentTo(referral.ReferralDocumentURI);
            response?.Stage.Should().BeEquivalentTo(referral.Stage);
            response?.InitialDecision.Should().BeEquivalentTo(referral.InitialDecision);
            response?.ScreeningDecision.Should().BeEquivalentTo(referral.ScreeningDecision);
            response?.ScreeningCreatedAt.Should().BeEquivalentTo(referral.ScreeningCreatedAt?.ToString("O"));
            response?.ScreeningUrgentContactRequired.Should().Be(referral.ScreeningUrgentContactRequired);
            response?.FinalDecision.Should().BeEquivalentTo(referral.FinalDecision);
        }

        [Test]
        public void NotGettingAMashReferralReturnsNull()
        {
            const long nonExistentId = 123L;
            _mashReferralGateway
                .Setup(x => x.GetReferralUsingId_2(nonExistentId));

            var response = _mashReferralUseCase.GetMashReferralUsingId(nonExistentId);

            response.Should().BeNull();
        }
    }
}
