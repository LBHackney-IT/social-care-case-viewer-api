using Moq;
using NUnit.Framework;
using SocialCareCaseViewerApi.Tests.V1.Helpers;
using SocialCareCaseViewerApi.V1.Gateways.Interfaces;
using SocialCareCaseViewerApi.V1.UseCase;
using MashReferralEntity = SocialCareCaseViewerApi.V1.Infrastructure.MashReferral;
using MashReferralBoundaryResponse = SocialCareCaseViewerApi.V1.Boundary.Response.MashReferral;

namespace SocialCareCaseViewerApi.Tests.V1.UseCase.MashReferral
{
    [TestFixture]
    public class CreateNewMashReferralTests
    {
        private Mock<IMashReferralGateway> _mashReferralGateway;
        private Mock<IDatabaseGateway> _databaseGateway;
        private MashReferralUseCase _mashReferralUseCase;

        [SetUp]
        public void Setup()
        {
            _mashReferralGateway = new Mock<IMashReferralGateway>();
            _databaseGateway = new Mock<IDatabaseGateway>();
            _mashReferralUseCase = new MashReferralUseCase(_mashReferralGateway.Object, _databaseGateway.Object);
        }

        [Test]
        public void CallsTheGatewayToInsertTheNewReferral()
        {
            var request = TestHelpers.CreateNewMashReferralRequest();

            _mashReferralUseCase.CreateNewMashReferral(request);

            _mashReferralGateway.Verify(x => x.CreateReferral(request), Times.Once);
        }
    }
}
