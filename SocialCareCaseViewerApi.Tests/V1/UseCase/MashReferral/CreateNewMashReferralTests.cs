using System;
using System.Configuration;
using Moq;
using NUnit.Framework;
using SocialCareCaseViewerApi.Tests.V1.Helpers;
using SocialCareCaseViewerApi.V1.Gateways.Interfaces;
using SocialCareCaseViewerApi.V1.Helpers;
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
        private Mock<ISystemTime> _systemTime;
        private MashReferralUseCase _mashReferralUseCase;
        private DateTime _dateTime;

        [SetUp]
        public void Setup()
        {
            _mashReferralGateway = new Mock<IMashReferralGateway>();
            _databaseGateway = new Mock<IDatabaseGateway>();
            _systemTime = new Mock<ISystemTime>();
            _mashReferralUseCase = new MashReferralUseCase(_mashReferralGateway.Object, _databaseGateway.Object, _systemTime.Object);

            _dateTime = DateTime.Now;
            _systemTime.Setup(x => x.Now).Returns(_dateTime);
        }

        [Test]
        public void CallsTheGatewayToInsertTheNewReferral()
        {
            var request = TestHelpers.CreateNewMashReferralRequest();

            _mashReferralUseCase.CreateNewMashReferral(request);

            _mashReferralGateway.Verify(x => x.InsertDocument(It.IsAny<MashReferralEntity>()), Times.Once);
        }

        [Test]
        [Ignore("Incomplete as current gateway action does not return an object, may need to update")]
        public void ReturnsAResponseWhenSuccessful()
        {
        }
    }
}
